"""
train_phase_time_model.py

Treina uma regressão linear (Ridge, regularizada) por fase de fabrico,
usando o histórico real em product_phase + product_config + workstation
(linha) + product (modelo), e grava os coeficientes em phase_time_coefficient.

Desenho do modelo, por fase:
    tempo_fase_segundos = intercepto
                         + peso_modelo   (se não for o modelo "baseline")
                         + peso_linha    (se não for a linha "baseline")
                         + soma( peso_opção, para cada ConfigOption escolhida
                                 que não seja a opção 'isDefault' de uma
                                 categoria single-select )

Porque Ridge em vez de LinearRegression simples: com poucas amostras e
muitas variáveis possíveis, LinearRegression "decora" ruído (R² alto em
fases que não deviam ter relação nenhuma com a configuração). Ridge
penaliza isso. O R² reportado vem de validação cruzada (treino numa
parte dos dados, teste noutra), não do erro de treino — isto evita
mostrar um R² artificialmente bom.

Instalar dependências: pip install psycopg2-binary scikit-learn --break-system-packages
Correr: python train_phase_time_model.py
"""

import os
import psycopg2
from datetime import datetime
from sklearn.linear_model import RidgeCV
from sklearn.preprocessing import StandardScaler
from sklearn.pipeline import Pipeline
from sklearn.model_selection import cross_val_score, KFold
import numpy as np

DB_CONFIG = {
    "host": os.environ.get("DB_HOST", "localhost"),
    "port": int(os.environ.get("DB_PORT", "5432")),
    "dbname": os.environ.get("DB_NAME", "drivolution"),
    "user": os.environ.get("DB_USER", "drivolution"),
    "password": os.environ.get("DB_PASSWORD", "drivolution"),
}

RIDGE_ALPHAS = np.logspace(-2, 3, 50)  # gama testada automaticamente pelo RidgeCV, por fase


def main():
    conn = psycopg2.connect(**DB_CONFIG)
    cur = conn.cursor()

    try:
        phases = load_phases(cur)
        feature_options, baseline_options = load_option_features(cur)
        feature_lines, baseline_line = load_production_lines(cur)
        feature_models, baseline_model = load_models_feature(cur)

        trained_at = datetime.now()

        for phase_name, phase in phases.items():
            X, y, n = build_dataset(
                cur, phase["id"], feature_options, feature_lines, feature_models
            )

            if n < 5:
                print(f"[{phase_name}] dados insuficientes ({n} amostras), a saltar.")
                continue

            pipeline = Pipeline([
                ("scaler", StandardScaler()),
                ("ridge", RidgeCV(alphas=RIDGE_ALPHAS)),
            ])

            cv_r2 = cross_validated_r2(pipeline, X, y)

            pipeline.fit(X, y)  # treino final, com todos os dados

            intercept_original, coefs_original = unscale_coefficients(pipeline)

            save_coefficients(
                cur, phase["id"], intercept_original, coefs_original,
                feature_options, feature_lines, feature_models, trained_at
            )

            print(f"[{phase_name}] {n} amostras | R² (validação cruzada) = {cv_r2:.3f} | "
                  f"intercepto = {intercept_original:.1f}s")

        conn.commit()
        print(f"\nCoeficientes gravados (trained_at = {trained_at}).")

    except Exception:
        conn.rollback()
        raise
    finally:
        cur.close()
        conn.close()


def unscale_coefficients(pipeline):
    """
    O Ridge foi treinado em variáveis normalizadas: x_scaled = (x - mean) / std.
    Para guardar pesos em segundos, que o C# soma diretamente sobre as
    variáveis binárias originais (0/1, sem normalização), é preciso
    converter de volta:

        y = intercept_scaled + Σ coef_scaled_i * (x_i - mean_i) / std_i
          = [intercept_scaled - Σ coef_scaled_i * mean_i / std_i]   (novo intercepto)
          + Σ [coef_scaled_i / std_i] * x_i                         (novos pesos)
    """
    scaler = pipeline.named_steps["scaler"]
    ridge = pipeline.named_steps["ridge"]

    std = scaler.scale_   # já vem protegido contra divisão por zero pelo sklearn
    mean = scaler.mean_

    coefs_original = ridge.coef_ / std
    intercept_original = ridge.intercept_ - np.sum(ridge.coef_ * mean / std)

    return float(intercept_original), coefs_original


