"""
train_phase_time_model.py

Treina modelos Ridge por fase de fabrico e guarda tudo num ficheiro pickle
para o microserviço ml-service usar no endpoint POST /predict.
"""

import os
import pickle
import psycopg2
from datetime import datetime
from pathlib import Path

import numpy as np
from sklearn.linear_model import RidgeCV
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler
from sklearn.model_selection import cross_val_score, KFold


DB_CONFIG = {
    "host": os.environ.get("DB_HOST", "localhost"),
    "port": int(os.environ.get("DB_PORT", "5433")),
    "dbname": os.environ.get("DB_NAME", "drivolution"),
    "user": os.environ.get("DB_USER", "drivolution"),
    "password": os.environ.get("DB_PASSWORD", "drivolution"),
}

MODEL_OUTPUT_PATH = os.environ.get(
    "MODEL_OUTPUT_PATH",
    "ml-service/phase_time_model.pkl"
)

RIDGE_ALPHAS = np.logspace(-2, 3, 50)


def main():
    conn = psycopg2.connect(**DB_CONFIG)
    cur = conn.cursor()

    try:
        phases = load_phases(cur)
        feature_options, baseline_options = load_option_features(cur)
        feature_lines, baseline_line = load_production_lines(cur)
        feature_models, baseline_model = load_models_feature(cur)

        trained_at = datetime.now()
        phase_models = {}

        for phase_name, phase in phases.items():
            X, y, n = build_dataset(
                cur,
                phase["id"],
                feature_options,
                feature_lines,
                feature_models
            )

            if n < 5:
                print(f"[{phase_name}] dados insuficientes ({n} amostras), a saltar.")
                continue

            pipeline = Pipeline([
                ("scaler", StandardScaler()),
                ("ridge", RidgeCV(alphas=RIDGE_ALPHAS)),
            ])

            cv_r2 = cross_validated_r2(pipeline, X, y)
            pipeline.fit(X, y)

            phase_models[phase["id"]] = {
                "phase_name": phase_name,
                "pipeline": pipeline,
                "samples": n,
                "cv_r2": cv_r2,
            }

            print(
                f"[{phase_name}] {n} amostras | "
                f"R² validação cruzada = {cv_r2:.3f}"
            )

        model_package = {
            "trained_at": trained_at.isoformat(),
            "feature_options": feature_options,
            "feature_lines": feature_lines,
            "feature_models": feature_models,
            "baseline_options": baseline_options,
            "baseline_line": baseline_line,
            "baseline_model": baseline_model,
            "phase_models": phase_models,
        }

        output_path = Path(MODEL_OUTPUT_PATH)
        output_path.parent.mkdir(parents=True, exist_ok=True)

        with open(output_path, "wb") as f:
            pickle.dump(model_package, f)

        print(f"\nModelo pickle guardado em: {output_path}")
        print(f"Fases treinadas: {len(phase_models)}")
        print(f"trained_at = {trained_at}")

    finally:
        cur.close()
        conn.close()


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
        elif baseline_options.get(config_id) != option_id:
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
    feature_models = models[1:]
    return feature_models, baseline_model


def build_dataset(cur, phase_id, feature_options, feature_lines, feature_models):
    cur.execute("""
        SELECT pp.id,
               pp.datetime_ini,
               pp.datetime_end,
               pp.product_id,
               w.production_line_id,
               p.model_id
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
    line_index = {
        line_id: line_offset + i
        for i, line_id in enumerate(feature_lines)
    }

    model_offset = line_offset + len(feature_lines)
    model_index = {
        model_id: model_offset + i
        for i, model_id in enumerate(feature_models)
    }

    for row_i, (_, dt_ini, dt_end, product_id, line_id, model_id) in enumerate(phase_rows):
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


if __name__ == "__main__":
    main()