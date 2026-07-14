"""
DRIVOLUTION - Agente Simulado de Crachá (Presença por Workstation)

Simula um operador a "passar o cartão" (crachá RFID) num leitor associado
a uma workstation Humana/Híbrida. Segue exatamente o mesmo padrão do
drivolution_agent.py (skids): envia para o IoT Agent, que atualiza a
entidade no Orion, que por sua vez notifica a API via subscrição.

Toggle: ler o crachá na mesma workstation onde já está presente = saída.
        ler o crachá numa workstation diferente = saída automática da
        anterior (se houver) + entrada na nova.
"""

import argparse
import os
import time
import requests
from dotenv import load_dotenv
load_dotenv()

IOT_AGENT_URL = "http://localhost:7896/iot/json"
API_BASE_URL  = "http://localhost:8080/api"
BADGE_APIKEY  = "drivolution-badge-key"

HEADERS_IOT = {
    "Content-Type":       "application/json",
    "FIWARE-Service":     "drivolution",
    "FIWARE-ServicePath": "/",
}

_cached_token = None


def login():
    """Mesmo padrão de drivolution_agent.py:
    DRIVOLUTION_EMAIL / DRIVOLUTION_PASSWORD, ou DRIVOLUTION_TOKEN direto."""
    global _cached_token

    if _cached_token:
        return _cached_token

    manual_token = os.getenv("DRIVOLUTION_TOKEN")
    if manual_token:
        _cached_token = manual_token.replace("Bearer ", "").strip()
        return _cached_token

    email = os.getenv("DRIVOLUTION_EMAIL")
    password = os.getenv("DRIVOLUTION_PASSWORD")

    if not email or not password:
        print("   ⚠ Sem autenticação definida.")
        print("   Define DRIVOLUTION_EMAIL e DRIVOLUTION_PASSWORD ou DRIVOLUTION_TOKEN.")
        return None

    r = requests.post(
        f"{API_BASE_URL}/Auth/login",
        json={"email": email, "password": password},
        timeout=10,
    )

    if r.status_code not in (200, 201):
        print(f"   ✗ Login falhou ({r.status_code}): {r.text}")
        return None

    token = r.json().get("token")
    if not token:
        print(f"   ✗ Login respondeu mas sem token: {r.json()}")
        return None

    _cached_token = token
    print("   ✓ Login efetuado com sucesso.")
    return _cached_token


def api_headers():
    h = {"Content-Type": "application/json"}
    token = login()
    if token:
        h["Authorization"] = f"Bearer {token}"
    return h


def load_users(role: str | None = None) -> list:
    """Carrega utilizadores (operator/manager) da API. Requer conta admin no .env."""
    try:
        users = []
        roles = [role] if role else ["operator", "manager"]
        for r_ in roles:
            r = requests.get(
                f"{API_BASE_URL}/User",
                headers=api_headers(),
                params={"role": r_, "pageSize": 100},
                timeout=5,
            )
            if r.status_code == 200:
                data = r.json()
                items = data.get("data") or data.get("Data") or data.get("$values", data)
                users.extend(items if isinstance(items, list) else [])
            else:
                print(f"   ✗ Erro ao carregar role '{r_}': {r.status_code}")
        return users
    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return []


def load_workstations() -> list:
    """Carrega workstations da API e filtra só as elegíveis para presença humana."""
    try:
        r = requests.get(f"{API_BASE_URL}/Workstation", headers=api_headers(), timeout=5)
        if r.status_code == 200:
            data = r.json()
            raw = data.get("$values", data) if isinstance(data, dict) else data
            raw = raw if isinstance(raw, list) else []
            return [w for w in raw if w.get("kind") in ("human", "hybrid")]
        print(f"   ✗ Erro ao carregar workstations: {r.status_code}")
        return []
    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return []


def send_badge_event(user: dict, workstation: dict) -> bool:
    payload = {"currentWorkstation": workstation["id"]}
    url = f"{IOT_AGENT_URL}?i=badge-{user['id']}&k={BADGE_APIKEY}"

    line_name  = workstation.get("productionLineName", f"Linha #{workstation.get('productionLineId', '?')}")
    phase_name = workstation.get("phaseName", "?")

    print(f"\n🪪 A simular leitura de crachá:")
    print(f"   Operador:    {user.get('name', '?')} (id {user['id']})")
    print(f"   Workstation: {workstation['id']} — {line_name} / {phase_name}")

    try:
        r = requests.post(url, headers=HEADERS_IOT, json=payload, timeout=10)
        if r.status_code in (200, 201, 204):
            print(f"   ✓ IoT Agent aceitou ({r.status_code}).")
            return True
        print(f"   ✗ Rejeitado ({r.status_code}): {r.text}")
        return False
    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return False