def cross_validated_r2(model, X, y):
    n_splits = min(5, len(y))
    if n_splits < 2:
        return float("nan")
    kfold = KFold(n_splits=n_splits, shuffle=True, random_state=42)
    scores = cross_val_score(model, X, y, cv=kfold, scoring="r2")
    return float(np.mean(scores))


def load_phases(cur):
    cur.execute("SELECT id, trim(name) FROM manufacturing_phase ORDER BY id;")
    return {name: {"id": pid} for pid, name in cur.fetchall()}


def load_option_features(cur):
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
    feature_lines = lines[1:]
    return feature_lines, baseline_line


def load_models_feature(cur):
    cur.execute("SELECT id FROM model ORDER BY id;")
    models = [row[0] for row in cur.fetchall()]
    baseline_model = models[0] if models else None
    feature_models = models[1:]  # todos exceto o modelo "baseline" (ex: Bravon Halo)
    return feature_models, baseline_model


def build_dataset(cur, phase_id, feature_options, feature_lines, feature_models):
    cur.execute("""
        SELECT pp.id, pp.datetime_ini, pp.datetime_end, pp.product_id,
               w.production_line_id, p.model_id
        FROM product_phase pp
        JOIN workstation w ON w.id = pp.workstation_id
        JOIN product p ON p.id = pp.product_id
        WHERE pp.manufacturing_phase_id = %s
          AND pp.datetime_end IS NOT NULL;
    """, (phase_id,))
    phase_rows = cur.fetchall()

    n = len(phase_rows)
    n_features = len(feature_options) + len(feature_lines) + len(feature_models)
    X = np.zeros((n, n_features))
    y = np.zeros(n)

    option_index = {opt_id: i for i, opt_id in enumerate(feature_options)}
    line_offset = len(feature_options)
    line_index = {line_id: line_offset + i for i, line_id in enumerate(feature_lines)}
    model_offset = line_offset + len(feature_lines)
    model_index = {model_id: model_offset + i for i, model_id in enumerate(feature_models)}

    for row_i, (pp_id, dt_ini, dt_end, product_id, line_id, model_id) in enumerate(phase_rows):
        y[row_i] = (dt_end - dt_ini).total_seconds()

        if line_id in line_index:
            X[row_i, line_index[line_id]] = 1

        if model_id in model_index:
            X[row_i, model_index[model_id]] = 1

        cur.execute(
            "SELECT config_option_id FROM product_config WHERE product_id = %s;",
            (product_id,)
        )
        for (option_id,) in cur.fetchall():
            if option_id in option_index:
                X[row_i, option_index[option_id]] = 1

    return X, y, n


def save_coefficients(cur, phase_id, intercept_original, coefs_original,
                       feature_options, feature_lines, feature_models, trained_at):
    cur.execute("DELETE FROM phase_time_coefficient WHERE manufacturing_phase_id = %s;", (phase_id,))

    cur.execute(
        "INSERT INTO phase_time_coefficient "
        "(manufacturing_phase_id, config_option_id, production_line_id, model_id, weight_seconds, trained_at) "
        "VALUES (%s, NULL, NULL, NULL, %s, %s);",
        (phase_id, intercept_original, trained_at)
    )

    coefs = coefs_original

    for i, option_id in enumerate(feature_options):
        cur.execute(
            "INSERT INTO phase_time_coefficient "
            "(manufacturing_phase_id, config_option_id, production_line_id, model_id, weight_seconds, trained_at) "
            "VALUES (%s, %s, NULL, NULL, %s, %s);",
            (phase_id, option_id, float(coefs[i]), trained_at)
        )

    line_offset = len(feature_options)
    for j, line_id in enumerate(feature_lines):
        cur.execute(
            "INSERT INTO phase_time_coefficient "
            "(manufacturing_phase_id, config_option_id, production_line_id, model_id, weight_seconds, trained_at) "
            "VALUES (%s, NULL, %s, NULL, %s, %s);",
            (phase_id, line_id, float(coefs[line_offset + j]), trained_at)
        )

    model_offset = line_offset + len(feature_lines)
    for k, mdl_id in enumerate(feature_models):
        cur.execute(
            "INSERT INTO phase_time_coefficient "
            "(manufacturing_phase_id, config_option_id, production_line_id, model_id, weight_seconds, trained_at) "
            "VALUES (%s, NULL, NULL, %s, %s, %s);",
            (phase_id, mdl_id, float(coefs[model_offset + k]), trained_at)
        )


if __name__ == "__main__":
    main()