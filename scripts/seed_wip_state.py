"""
seed_wip_state.py

Popula a BD com dados WIP para testar o WIP Dashboard (Tabelas, Kanban, Grafo)
e os endpoints de previsão de ETA (precisam de produtos AINDA EM CURSO,
não concluídos — diferente do seed_synthetic_history.py, que só gera
histórico já fechado).

Cria produtos em diferentes estados:
  - Fila de espera sem skid (waiting_support)
  - Com skid mas sem localização ativa (waiting_line)
  - Em curso em diferentes workstations (in_progress) — alguns atrasados
  - Alguns já completados (para KPI "Concluídos")

NÃO toca nos produtos sintéticos já existentes (VIN-SEED-*).
Cria uma nova encomenda de teste: ORD-WIP-TEST.

Correr: python seed_wip_state.py
Dependência: pip install psycopg2-binary --break-system-packages
"""

import psycopg2
import random
from datetime import datetime, timedelta

DB_CONFIG = {
    "host": "localhost",
    "port": 5433,
    "dbname": "drivolution",
    "user": "drivolution",
    "password": "drivolution",
}

random.seed(99)

# Workstations por fase (confirmado na BD)
# Linha 1: ws 1(Estampagem), 2(Soldadura), 3(Pintura), 7(Montagem), 8(Inspeção)
# Linha 2: ws 9(Estampagem), 10(Soldadura), 11(Pintura), 12(Montagem), 13(Inspeção)
PHASE_WS = {
    1: {1: 1, 2: 6},   # Estampagem: linha1→ws1, linha2→ws6
    2: {1: 2, 2: 7},   # Soldadura
    3: {1: 3, 2: 8},   # Pintura
    4: {1: 4, 2: 9},   # Montagem
    5: {1: 5, 2: 10},  # Inspeção
}

# Suportes existentes na BD (confirmado pelo fiware_setup.py)
SUPPORT_RFID_TAGS = [
    "3542100258",
    "3220140258",
    "3542100123",
    "35408750123",
    "35408750000",
    "35408750111",
    "35408750843",
]


