"""
DRIVOLUTION - Orquestrador de Simulação Completa da Linha
"""

import argparse
import os
import time
import requests
from dotenv import load_dotenv

load_dotenv()

from drivolution_quality_agent import (
    login,
    headers,
    normalize_list,
    get_existing_quality_checks,
    create_quality_check,
    simulate_quality_result,
    get_current_phase_from_timeline,
)

API_BASE_URL = os.getenv("DRIVOLUTION_API_URL", "http://localhost:8080/api")
IOT_AGENT_URL = os.getenv("DRIVOLUTION_IOT_URL", "http://localhost:7896/iot/json")
API_KEY = os.getenv("DRIVOLUTION_IOT_KEY", "drivolution-key")

HEADERS_IOT = {
    "Content-Type": "application/json",
    "FIWARE-Service": "drivolution",
    "FIWARE-ServicePath": "/",
}

_model_phase_sequence_cache = {}


def get_wip():
    r = requests.get(f"{API_BASE_URL}/production-lines/wip", headers=headers(), timeout=10)
    r.raise_for_status()
    return r.json()


def get_supports(occupied: bool | None = None):
    params = {"pageSize": 500}
    if occupied is not None:
        params["occupied"] = str(occupied).lower()

    r = requests.get(f"{API_BASE_URL}/Support", headers=headers(), params=params, timeout=10)
    r.raise_for_status()
    data = r.json()
    return normalize_list(data.get("data", []))


def get_workstations_by_line(production_line_id: int):
    r = requests.get(f"{API_BASE_URL}/Workstation/line/{production_line_id}", headers=headers(), timeout=10)
    r.raise_for_status()
    return normalize_list(r.json())


def get_phase_sequence(model_id: int):
    if model_id in _model_phase_sequence_cache:
        return _model_phase_sequence_cache[model_id]

    r = requests.get(f"{API_BASE_URL}/PhaseSequence/model/{model_id}", headers=headers(), timeout=10)
    r.raise_for_status()
    sequence = sorted(normalize_list(r.json()), key=lambda p: p.get("order", 0))
    _model_phase_sequence_cache[model_id] = sequence
    return sequence


def get_product(product_id: int):
    r = requests.get(f"{API_BASE_URL}/Product/{product_id}", headers=headers(), timeout=10)
    if r.status_code == 404:
        return None
    r.raise_for_status()
    return r.json()


def is_eligible_for_new_skid(product_id: int) -> bool:
    r = requests.get(f"{API_BASE_URL}/products/{product_id}/timeline", headers=headers(), timeout=10)

    if r.status_code in (400, 404):
        return True

    if r.status_code != 200:
        return False

    data = r.json()
    return data.get("status") not in ("completed", "in_progress")


def assign_product_to_skid(support_id: int, product_id: int):
    payload = {"supportId": support_id, "productId": product_id}
    r = requests.post(f"{API_BASE_URL}/SupportedProduct", headers=headers(), json=payload, timeout=10)

    if r.status_code in (200, 201):
        return r.json()

    print(f"   ✗ Erro ao associar produto {product_id} ao skid {support_id} ({r.status_code}): {r.text}")
    return None


def get_current_supported_product(support_id: int):
    r = requests.get(f"{API_BASE_URL}/SupportedProduct/support/{support_id}/current", headers=headers(), timeout=10)
    if r.status_code == 404:
        return None
    r.raise_for_status()
    return r.json()


def close_supported_product(supported_product_id: int):
    r = requests.put(f"{API_BASE_URL}/SupportedProduct/{supported_product_id}/close", headers=headers(), timeout=10)

    if r.status_code not in (200, 204):
        print(f"   ✗ Erro ao libertar skid (SupportedProduct {supported_product_id}): {r.status_code} {r.text}")
        return False

    return True


def close_product_phase(product_phase_id: int, result: str, quality_id: int | None):
    payload = {"result": result, "qualityId": quality_id}
    r = requests.put(f"{API_BASE_URL}/ProductPhase/{product_phase_id}/close", headers=headers(), json=payload, timeout=10)

    if r.status_code not in (200, 204):
        print(f"   ✗ Erro ao fechar ProductPhase {product_phase_id}: {r.status_code} {r.text}")
        return False

    return True


