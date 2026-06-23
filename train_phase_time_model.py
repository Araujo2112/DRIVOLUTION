"""
train_phase_time_model.py

Treina uma regressão linear por fase de fabrico, usando o histórico real
em product_phase (+ product_config + workstation/production_line), e
grava os coeficientes resultantes em phase_time_coefficient.

Desenho do modelo, por fase:
    tempo_fase_segundos = intercepto
                         + peso_linha (se não for a linha "baseline")
                         + soma( peso_opção, para cada ConfigOption escolhida
                                 que não seja a opção 'isDefault' de uma
                                 categoria single-select )

Isto é executado:
- manualmente (corre este script à mão), ou
- pelo BackgroundService em C# (semanalmente, ou via endpoint /api/ml/retrain)

Instalar dependências: pip install psycopg2-binary scikit-learn --break-system-packages
Correr: python train_phase_time_model.py
"""

import psycopg2
from datetime import datetime
from sklearn.linear_model import LinearRegression
import numpy as np

DB_CONFIG = {
    "host": "localhost",
    "port": 5433,
    "dbname": "drivolution",
    "user": "drivolution",
    "password": "drivolution",
}


def main():
    conn = psycopg2.connect(**DB_CONFIG)
    cur = conn.cursor()

    try:
        phases = load_phases(cur)
        feature_options, baseline_options = load_option_features(cur)
        lines, baseline_line = load_production_lines(cur)

        trained_at = datetime.now()

        for phase_name, phase in phases.items():
            X, y, n = build_dataset(cur, phase["id"], feature_options, lines, baseline_line)

            if n < 3:
                print(f"[{phase_name}] dados insuficientes ({n} amostras), a saltar.")
                continue

            model = LinearRegression()
            model.fit(X, y)
            r2 = model.score(X, y)

            save_coefficients(
                cur, phase["id"], model, feature_options, lines, baseline_line, trained_at
            )

            print(f"[{phase_name}] treinado com {n} amostras | R² = {r2:.3f} | "
                  f"intercepto = {model.intercept_:.1f}s")

        conn.commit()
        print(f"\nCoeficientes gravados (trained_at = {trained_at}).")

    except Exception:
        conn.rollback()
        raise
    finally:
        cur.close()
        conn.close()


def load_phases(cur):
    cur.execute("SELECT id, trim(name) FROM manufacturing_phase ORDER BY id;")
    return {name: {"id": pid} for pid, name in cur.fetchall()}


def load_option_features(cur):
    """
    Devolve:
    - feature_options: lista de config_option_id que entram como variável
      (todas as multi-select + todas as single-select EXCETO a opção default
      de cada categoria, que fica como baseline implícito no intercepto).
    - baseline_options: dict config_id -> option_id default (informativo).
    """
    cur.execute("""
        SELECT co.id, co.config_id, co.is_default, c.allow_multiple
        FROM config_option co
        JOIN config c ON c.id = co.config_id;
    """)
    rows = cur.fetchall()

    baseline_options = {}
    for option_id, config_id, is_default, allow_multiple in rows:
        if not allow_multiple and is_default:
            baseline_options[config_id] = option_id

    feature_options = []
    for option_id, config_id, is_default, allow_multiple in rows:
        if allow_multiple:
            feature_options.append(option_id)
        else:
            if baseline_options.get(config_id) != option_id:
                feature_options.append(option_id)

    return feature_options, baseline_options


def load_production_lines(cur):
    cur.execute("SELECT id FROM production_line ORDER BY id;")
    lines = [row[0] for row in cur.fetchall()]
    baseline_line = lines[0] if lines else None
    feature_lines = lines[1:]  # todas exceto a baseline
    return feature_lines, baseline_line


def build_dataset(cur, phase_id, feature_options, feature_lines, baseline_line):
    """
    Para cada product_phase desta fase, constrói uma linha de X (features
    binárias: opções escolhidas + linha) e o y correspondente (duração em
    segundos).
    """
    cur.execute("""
        SELECT pp.id, pp.datetime_ini, pp.datetime_end, pp.product_id, w.production_line_id
        FROM product_phase pp
        JOIN workstation w ON w.id = pp.workstation_id
        WHERE pp.manufacturing_phase_id = %s
          AND pp.datetime_end IS NOT NULL;
    """, (phase_id,))
    phase_rows = cur.fetchall()

    n = len(phase_rows)
    n_features = len(feature_options) + len(feature_lines)
    X = np.zeros((n, n_features))
    y = np.zeros(n)

    option_index = {opt_id: i for i, opt_id in enumerate(feature_options)}
    line_index = {line_id: len(feature_options) + i for i, line_id in enumerate(feature_lines)}

    for row_i, (pp_id, dt_ini, dt_end, product_id, line_id) in enumerate(phase_rows):
        y[row_i] = (dt_end - dt_ini).total_seconds()

        if line_id in line_index:
            X[row_i, line_index[line_id]] = 1

        cur.execute(
            "SELECT config_option_id FROM product_config WHERE product_id = %s;",
            (product_id,)
        )
        for (option_id,) in cur.fetchall():
            if option_id in option_index:
                X[row_i, option_index[option_id]] = 1

    return X, y, n


def save_coefficients(cur, phase_id, model, feature_options, feature_lines, baseline_line, trained_at):
    cur.execute("DELETE FROM phase_time_coefficient WHERE manufacturing_phase_id = %s;", (phase_id,))

    # Intercepto (config_option_id e production_line_id NULL)
    cur.execute(
        "INSERT INTO phase_time_coefficient "
        "(manufacturing_phase_id, config_option_id, production_line_id, weight_seconds, trained_at) "
        "VALUES (%s, NULL, NULL, %s, %s);",
        (phase_id, float(model.intercept_), trained_at)
    )

    coefs = model.coef_
    for i, option_id in enumerate(feature_options):
        cur.execute(
            "INSERT INTO phase_time_coefficient "
            "(manufacturing_phase_id, config_option_id, production_line_id, weight_seconds, trained_at) "
            "VALUES (%s, %s, NULL, %s, %s);",
            (phase_id, option_id, float(coefs[i]), trained_at)
        )

    offset = len(feature_options)
    for j, line_id in enumerate(feature_lines):
        cur.execute(
            "INSERT INTO phase_time_coefficient "
            "(manufacturing_phase_id, config_option_id, production_line_id, weight_seconds, trained_at) "
            "VALUES (%s, NULL, %s, %s, %s);",
            (phase_id, line_id, float(coefs[offset + j]), trained_at)
        )


if __name__ == "__main__":
    main()