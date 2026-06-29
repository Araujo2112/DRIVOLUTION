"""
setup_environment.py

Cria toda a infraestrutura do DRIVOLUTION do zero numa BD limpa:
  1. Fases de fabrico (5 fases Toyota)
  2. Linhas de produção (Linha A + Linha B)
  3. Workstations (5 por linha, uma por fase)
  4. Suportes/skids (7 skids com tags RFID)
  5. Registo FIWARE (IoT Agent + Orion + subscrições)

Depois de correr este script, podes correr os outros seeds normalmente.

Pré-requisito: docker compose up -d e API acessível em localhost:8080
Correr: python setup_environment.py
"""

import requests
import json
import time
import sys

API_BASE      = "http://localhost:8080/api"
ORION_URL     = "http://localhost:1026"
IOT_AGENT_URL = "http://localhost:4041"

# URLs internas do Docker (usadas nas subscrições — não mudar)
API_NOTIFY_URL = "http://api:8080/api/FiwareNotification"
QL_NOTIFY_URL  = "http://quantumleap:8668/v2/notify"

CONTEXT_URL   = "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld"
FIWARE_SERVICE     = "drivolution"
FIWARE_SERVICEPATH = "/"
APIKEY    = "drivolution-key"
RESOURCE  = "/iot/json"

# Credenciais do admin seedado na BD (init-drivolution-schema.sql).
# A maioria dos endpoints da API agora exige [Authorize], por isso o script
# precisa de um JWT válido antes de poder criar fases/linhas/workstations/etc.
ADMIN_EMAIL    = "admin@drivolution.pt"
ADMIN_PASSWORD = "12345678"

HEADERS_LD = {
    "Content-Type": "application/ld+json",
    "FIWARE-Service": FIWARE_SERVICE,
    "FIWARE-ServicePath": FIWARE_SERVICEPATH,
}
HEADERS_JSON = {
    "FIWARE-Service": FIWARE_SERVICE,
    "FIWARE-ServicePath": FIWARE_SERVICEPATH,
}
IOT_HEADERS = {
    "Content-Type": "application/json",
    "FIWARE-Service": FIWARE_SERVICE,
    "FIWARE-ServicePath": FIWARE_SERVICEPATH,
}

# Preenchido por login() — usado em todos os pedidos à API do Drivolution.
AUTH_HEADERS = {}

# ─────────────────────────────────────────────────────────────────────────────
# Dados a replicar (espelho exato do ambiente do Tiago)
# ─────────────────────────────────────────────────────────────────────────────

PHASES = [
    {"name": "Estampagem", "estimatedDuration": 1800, "maxAcceptableSeverity": "none", "reworkSeverity": "none",  "timeThresholdPct": 150},
    {"name": "Soldadura",  "estimatedDuration": 2700, "maxAcceptableSeverity": "none", "reworkSeverity": "minor", "timeThresholdPct": 150},
    {"name": "Pintura",    "estimatedDuration": 1800, "maxAcceptableSeverity": "none", "reworkSeverity": "minor", "timeThresholdPct": 150},
    {"name": "Montagem",   "estimatedDuration": 1800, "maxAcceptableSeverity": "none", "reworkSeverity": "minor", "timeThresholdPct": 150},
    {"name": "Inspeção",   "estimatedDuration": 1800, "maxAcceptableSeverity": "none", "reworkSeverity": "none",  "timeThresholdPct": 150},
]

LINES = [
    {"name": "Linha A", "location": "Este",  "status": "functional", "capacity": 100},
    {"name": "Linha B", "location": "Oeste", "status": "functional", "capacity": 50},
]