def send_rfid_event(skid: dict, workstation: dict) -> bool:
    payload = {
        "currentWorkstation": workstation["id"],
        "productId": skid.get("currentProductId"),
        "supportId": skid["id"],
        "rfidTag": skid.get("rfidTag"),
    }

    url = f"{IOT_AGENT_URL}?i=skid-{skid.get('rfidTag')}&k={API_KEY}"

    print(
        f"   📡 Skid {skid.get('rfidTag')} -> Workstation {workstation['id']} "
        f"({workstation.get('phaseName', '?')})"
    )

    try:
        r = requests.post(url, headers=HEADERS_IOT, json=payload, timeout=10)

        if r.status_code in (200, 201, 204):
            return True

        print(f"   ✗ IoT Agent rejeitou ({r.status_code}): {r.text}")
        return False

    except Exception as e:
        print(f"   ✗ Erro ao enviar evento RFID: {e}")
        return False


def start_journey(skid: dict, product_id: int) -> bool:
    product = get_product(product_id)
    if not product:
        return False

    phase_seq = get_phase_sequence(product["modelId"])
    if not phase_seq:
        print(f"   ✗ Modelo {product['modelId']} sem phase_sequence definida.")
        return False

    first_phase = phase_seq[0]
    line_ws = get_workstations_by_line(skid["productionLineId"])

    target_ws = next(
        (w for w in line_ws if w.get("manufacturingPhaseId") == first_phase.get("manufacturingPhaseId")),
        None,
    )

    if not target_ws:
        print(f"   ✗ Linha {skid['productionLineId']} não tem workstation para a 1ª fase do modelo.")
        return False

    skid["currentProductId"] = product_id
    return send_rfid_event(skid, target_ws)


def assign_free_products(waiting_items: list):
    waiting_for_skid = [w for w in waiting_items if w.get("queueReason") == "waiting_support"]
    waiting_for_line = [w for w in waiting_items if w.get("queueReason") == "waiting_line"]

    if waiting_for_skid:
        free_skids = get_supports(occupied=False)

        if not free_skids:
            print("   - Há produtos à espera de skid, mas não há skids livres.")
        else:
            skid_index = 0

            for item in waiting_for_skid:
                if skid_index >= len(free_skids):
                    print("   - Sem mais skids livres neste ciclo.")
                    break

                product_id = item.get("productId")
                serial = item.get("serialNumber")

                if not is_eligible_for_new_skid(product_id):
                    continue

                skid = free_skids[skid_index]
                assigned = assign_product_to_skid(skid["id"], product_id)

                if not assigned:
                    continue

                print(f"   ✓ Produto {serial} associado ao skid {skid.get('rfidTag')} (linha {skid.get('productionLineId')})")

                skid_index += 1
                start_journey(skid, product_id)

    if waiting_for_line:
        occupied_skids = get_supports(occupied=True)

        for item in waiting_for_line:
            product_id = item.get("productId")
            serial = item.get("serialNumber")
            support_id = item.get("supportId")

            if not support_id:
                print(f"   ✗ Produto {serial} em 'waiting_line' sem supportId — dado inconsistente.")
                continue

            skid = next((s for s in occupied_skids if s.get("id") == support_id), None)

            if not skid:
                print(f"   ✗ Produto {serial}: skid {support_id} não encontrado entre os ocupados.")
                continue

            print(f"   ▶ Produto {serial} já tem skid {skid.get('rfidTag')} mas nunca arrancou — a iniciar jornada.")
            start_journey(skid, product_id)


def adopt_orphan_product(item: dict, product_id: int, serial: str):
    line_id = item.get("productionLineId")

    if not line_id:
        print(f"   ✗ Produto {serial} em progresso sem skid e sem linha conhecida — não é possível adotar.")
        return None

    free_skids = [s for s in get_supports(occupied=False) if s.get("productionLineId") == line_id]

    if not free_skids:
        print(f"   ⚠ Produto {serial} em progresso sem skid, e não há skids livres na linha {line_id} para adotar.")
        return None

    skid = free_skids[0]
    assigned = assign_product_to_skid(skid["id"], product_id)

    if not assigned:
        return None

    print(f"   🩹 Produto {serial} adotado pelo skid {skid.get('rfidTag')} da linha {line_id}.")
    return skid


