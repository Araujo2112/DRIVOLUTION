"""
DRIVOLUTION - Agente Simulado RFID
Carrega skids e workstations dinamicamente da API.
"""

import argparse
import json
import time

import requests

IOT_AGENT_URL = "http://localhost:7896/iot/json"
API_BASE_URL  = "http://localhost:8080/api"
API_KEY       = "drivolution-key"

HEADERS_IOT = {
    "Content-Type":       "application/json",
    "FIWARE-Service":     "drivolution",
    "FIWARE-ServicePath": "/",
}


def load_skids() -> list:
    """Carrega suportes da API."""
    try:
        r = requests.get(f"{API_BASE_URL}/Support", timeout=5)
        if r.status_code == 200:
            data = r.json()
            raw = data.get("$values", data) if isinstance(data, dict) else data
            return raw if isinstance(raw, list) else []
        print(f"   ✗ Erro ao carregar suportes: {r.status_code}")
        return []
    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return []


def load_workstations() -> list:
    """Carrega workstations da API."""
    try:
        r = requests.get(f"{API_BASE_URL}/Workstation", timeout=5)
        if r.status_code == 200:
            data = r.json()
            raw = data.get("$values", data) if isinstance(data, dict) else data
            return raw if isinstance(raw, list) else []
        print(f"   ✗ Erro ao carregar workstations: {r.status_code}")
        return []
    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return []


def get_current_product(support_id: int) -> int | None:
    try:
        r = requests.get(
            f"{API_BASE_URL}/SupportedProduct/support/{support_id}/current",
            timeout=5
        )
        return r.json().get("productId") if r.status_code == 200 else None
    except (requests.RequestException, ValueError):
        return None


def send_rfid_event(skid: dict, workstation: dict) -> bool:
    product_id = get_current_product(skid["id"])

    payload = {
        "currentWorkstation": workstation["id"],
        "productId":          product_id,
        "supportId":          skid["id"],
        "rfidTag":            skid["rfidTag"],
    }

    url = f"{IOT_AGENT_URL}?i=skid-{skid['rfidTag']}&k={API_KEY}"

    line_name = workstation.get("productionLineName", f"Linha #{workstation['productionLineId']}")
    phase_name = workstation.get("phaseName", "?")
    ws_type   = workstation.get("type", "?")

    print(f"\n📡 A simular leitura RFID:")
    print(f"   Skid:        {skid['rfidTag']} ({skid.get('type', '?')})")
    print(f"   Workstation: {workstation['id']} — {line_name} / WS {ws_type} / {phase_name}")
    print(f"   Produto:     {product_id if product_id else 'nenhum'}")

    try:
        r = requests.post(url, headers=HEADERS_IOT, json=payload, timeout=10)
        if r.status_code in (200, 201, 204):
            print(f"   ✓ IoT Agent aceitou ({r.status_code}).")
            return True
        else:
            print(f"   ✗ Rejeitado ({r.status_code}): {r.text}")
            return False
    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return False


def verify_orion(rfid_tag: str):
    entity_id = f"urn:ngsi-ld:Skid:skid-{rfid_tag}"
    try:
        r = requests.get(
            f"http://localhost:1026/ngsi-ld/v1/entities/{entity_id}",
            headers={"FIWARE-Service": "drivolution", "FIWARE-ServicePath": "/"},
            timeout=5
        )
        if r.status_code == 200:
            ws = r.json().get("currentWorkstation", {}).get("value", "?")
            print(f"   🔍 Orion — currentWorkstation: {ws}")
        else:
            print(f"   ⚠ Entidade não encontrada no Orion ({r.status_code})")
    except Exception as e:
        print(f"   ⚠ {e}")


