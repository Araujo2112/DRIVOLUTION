"""
seed_synthetic_history.py

Gera histórico SINTÉTICO de produtos já "concluídos", para arrancar o
modelo de regressão de previsão de tempo (cold start).

Agora inclui um delta de BASE por modelo/fase (independente das opções
escolhidas), para refletir que carros maiores (ex: SUV) demoram sempre
mais tempo nalgumas fases do que carros pequenos, mesmo com a mesma
configuração.

NÃO usa a API — insere diretamente na base de dados via psycopg2.

Instalar dependência: pip install psycopg2-binary --break-system-packages
Correr: python seed_synthetic_history.py
"""

import json
import random
import psycopg2
import os
from datetime import datetime, timedelta

DB_CONFIG = {
    "host": "localhost",
    "port": 5433,
    "dbname": "drivolution",
    "user": "drivolution",
    "password": "drivolution",
}

PRODUCTS_PER_MODEL = 200
ACCESSORY_PROB = 0.35
NOISE_STD_PCT = 0.15
HANDOVER_MIN_MINUTES = 5
HANDOVER_MAX_MINUTES = 25
ORDER_SPREAD_DAYS = 60

# Durações BASE realistas (segundos), usadas para gerar o histórico sintético.
# Propositadamente independentes de manufacturing_phase.estimated_duration na BD —
# o histórico de treino do ML reflete a realidade de fábrica, não o valor
# administrativo usado só para o alerta de tempo excedido.
# Referência: proporção real de fábricas de grande volume (pintura ~metade do
# tempo total de produção; estampagem/soldadura mais rápidas; ver CSV de
# referência partilhado à parte).
REALISTIC_PHASE_DURATIONS_SECONDS = {
    "Estampagem": 20 * 60,
    "Soldadura": 40 * 60,
    "Pintura": 90 * 60,
    "Montagem": 60 * 60,
    "Inspeção": 30 * 60,
}

DELTAS_FILE = os.path.join(os.path.dirname(__file__), "time-deltas-by-option-phase.json")

random.seed(42)


def print_coverage_report(model_name, configs):
    """Mostra quantas vezes cada ConfigOption deste modelo vai
    aparecer nos dados sintéticos, dado o PRODUCTS_PER_MODEL atual.
    Regra prática: <15 amostras é fraco para o Ridge estimar um peso
    fiável para essa opção; 25+ já é sólido."""
    print(f"   Cobertura esperada por opção (modelo: {model_name}, {PRODUCTS_PER_MODEL} produtos):")
    for item, cfg in configs.items():
        n_options = len(cfg["options"])
        if not n_options:
            continue

        if cfg["allow_multiple"]:
            expected = PRODUCTS_PER_MODEL * ACCESSORY_PROB
            kind = "multi-select"
        else:
            expected = PRODUCTS_PER_MODEL / n_options
            kind = "single-select"

        flag = "⚠ fraco" if expected < 15 else ("ok" if expected < 25 else "✓ sólido")
        print(f"     - {item} ({kind}, {n_options} opções): ~{expected:.0f} amostras/opção [{flag}]")


def main():
    with open(DELTAS_FILE, "r", encoding="utf-8") as f:
        deltas = json.load(f)
    deltas.pop("_comentario", None)
    model_base_deltas = deltas.pop("_model_base_deltas", {})

    conn = psycopg2.connect(**DB_CONFIG)
    conn.autocommit = False
    cur = conn.cursor()

    try:
        models = load_models(cur)
        phases = load_phases(cur)
        workstations = load_workstations(cur)
        lines = load_production_lines(cur)

        ensure_phase_sequences(cur, models, phases)

        client_user_id = ensure_test_client_user(cur)
        print(f"✓ Conta de cliente de teste (app_user_id={client_user_id})")

        for model_name, model_id in models.items():
            if model_name not in deltas:
                print(f"  [aviso] sem deltas definidos para '{model_name}', a saltar.")
                continue
            configs = load_configs_with_options(cur, model_id)
            base_deltas = model_base_deltas.get(model_name, {})
            print(f"\n=== Gerando histórico para {model_name} (id={model_id}) ===")
            print_coverage_report(model_name, configs)
            create_history_for_model(
                cur, model_name, model_id, configs, deltas[model_name], base_deltas,
                phases, workstations, lines, client_user_id
            )

        conn.commit()
        print("\nConcluído e guardado na base de dados.")
    except Exception:
        conn.rollback()
        raise
    finally:
        cur.close()
        conn.close()