def main():
    conn = psycopg2.connect(**DB_CONFIG)
    conn.autocommit = False
    cur = conn.cursor()

    try:
        print("=== seed_wip_state.py ===\n")

        models = load_models(cur)
        phases = load_phases(cur)
        supports = load_supports(cur)

        if not supports:
            print("❌ Sem suportes na BD. Cria suportes no dashboard primeiro.")
            return

        print(f"✓ {len(models)} modelos | {len(phases)} fases | {len(supports)} suportes")

        ensure_phase_sequences(cur, models, phases)

        now = datetime.now()
        cur.execute(
            "INSERT INTO client_order (order_number, order_date, customer_name, quantity) "
            "VALUES (%s, %s, %s, %s) RETURNING id;",
            ("ORD-WIP-TEST", now - timedelta(days=2), "WIP Test Client", 20)
        )
        client_order_id = cur.fetchone()[0]
        print(f"✓ ClientOrder criada (id={client_order_id})")

        run_suffix = random.randint(1000, 9999)

        model_ids = list(models.values())
        support_ids = [s["id"] for s in supports]
        phase_ids_ordered = sorted(phases.keys())  # [1,2,3,4,5]

        products_created = []

        # ── Grupo 1: 5 produtos SEM SKID (waiting_support) ──────────────────
        print("\n[1/5] A criar produtos sem skid (waiting_support)...")
        for i in range(1, 6):
            mo_id, product_id = create_mo_and_product(
                cur, client_order_id, f"ORD-WIP-TEST-MO-NOSK-{i:02d}",
                random.choice(model_ids), now, i
            )
            products_created.append({"id": product_id, "state": "waiting_support"})
            print(f"   VIN-WIP-NOSK-{i:02d} (id={product_id}) — sem skid")

        # ── Grupo 2: 2 produtos COM SKID mas sem localização (waiting_line) ──
        print("\n[2/5] A criar produtos com skid mas sem localização (waiting_line)...")
        skids_for_waiting_line = support_ids[:2]
        for i, support_id in enumerate(skids_for_waiting_line, start=1):
            mo_id, product_id = create_mo_and_product(
                cur, client_order_id, f"ORD-WIP-TEST-MO-WLINE-{i:02d}",
                random.choice(model_ids), now, 5 + i
            )
            cur.execute(
                "INSERT INTO supported_product (support_id, product_id, datetime_ini) "
                "VALUES (%s, %s, %s);",
                (support_id, product_id, now - timedelta(hours=1))
            )
            products_created.append({"id": product_id, "state": "waiting_line"})
            print(f"   VIN-WIP-WLINE-{i:02d} (id={product_id}) — skid {support_id}, sem localização")

        # ── Grupo 3: produtos EM CURSO em diferentes workstations ────────────
        print("\n[3/5] A criar produtos em curso em diferentes fases...")

        # Produto na fase 1 (Estampagem) - linha 1, ws 1 — normal
        p_id, _ = create_in_progress(
            cur, client_order_id, f"MO-INP-01-{run_suffix}", random.choice(model_ids), now,
            phase_id=phase_ids_ordered[0], ws_id=1, line_id=1,
            support_id=support_ids[2],
            started_ago_hours=0.5  # 30 min — dentro do tempo (estimated=1800s)
        )
        print(f"   id={p_id} — Estampagem ws1 (normal, 30min)")

        # Produto na fase 2 (Soldadura) - linha 1, ws 2 — normal
        p_id, _ = create_in_progress(
            cur, client_order_id, f"MO-INP-02-{run_suffix}", random.choice(model_ids), now,
            phase_id=phase_ids_ordered[1], ws_id=2, line_id=1,
            support_id=support_ids[3],
            started_ago_hours=0.7  # 42 min — dentro do tempo (estimated=2700s=45min)
        )
        print(f"   id={p_id} — Soldadura ws2 (normal, 42min)")

        # Produto na fase 3 (Pintura) - linha 1, ws 3 — WARNING (>100% estimated)
        p_id, _ = create_in_progress(
            cur, client_order_id, f"MO-INP-03-{run_suffix}", random.choice(model_ids), now,
            phase_id=phase_ids_ordered[2], ws_id=3, line_id=1,
            support_id=support_ids[4],
            started_ago_hours=1.2  # 72 min — excede estimated 30min → warning
        )
        print(f"   id={p_id} — Pintura ws3 (WARNING, 72min > 30min estimado)")

        # Produto na fase 4 (Montagem) - linha 2, ws 12 — normal
        p_id, _ = create_in_progress(
            cur, client_order_id, f"MO-INP-04-{run_suffix}", random.choice(model_ids), now,
            phase_id=phase_ids_ordered[3], ws_id=9, line_id=2,
            support_id=support_ids[5],
            started_ago_hours=0.3
        )
        print(f"   id={p_id} — Montagem ws12 linha2 (normal, 18min)")

        # Produto na fase 5 (Inspeção) - linha 2, ws 13 — CRITICAL (cria alerta)
        p_id_critical, pp_id_critical = create_in_progress(
            cur, client_order_id, f"MO-INP-05-{run_suffix}", random.choice(model_ids), now,
            phase_id=phase_ids_ordered[4], ws_id=10, line_id=2,
            support_id=support_ids[6],
            started_ago_hours=2.0  # 120 min — excede 150% de 30min (45min) → critical
        )
        print(f"   id={p_id_critical} — Inspeção ws13 linha2 (CRITICAL, 120min > 45min threshold)")

        # Produto na fase 1 (Estampagem) - linha 2, ws 9
        p_id, _ = create_in_progress(
            cur, client_order_id, f"MO-INP-06-{run_suffix}", random.choice(model_ids), now,
            phase_id=phase_ids_ordered[0], ws_id=6, line_id=2,
            support_id=None,  # sem skid — aparece como waiting_support no WIP
            started_ago_hours=0.2
        )
        print(f"   id={p_id} — Estampagem ws9 linha2 (normal, sem skid)")

        # ── Grupo 4: criar alerta para o produto CRITICAL ────────────────────
        print("\n[4/5] A criar alerta Andon para produto critical...")
        cur.execute("SELECT serial_number FROM product WHERE id = %s;", (p_id_critical,))
        serial = cur.fetchone()[0]

        cur.execute(
            "INSERT INTO alert (type, status, product_id, product_phase_id, triggered_at, "
            "notes, product_serial, phase_name, threshold_pct, estimated_duration) "
            "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s);",
            ("time_exceeded", "open", p_id_critical, pp_id_critical,
            now - timedelta(minutes=30),
            f"Produto excedeu 150% do tempo estimado na fase Inspeção",
            serial, "Inspeção", 150, 1800)
        )
        print(f"   ✓ Alerta criado para produto id={p_id_critical}")

        # ── Grupo 5: 2 produtos COMPLETADOS ──────────────────────────────────
        print("\n[5/5] A criar produtos completados...")
        for i in range(1, 3):
            mo_id, product_id = create_mo_and_product(
                cur, client_order_id, f"ORD-WIP-TEST-MO-DONE-{i:02d}",
                random.choice(model_ids), now, 15 + i
            )
            t = now - timedelta(hours=8)
            for phase_id in phase_ids_ordered:
                ws_id = PHASE_WS[phase_id][1]
                t_end = t + timedelta(seconds=1800)
                cur.execute(
                    "INSERT INTO product_phase (notes, result, datetime_ini, datetime_end, "
                    "manufacturing_phase_id, product_id, workstation_id) "
                    "VALUES (%s, %s, %s, %s, %s, %s, %s);",
                    ("seed wip test", "Aprovado", t, t_end, phase_id, product_id, ws_id)
                )
                t = t_end + timedelta(minutes=5)
            cur.execute(
                "UPDATE manufacturing_order SET status = %s, end_date = %s WHERE id = %s;",
                ("completed", t, mo_id)
            )
            cur.execute(
                "UPDATE product SET production_date = %s WHERE id = %s;",
                (t, product_id)
            )
            print(f"   id={product_id} — completado")

        conn.commit()
        print("\n✅ Seed WIP concluído!")
        print("\nEstados criados:")
        print("  • 5 produtos sem skid          → aparecem na Fila de Espera (motivo: Sem skid)")
        print("  • 2 produtos com skid sem linha → aparecem na Fila de Espera (motivo: À espera de linha)")
        print("  • 6 produtos em curso           → aparecem no Kanban por workstation")
        print("    - 2 normais (dentro do tempo)")
        print("    - 1 warning (>100% estimado, sem alerta)")
        print("    - 1 critical (alerta Andon ativo)")
        print("    - 2 normais em linha 2")
        print("  • 2 produtos completados        → KPI Concluídos = 2")
        print("\nAbre o WIP Dashboard e verifica as 3 vistas.")
        print("\nProdutos 'in_progress' bons para testar GET /api/Product/{id}/eta:")
        print(f"  - id={p_id_critical} (Inspeção, linha 2, atrasado)")
        print("  - usa os outros ids 'id=...' impressos acima no Grupo 3")

    except Exception as e:
        conn.rollback()
        print(f"\n❌ Erro: {e}")
        raise
    finally:
        cur.close()
        conn.close()