# Workstations: (linha_index, fase_index, type)
# linha_index e fase_index são 0-based — os IDs reais são resolvidos após criação
WORKSTATIONS = [
    # Linha A
    # type = identificador de posto | kind = classificação de operação (human/hybrid/machine)
    {"line_index": 0, "phase_index": 0, "type": "A", "kind": "machine"},  # Estampagem
    {"line_index": 0, "phase_index": 1, "type": "B", "kind": "hybrid"},   # Soldadura
    {"line_index": 0, "phase_index": 2, "type": "C", "kind": "machine"},  # Pintura
    {"line_index": 0, "phase_index": 3, "type": "D", "kind": "human"},    # Montagem
    {"line_index": 0, "phase_index": 4, "type": "E", "kind": "human"},    # Inspeção
    # Linha B
    {"line_index": 1, "phase_index": 0, "type": "1", "kind": "machine"},  # Estampagem
    {"line_index": 1, "phase_index": 1, "type": "2", "kind": "hybrid"},   # Soldadura
    {"line_index": 1, "phase_index": 2, "type": "3", "kind": "machine"},  # Pintura
    {"line_index": 1, "phase_index": 3, "type": "4", "kind": "human"},    # Montagem
    {"line_index": 1, "phase_index": 4, "type": "6", "kind": "human"},    # Inspeção
]

SKIDS = [
    {"rfidTag": "3542100258",  "type": "Skid-A-001", "line_index": 0},
    {"rfidTag": "3220140258",  "type": "Skid-A-002", "line_index": 0},
    {"rfidTag": "3542100123",  "type": "Skid-B-001", "line_index": 1},
    {"rfidTag": "35408750123", "type": "Skid-A-003", "line_index": 0},
    {"rfidTag": "35408750000", "type": "Skid-A-004", "line_index": 0},
    {"rfidTag": "35408750111", "type": "Skid-A-005", "line_index": 0},
    {"rfidTag": "35408750843", "type": "Skid-A-006", "line_index": 0},
]


# ─────────────────────────────────────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────────────────────────────────────

def login():
    """Autentica como admin e guarda o JWT para usar em todos os pedidos seguintes."""
    print("A autenticar como admin...", end="", flush=True)
    r = requests.post(
        f"{API_BASE}/Auth/login",
        json={"email": ADMIN_EMAIL, "password": ADMIN_PASSWORD},
        timeout=10,
    )
    if r.status_code != 200:
        print(" ✗")
        print(f"   Login falhou ({r.status_code}): {r.text[:200]}")
        print("   Confirma ADMIN_EMAIL/ADMIN_PASSWORD no topo deste script.")
        sys.exit(1)

    token = r.json()["token"]
    AUTH_HEADERS["Authorization"] = f"Bearer {token}"
    print(" ✓")


def post(path, body):
    r = requests.post(f"{API_BASE}/{path}", json=body, headers=AUTH_HEADERS, timeout=10)
    if r.status_code not in (200, 201):
        raise RuntimeError(f"POST /{path} → {r.status_code}: {r.text[:200]}")
    return r.json()

def get(path):
    r = requests.get(f"{API_BASE}/{path}", headers=AUTH_HEADERS, timeout=10)
    if r.status_code != 200:
        raise RuntimeError(f"GET /{path} → {r.status_code}: {r.text[:200]}")
    data = r.json()
    return data.get("$values", data) if isinstance(data, dict) else data

def wait_for_api():
    print("A aguardar a API...", end="", flush=True)
    for _ in range(20):
        try:
            requests.get(f"{API_BASE.replace('/api','')}/", timeout=3)
            print(" ✓")
            return
        except Exception:
            print(".", end="", flush=True)
            time.sleep(2)
    print("\n✗ API não acessível. Confirma que o docker compose está up.")
    sys.exit(1)


# ─────────────────────────────────────────────────────────────────────────────
# Verificar se já existe infra (evitar duplicados)
# ─────────────────────────────────────────────────────────────────────────────

def check_existing():
    phases = get("ManufacturingPhase")
    lines  = get("ProductionLine")
    skids  = get("Support")

    if phases or lines or skids:
        print("\n⚠️  A BD já tem dados de infraestrutura:")
        print(f"   {len(phases)} fase(s), {len(lines)} linha(s), {len(skids)} suporte(s)")
        print("\n   Para recomeçar do zero, limpa a BD primeiro:")
        print("   docker exec -i drivolution-timescaledb-1 psql -U drivolution -d drivolution -c \"")
        print("   DELETE FROM phase_time_coefficient; DELETE FROM alert;")
        print("   DELETE FROM product_phase; DELETE FROM localization_history;")
        print("   DELETE FROM supported_product; DELETE FROM product_config;")
        print("   DELETE FROM product; DELETE FROM manufacturing_order;")
        print("   DELETE FROM client_order; DELETE FROM phase_sequence;")
        print("   DELETE FROM workstation; DELETE FROM support;")
        print("   DELETE FROM production_line; DELETE FROM manufacturing_phase;\"")
        print("\n   Depois corre este script novamente.")
        ans = input("\n   Continuar mesmo assim? (s/N): ").strip().lower()
        if ans != "s":
            sys.exit(0)