def load_models(cur):
    cur.execute("SELECT id, name FROM model ORDER BY id;")
    return {name: mid for mid, name in cur.fetchall()}


def ensure_test_client_user(cur):
    """
    Garante uma conta app_user com role='client' para usar como app_user_id
    nas client_order sintéticas. Reutiliza se já existir (ON CONFLICT por email).
    Hash de password fixo e inválido — esta conta nunca faz login real.
    """
    cur.execute(
        "INSERT INTO app_user (name, email, password_hash, role, status, must_change_password) "
        "VALUES (%s, %s, %s, 'client', 'active', false) "
        "ON CONFLICT (email) DO UPDATE SET email = EXCLUDED.email "
        "RETURNING id;",
        ("Cliente de Teste (seed)", "seed-client@drivolution.test", "SEED_ACCOUNT_NO_LOGIN")
    )
    return cur.fetchone()[0]


def load_phases(cur):
    cur.execute("SELECT id, trim(name), estimated_duration FROM manufacturing_phase ORDER BY id;")
    return {name: {"id": pid, "duration": dur} for pid, name, dur in cur.fetchall()}


def load_workstations(cur):
    cur.execute("SELECT id, manufacturing_phase_id, production_line_id FROM workstation;")
    result = {}
    for ws_id, phase_id, line_id in cur.fetchall():
        result[(phase_id, line_id)] = ws_id
    return result


def load_production_lines(cur):
    cur.execute("SELECT id FROM production_line ORDER BY id;")
    return [row[0] for row in cur.fetchall()]


def load_configs_with_options(cur, model_id):
    cur.execute("SELECT id, item, allow_multiple FROM config WHERE model_id = %s;", (model_id,))
    configs = {}
    for cid, item, allow_multiple in cur.fetchall():
        cur.execute("SELECT id, value, is_default FROM config_option WHERE config_id = %s;", (cid,))
        options = [{"id": oid, "value": value, "default": is_default}
                   for oid, value, is_default in cur.fetchall()]
        configs[item] = {"id": cid, "allow_multiple": allow_multiple, "options": options}
    return configs


def ensure_phase_sequences(cur, models, phases):
    phase_order = ["Estampagem", "Soldadura", "Pintura", "Montagem", "Inspeção"]
    for model_id in models.values():
        cur.execute("SELECT COUNT(*) FROM phase_sequence WHERE model_id = %s;", (model_id,))
        if cur.fetchone()[0] > 0:
            continue
        for order, phase_name in enumerate(phase_order, start=1):
            cur.execute(
                'INSERT INTO phase_sequence ("order", manufacturing_phase_id, model_id) VALUES (%s, %s, %s);',
                (order, phases[phase_name]["id"], model_id)
            )


def pick_config_selections(configs):
    selections = {}
    for item, cfg in configs.items():
        if cfg["allow_multiple"]:
            chosen = [opt for opt in cfg["options"] if random.random() < ACCESSORY_PROB]
            selections[item] = chosen
        else:
            selections[item] = [random.choice(cfg["options"])]
    return selections


def lookup_delta_minutes(model_deltas, config_item, option_value, phase_name):
    entries = model_deltas.get(config_item, {}).get(option_value, [])
    for entry in entries:
        if entry["phase"] == phase_name:
            return entry["deltaMinutes"]
    return 0