def load_models(cur):
    cur.execute("SELECT id, name FROM model ORDER BY id;")
    return {name: mid for mid, name in cur.fetchall()}


def load_phases(cur):
    cur.execute("SELECT id, name FROM manufacturing_phase ORDER BY id;")
    return {pid: name for pid, name in cur.fetchall()}


def load_supports(cur):
    cur.execute("SELECT id, rfid_tag FROM support ORDER BY id;")
    return [{"id": sid, "rfid_tag": tag} for sid, tag in cur.fetchall()]


def ensure_phase_sequences(cur, models, phases):
    phase_order = [1, 2, 3, 4, 5]  # ids das fases por ordem
    for model_name, model_id in models.items():
        cur.execute("SELECT COUNT(*) FROM phase_sequence WHERE model_id = %s;", (model_id,))
        if cur.fetchone()[0] > 0:
            continue
        for order, phase_id in enumerate(phase_order, start=1):
            cur.execute(
                'INSERT INTO phase_sequence ("order", manufacturing_phase_id, model_id) '
                "VALUES (%s, %s, %s);",
                (order, phase_id, model_id)
            )
        print(f"  ✓ PhaseSequence criada para modelo id={model_id}")


def create_mo_and_product(cur, client_order_id, mo_number, model_id, now, seq):
    cur.execute(
        "INSERT INTO manufacturing_order (client_order_id, manufacturing_order_number, "
        "start_date, status) VALUES (%s, %s, %s, %s) RETURNING id;",
        (client_order_id, mo_number, now - timedelta(hours=seq), "pending")
    )
    mo_id = cur.fetchone()[0]
    serial = f"VIN-WIP-{mo_number[-6:]}-{seq:03d}"
    cur.execute(
        "INSERT INTO product (manufacturing_order_id, model_id, serial_number, lot_number) "
        "VALUES (%s, %s, %s, %s) RETURNING id;",
        (mo_id, model_id, serial, "WIP-TEST")
    )
    product_id = cur.fetchone()[0]
    return mo_id, product_id


def create_in_progress(cur, client_order_id, mo_number, model_id, now,
                        phase_id, ws_id, line_id, support_id, started_ago_hours):
    """
    Devolve (product_id, product_phase_id) — o chamador precisa do
    product_phase_id sempre que for criar um Alert associado a esta fase.
    """
    mo_id, product_id = create_mo_and_product(
        cur, client_order_id, mo_number, model_id, now, int(started_ago_hours * 10)
    )

    cur.execute(
        "UPDATE manufacturing_order SET status = %s WHERE id = %s;",
        ("in_progress", mo_id)
    )

    datetime_ini = now - timedelta(hours=started_ago_hours)

    cur.execute(
        "INSERT INTO product_phase (notes, result, datetime_ini, datetime_end, "
        "manufacturing_phase_id, product_id, workstation_id) "
        "VALUES (%s, %s, %s, %s, %s, %s, %s) RETURNING id;",
        ("seed wip test", None, datetime_ini, None, phase_id, product_id, ws_id)
    )
    pp_id = cur.fetchone()[0]

    if support_id:
        cur.execute(
            "INSERT INTO supported_product (support_id, product_id, datetime_ini) "
            "VALUES (%s, %s, %s);",
            (support_id, product_id, datetime_ini)
        )
        cur.execute(
            "INSERT INTO localization_history (support_id, workstation_id, datetime_ini, status) "
            "VALUES (%s, %s, %s, %s);",
            (support_id, ws_id, datetime_ini, "active")
        )

    return product_id, pp_id


if __name__ == "__main__":
    main()