# ─────────────────────────────────────────────────────────────────────────────
# Criação da infra via API
# ─────────────────────────────────────────────────────────────────────────────

def create_phases():
    print("\n[1/5] A criar fases de fabrico...")
    phase_ids = []
    for p in PHASES:
        result = post("ManufacturingPhase", p)
        phase_ids.append(result["id"])
        print(f"   ✓ {p['name']} (id={result['id']}, estimatedDuration={p['estimatedDuration']}s)")
    return phase_ids


def create_lines():
    print("\n[2/5] A criar linhas de produção...")
    line_ids = []
    for l in LINES:
        result = post("ProductionLine", l)
        line_ids.append(result["id"])
        print(f"   ✓ {l['name']} (id={result['id']})")
    return line_ids


def create_workstations(phase_ids, line_ids):
    print("\n[3/5] A criar workstations...")
    ws_ids = []
    for ws in WORKSTATIONS:
        body = {
            "productionLineId":    line_ids[ws["line_index"]],
            "manufacturingPhaseId": phase_ids[ws["phase_index"]],
            "type": ws["type"],
            "kind": ws.get("kind", "machine"),
        }
        result = post("Workstation", body)
        ws_ids.append(result["id"])
        line_name = LINES[ws["line_index"]]["name"]
        phase_name = PHASES[ws["phase_index"]]["name"]
        print(f"   ✓ WS id={result['id']} | {line_name} | {phase_name} | type={ws['type']}")
    return ws_ids


def create_supports(line_ids):
    print("\n[4/5] A criar suportes (skids)...")
    supports = []
    for s in SKIDS:
        body = {
            "productionLineId": line_ids[s["line_index"]],
            "rfidTag": s["rfidTag"],
            "type": s["type"],
        }
        result = post("Support", body)
        supports.append({**result, "rfidTag": s["rfidTag"]})
        print(f"   ✓ {s['type']} | tag={s['rfidTag']} (id={result['id']})")
    return supports


# ─────────────────────────────────────────────────────────────────────────────
# FIWARE
# ─────────────────────────────────────────────────────────────────────────────