def interactive_mode(skids, workstations):
    print("\n" + "=" * 55)
    print("  DRIVOLUTION - Agente RFID (Modo Interativo)")
    print("=" * 55)

    while True:
        # Recarregar skids a cada ciclo para mostrar estado atual
        skids = load_skids()

        print("\n── Skids ──────────────────────────────────────────────")
        if not skids:
            print("  ✗ Nenhum suporte encontrado na API.")
            break

        for i, s in enumerate(skids):
            product_id = get_current_product(s["id"])
            prod_str   = f"Produto #{product_id}" if product_id else "Livre"
            tag        = s.get("rfidTag") or "sem tag"
            tipo       = s.get("type") or "?"
            print(f"  [{i+1}] ID:{s['id']:3} | {tag:15} | {tipo:15} | {prod_str}")

        try:
            escolha = int(input("\nEscolhe um skid (0 para sair): "))
            if escolha == 0:
                break
            skid = skids[escolha - 1]
        except (ValueError, IndexError):
            print("Opção inválida.")
            continue

        if not skid.get("rfidTag"):
            print("   ✗ Este suporte não tem tag RFID definida.")
            continue

        print("\n── Workstations ───────────────────────────────────────")
        for ws in workstations:
            line_name  = ws.get("productionLineName", f"Linha #{ws['productionLineId']}")
            phase_name = ws.get("phaseName", "?")
            print(f"  [{ws['id']}] {line_name:10} | WS {ws.get('type', '?'):3} | {phase_name}")

        try:
            ws_id = int(input("\nWorkstation de destino: "))
            workstation = next((w for w in workstations if w["id"] == ws_id), None)
            if not workstation:
                print("Workstation inválida.")
                continue
        except ValueError:
            print("Opção inválida.")
            continue

        sucesso = send_rfid_event(skid, workstation)
        if sucesso:
            print("\n⏳ A aguardar 3s...")
            time.sleep(3)
            verify_orion(skid["rfidTag"])
            print("✅ Verifica o WIP Dashboard.")


def auto_mode(skids, workstations):
    """Simula o primeiro skid a percorrer as workstations da sua linha."""
    print("\n" + "=" * 55)
    print("  DRIVOLUTION - Agente RFID (Modo Automático)")
    print("=" * 55)

    if not skids:
        print("✗ Nenhum suporte disponível.")
        return

    skid = skids[0]
    if not skid.get("rfidTag"):
        print("✗ Primeiro suporte sem tag RFID.")
        return

    linha_ws = [ws for ws in workstations if ws["productionLineId"] == skid.get("productionLineId", 1)]
    if not linha_ws:
        linha_ws = workstations[:3]

    print(f"\n🚗 Skid {skid['rfidTag']} a percorrer {len(linha_ws)} workstations...")

    for ws in linha_ws:
        send_rfid_event(skid, ws)
        print("   ⏳ 5s...")
        time.sleep(5)
        verify_orion(skid["rfidTag"])

    print("\n✅ Simulação automática concluída.")


def direct_mode(skids, workstations, tag: str, ws_id: int):
    skid = next((s for s in skids if s.get("rfidTag") == tag), None)
    ws   = next((w for w in workstations if w["id"] == ws_id), None)
    if not skid:
        print(f"✗ Tag '{tag}' não encontrada.")
        return
    if not ws:
        print(f"✗ Workstation {ws_id} não encontrada.")
        return
    send_rfid_event(skid, ws)
    time.sleep(3)
    verify_orion(tag)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="DRIVOLUTION RFID Agent")
    parser.add_argument("--auto", action="store_true")
    parser.add_argument("--tag",  type=str)
    parser.add_argument("--ws",   type=int)
    args = parser.parse_args()

    print("🔄 A carregar dados da API...")
    skids        = load_skids()
    workstations = load_workstations()
    print(f"   ✓ {len(skids)} suporte(s) | {len(workstations)} workstation(s)")

    if args.auto:
        auto_mode(skids, workstations)
    elif args.tag and args.ws:
        direct_mode(skids, workstations, args.tag, args.ws)
    else:
        interactive_mode(skids, workstations)