def verify_orion(user_id: int):
    entity_id = f"urn:ngsi-ld:Badge:badge-{user_id}"
    try:
        r = requests.get(
            f"http://localhost:1026/ngsi-ld/v1/entities/{entity_id}",
            headers={"FIWARE-Service": "drivolution", "FIWARE-ServicePath": "/"},
            timeout=5,
        )
        if r.status_code == 200:
            ws = r.json().get("currentWorkstation", {}).get("value", "?")
            print(f"   🔍 Orion — currentWorkstation: {ws}")
        else:
            print(f"   ⚠ Entidade não encontrada no Orion ({r.status_code})")
    except Exception as e:
        print(f"   ⚠ {e}")


def interactive_mode(users, workstations):
    print("\n" + "=" * 55)
    print("  DRIVOLUTION - Agente de Crachá (Modo Interativo)")
    print("=" * 55)

    while True:
        print("\n── Operadores ─────────────────────────────────────────")
        if not users:
            print("  ✗ Nenhum utilizador operator/manager encontrado.")
            break

        for i, u in enumerate(users):
            print(f"  [{i+1}] ID:{u['id']:3} | {u.get('name', '?')}")

        try:
            escolha = int(input("\nEscolhe um operador (0 para sair): "))
            if escolha == 0:
                break
            user = users[escolha - 1]
        except (ValueError, IndexError):
            print("Opção inválida.")
            continue

        print("\n── Workstations (human/hybrid) ─────────────────────────")
        if not workstations:
            print("  ✗ Nenhuma workstation elegível (human/hybrid) encontrada.")
            continue

        for ws in workstations:
            line_name  = ws.get("productionLineName", f"Linha #{ws.get('productionLineId', '?')}")
            phase_name = ws.get("phaseName", "?")
            print(f"  [{ws['id']}] {line_name:10} | {phase_name} | kind={ws.get('kind')}")

        try:
            ws_id = int(input("\nWorkstation do leitor: "))
            workstation = next((w for w in workstations if w["id"] == ws_id), None)
            if not workstation:
                print("Workstation inválida.")
                continue
        except ValueError:
            print("Opção inválida.")
            continue

        sucesso = send_badge_event(user, workstation)
        if sucesso:
            print("\n⏳ A aguardar 3s...")
            time.sleep(3)
            verify_orion(user["id"])
            print("✅ Verifica o ecrã de Presença no dashboard.")


def auto_mode(users, workstations):
    """Simula o primeiro operador a entrar e sair da primeira workstation elegível."""
    print("\n" + "=" * 55)
    print("  DRIVOLUTION - Agente de Crachá (Modo Automático)")
    print("=" * 55)

    if not users:
        print("✗ Nenhum operador disponível.")
        return
    if not workstations:
        print("✗ Nenhuma workstation human/hybrid disponível.")
        return

    user = users[0]
    ws = workstations[0]

    print(f"\n🪪 Operador {user.get('name', '?')} vai fazer check-in em WS {ws['id']}...")
    send_badge_event(user, ws)
    time.sleep(3)
    verify_orion(user["id"])

    print("\n⏳ 5s de presença simulada...")
    time.sleep(5)

    print(f"\n🪪 Operador {user.get('name', '?')} vai fazer check-out (mesmo leitor)...")
    send_badge_event(user, ws)  # mesma workstation = toggle → check-out
    time.sleep(3)
    verify_orion(user["id"])

    print("\n✅ Simulação automática concluída.")


def direct_mode(users, workstations, user_id: int, ws_id: int):
    user = next((u for u in users if u["id"] == user_id), None)
    ws   = next((w for w in workstations if w["id"] == ws_id), None)
    if not user:
        print(f"✗ Utilizador {user_id} não encontrado (ou não é operator/manager).")
        return
    if not ws:
        print(f"✗ Workstation {ws_id} não encontrada (ou não é human/hybrid).")
        return
    send_badge_event(user, ws)
    time.sleep(3)
    verify_orion(user_id)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="DRIVOLUTION Badge Agent")
    parser.add_argument("--auto", action="store_true")
    parser.add_argument("--user", type=int, help="ID do utilizador (AppUser)")
    parser.add_argument("--ws",   type=int, help="ID da workstation")
    args = parser.parse_args()

    print("🔄 A carregar dados da API...")
    users        = load_users()
    workstations = load_workstations()
    print(f"   ✓ {len(users)} operador(es)/manager(s) | {len(workstations)} workstation(s) elegível(eis)")

    if args.auto:
        auto_mode(users, workstations)
    elif args.user and args.ws:
        direct_mode(users, workstations, args.user, args.ws)
    else:
        interactive_mode(users, workstations)