def create_history_for_model(cur, model_name, model_id, configs, model_deltas, base_deltas,
                              phases, workstations, lines, client_user_id):
    order_number = f"SEED-{model_id}-{random.randint(1000, 9999)}"
    order_date = datetime.utcnow() - timedelta(days=random.randint(1, ORDER_SPREAD_DAYS))

    cur.execute(
        "INSERT INTO client_order (order_number, order_date, app_user_id, quantity) "
        "VALUES (%s, %s, %s, %s) RETURNING id;",
        (order_number, order_date, client_user_id, PRODUCTS_PER_MODEL)
    )
    client_order_id = cur.fetchone()[0]

    phase_order = ["Estampagem", "Soldadura", "Pintura", "Montagem", "Inspeção"]

    for i in range(1, PRODUCTS_PER_MODEL + 1):
        mo_number = f"{order_number}-MO-{i:03d}"
        mo_start = order_date + timedelta(hours=random.randint(0, 48))

        cur.execute(
            "INSERT INTO manufacturing_order (client_order_id, manufacturing_order_number, "
            "start_date, end_date, status) VALUES (%s, %s, %s, %s, %s) RETURNING id;",
            (client_order_id, mo_number, mo_start, None, "pending")
        )
        mo_id = cur.fetchone()[0]

        serial = f"VIN-SEED-{model_id}-{i:03d}-{random.randint(10000,99999)}"
        cur.execute(
            "INSERT INTO product (manufacturing_order_id, model_id, serial_number, lot_number, "
            "color_code, production_date) VALUES (%s, %s, %s, %s, %s, %s) RETURNING id;",
            (mo_id, model_id, serial, order_number, None, None)
        )
        product_id = cur.fetchone()[0]

        selections = pick_config_selections(configs)
        for item, chosen_options in selections.items():
            for opt in chosen_options:
                cur.execute(
                    "INSERT INTO product_config (product_id, config_option_id) VALUES (%s, %s);",
                    (product_id, opt["id"])
                )

        line_id = random.choice(lines)
        current_time = mo_start

        for phase_name in phase_order:
            phase = phases[phase_name]
            base_seconds = REALISTIC_PHASE_DURATIONS_SECONDS.get(phase_name, phase["duration"] or 1800)

            delta_minutes_total = base_deltas.get(phase_name, 0)
            for item, chosen_options in selections.items():
                for opt in chosen_options:
                    delta_minutes_total += lookup_delta_minutes(
                        model_deltas, item, opt["value"], phase_name
                    )

            expected_seconds = base_seconds + delta_minutes_total * 60
            actual_seconds = int(random.gauss(expected_seconds, expected_seconds * NOISE_STD_PCT))
            actual_seconds = max(actual_seconds, int(expected_seconds * 0.5))

            handover = timedelta(minutes=random.randint(HANDOVER_MIN_MINUTES, HANDOVER_MAX_MINUTES))
            datetime_ini = current_time + handover
            datetime_end = datetime_ini + timedelta(seconds=actual_seconds)

            ws_id = workstations.get((phase["id"], line_id))
            if ws_id is None:
                raise RuntimeError(
                    f"Sem workstation para fase '{phase_name}' na linha {line_id}."
                )

            cur.execute(
                "INSERT INTO product_phase (notes, result, datetime_ini, datetime_end, "
                "manufacturing_phase_id, product_id, workstation_id, quality_id) "
                "VALUES (%s, %s, %s, %s, %s, %s, %s, %s);",
                (
                    "Dados sintéticos (seed para ML)", "Aprovado",
                    datetime_ini, datetime_end,
                    phase["id"], product_id, ws_id, None
                )
            )

            current_time = datetime_end

        cur.execute(
            "UPDATE manufacturing_order SET end_date = %s, status = %s WHERE id = %s;",
            (current_time, "completed", mo_id)
        )
        cur.execute(
            "UPDATE product SET production_date = %s WHERE id = %s;",
            (current_time, product_id)
        )

        print(f"  Produto {serial} | linha {line_id} | concluído em {current_time}")


if __name__ == "__main__":
    main()