def fiware_clean():
    print("\n[5/5] A configurar FIWARE...")
    print("   A limpar estado anterior do FIWARE...")

    # Entidades Orion
    r = requests.get(f"{ORION_URL}/ngsi-ld/v1/entities?type=Skid&limit=100", headers=HEADERS_JSON)
    if r.status_code == 200:
        for entity in r.json():
            eid = entity.get("id", "")
            requests.delete(f"{ORION_URL}/ngsi-ld/v1/entities/{eid}", headers=HEADERS_JSON)

    # Subscrições Orion
    r = requests.get(f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100", headers=HEADERS_JSON)
    if r.status_code == 200:
        for sub in r.json():
            requests.delete(f"{ORION_URL}/ngsi-ld/v1/subscriptions/{sub['id']}", headers=HEADERS_JSON)

    # Devices IoT Agent
    r = requests.get(f"{IOT_AGENT_URL}/iot/devices?limit=100", headers=IOT_HEADERS)
    if r.status_code == 200:
        for device in r.json().get("devices", []):
            requests.delete(f"{IOT_AGENT_URL}/iot/devices/{device['device_id']}", headers=IOT_HEADERS)

    # Service group IoT Agent
    requests.delete(
        f"{IOT_AGENT_URL}/iot/services?resource={RESOURCE}&apikey={APIKEY}",
        headers=IOT_HEADERS,
    )
    print("   ✓ Limpeza FIWARE concluída")


def fiware_create_service_group():
    payload = {"services": [{"apikey": APIKEY, "cbroker": "http://orion:1026", "entity_type": "Skid", "resource": RESOURCE}]}
    r = requests.post(f"{IOT_AGENT_URL}/iot/services", headers=IOT_HEADERS, json=payload)
    if r.status_code in (200, 201, 409):
        print("   ✓ Service group IoT criado")
    else:
        print(f"   ✗ Service group ({r.status_code}): {r.text[:120]}")


def fiware_register_devices(supports):
    for s in supports:
        device_id = f"skid-{s['rfidTag']}"
        payload = {"devices": [{"device_id": device_id, "entity_name": f"urn:ngsi-ld:Skid:{device_id}",
            "entity_type": "Skid", "protocol": "PDI-IoTA-JSON", "transport": "HTTP",
            "attributes": [{"object_id": "currentWorkstation", "name": "currentWorkstation", "type": "Integer"}],
            "static_attributes": [
                {"name": "rfidTag",   "type": "Text",    "value": str(s["rfidTag"])},
                {"name": "supportId", "type": "Integer", "value": int(s["id"])},
                {"name": "skidType",  "type": "Text",    "value": s.get("type", "")},
            ],
        }]}
        r = requests.post(f"{IOT_AGENT_URL}/iot/devices", headers=IOT_HEADERS, json=payload)
        print(f"   {'✓' if r.status_code in (200,201) else '✗'} IoT device: {device_id}")


def fiware_create_entities(supports):
    for s in supports:
        entity_id = f"urn:ngsi-ld:Skid:skid-{s['rfidTag']}"
        payload = {
            "@context": CONTEXT_URL, "id": entity_id, "type": "Skid",
            "rfidTag":            {"type": "Property", "value": str(s["rfidTag"])},
            "supportId":          {"type": "Property", "value": int(s["id"])},
            "currentWorkstation": {"type": "Property", "value": 0},
            "skidType":           {"type": "Property", "value": s.get("type", "")},
        }
        r = requests.post(f"{ORION_URL}/ngsi-ld/v1/entities", headers=HEADERS_LD, data=json.dumps(payload))
        print(f"   {'✓' if r.status_code in (200,201) else '✗'} Orion entity: {entity_id[-30:]}")


def fiware_create_subscriptions():
    base_sub = {
        "@context": CONTEXT_URL, "type": "Subscription",
        "entities": [{"type": "Skid"}],
        "watchedAttributes": ["currentWorkstation"],
        "notification": {"attributes": ["rfidTag", "currentWorkstation", "supportId", "skidType"]},
    }

    sub_api = {**base_sub,
        "description": "DRIVOLUTION: skid muda → notifica API",
        "notification": {**base_sub["notification"], "endpoint": {"uri": API_NOTIFY_URL, "accept": "application/json"}},
    }
    sub_ql = {**base_sub,
        "description": "DRIVOLUTION: skid muda → QuantumLeap histórico",
        "notification": {**base_sub["notification"], "endpoint": {"uri": QL_NOTIFY_URL, "accept": "application/json"}},
    }

    for sub, name in [(sub_api, "API"), (sub_ql, "QuantumLeap")]:
        r = requests.post(f"{ORION_URL}/ngsi-ld/v1/subscriptions", headers=HEADERS_LD, data=json.dumps(sub))
        print(f"   {'✓' if r.status_code in (200,201) else '✗'} Subscrição {name}")


def fiware_setup(supports):
    fiware_clean()
    fiware_create_service_group()
    fiware_register_devices(supports)
    fiware_create_entities(supports)
    fiware_create_subscriptions()


# ─────────────────────────────────────────────────────────────────────────────
# Main
# ─────────────────────────────────────────────────────────────────────────────

def main():
    print("=" * 55)
    print("  DRIVOLUTION — Setup de Ambiente")
    print("=" * 55)

    wait_for_api()
    login()
    check_existing()

    phase_ids = create_phases()
    line_ids  = create_lines()
    create_workstations(phase_ids, line_ids)
    supports  = create_supports(line_ids)
    fiware_setup(supports)

    print("\n" + "=" * 55)
    print("✅ Setup completo!")
    print("\nPróximos passos (pela ordem):")
    print("  1. .\\scripts\\seed-fake-models.ps1       (criar modelos de carro)")
    print("  2. python scripts/seed_synthetic_history.py  (histórico ML)")
    print("  3. curl -X POST http://localhost:8080/api/ml/retrain  (treinar modelo)")
    print("  4. python scripts/seed_wip_state.py          (estado demo WIP)")
    print("  5. python agent/drivolution_agent.py --auto  (simular RFID)")
    print("=" * 55)


if __name__ == "__main__":
    main()