def get_quality_check_for_phase(product_id: int, manufacturing_phase_id: int):
    checks = get_existing_quality_checks(product_id)

    return next(
        (c for c in checks if c.get("manufacturingPhaseId") == manufacturing_phase_id),
        None,
    )


def advance_in_progress_products(wip_items: list):
    for item in wip_items:
        product_id = item.get("productId")
        serial = item.get("serialNumber")

        active_phase = get_current_phase_from_timeline(product_id)

        if not active_phase:
            continue

        manufacturing_phase_id = active_phase.get("manufacturingPhaseId")
        product_phase_id = active_phase.get("productPhaseId")

        check = get_quality_check_for_phase(product_id, manufacturing_phase_id)

        if not check:
            severity = simulate_quality_result()

            print(f"   🔍 Produto {serial} | Fase {item.get('currentPhase')} | Quality Check: {severity}")

            create_quality_check(
                product_id=product_id,
                manufacturing_phase_id=manufacturing_phase_id,
                severity=severity,
                notes=f"Quality Check automático gerado pelo orquestrador. Severidade observada: {severity}",
            )

            continue

        if check.get("status") != "passed":
            print(
                f"   ⛔ Produto {serial} bloqueado na fase {item.get('currentPhase')} "
                f"porque o Quality Check está '{check.get('status')}'."
            )
            continue

        product = get_product(product_id)

        if not product:
            continue

        phase_seq = get_phase_sequence(product["modelId"])

        current_order = next(
            (p.get("order") for p in phase_seq if p.get("manufacturingPhaseId") == manufacturing_phase_id),
            None,
        )

        if current_order is None:
            continue

        next_entry = next((p for p in phase_seq if p.get("order") == current_order + 1), None)

        occupied_skids = get_supports(occupied=True)
        skid = next((s for s in occupied_skids if s.get("currentProductId") == product_id), None)

        if not skid:
            skid = adopt_orphan_product(item, product_id, serial)

            if not skid:
                continue

            continue

        if next_entry is None:
            print(f"   🏁 Produto {serial} concluiu a última fase — a finalizar.")

            closed = close_product_phase(
                product_phase_id,
                result=check.get("status"),
                quality_id=check.get("id"),
            )

            if not closed:
                continue

            supported_product = get_current_supported_product(skid["id"])

            if supported_product:
                close_supported_product(supported_product["id"])
                print(f"   ✓ Skid {skid.get('rfidTag')} libertado para reutilização.")

        else:
            line_ws = get_workstations_by_line(skid["productionLineId"])

            target_ws = next(
                (w for w in line_ws if w.get("manufacturingPhaseId") == next_entry.get("manufacturingPhaseId")),
                None,
            )

            if not target_ws:
                print(f"   ✗ Linha {skid['productionLineId']} não tem workstation para a próxima fase.")
                continue

            send_rfid_event(skid, target_ws)


def tick():
    print("\n[Orchestrator] A processar ciclo da fábrica...")
    _model_phase_sequence_cache.clear()

    wip = get_wip()
    waiting_items = normalize_list(wip.get("waitingItems", []))
    in_progress_items = normalize_list(wip.get("items", []))

    if not waiting_items and not in_progress_items:
        print("   - Fábrica sem produtos ativos.")
        return

    assign_free_products(waiting_items)
    advance_in_progress_products(in_progress_items)


def main():
    parser = argparse.ArgumentParser(description="DRIVOLUTION - Orquestrador de Simulação Completa da Fábrica")
    parser.add_argument("--once", action="store_true", help="Executa um ciclo e termina.")
    parser.add_argument("--interval", type=int, default=10, help="Intervalo entre ciclos, em segundos.")
    args = parser.parse_args()

    print("=" * 60)
    print(" DRIVOLUTION - Orquestrador de Simulação da Fábrica")
    print("=" * 60)
    print(f" API: {API_BASE_URL}")
    print(f" IoT Agent: {IOT_AGENT_URL}")

    login()

    try:
        if args.once:
            tick()
            return

        while True:
            tick()
            time.sleep(args.interval)

    except requests.HTTPError as e:
        print(f"   ✗ Erro HTTP: {e}")

    except Exception as e:
        print(f"   ✗ Erro: {e}")


if __name__ == "__main__":
    main()