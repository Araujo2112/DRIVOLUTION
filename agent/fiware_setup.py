"""
DRIVOLUTION - FIWARE Setup (dinâmico)
"""

import requests
import json

ORION_URL         = "http://localhost:1026"
API_BASE_URL      = "http://localhost:8080/api"
API_NOTIFY_URL    = "http://api:8080/api/FiwareNotification"
QL_NOTIFY_URL     = "http://quantumleap:8668/v2/notify"
CONTEXT_URL       = "https://ruicarvalho1.github.io/test-JsonLd/datamodels.context-ngsi.jsonld"

HEADERS_LD = {
    "Content-Type":       "application/ld+json",
    "FIWARE-Service":     "drivolution",
    "FIWARE-ServicePath": "/production",
}
HEADERS_JSON = {
    "FIWARE-Service":     "drivolution",
    "FIWARE-ServicePath": "/production",
}
IOT_HEADERS = {
    "Content-Type":       "application/json",
    "FIWARE-Service":     "drivolution",
    "FIWARE-ServicePath": "/production",
}


def load_skids() -> list:
    print("\n[0/5] A carregar suportes da API...")
    try:
        r = requests.get(f"{API_BASE_URL}/Support", timeout=5)
        if r.status_code == 200:
            data  = r.json()
            skids = data.get("$values", data) if isinstance(data, dict) else data
            skids = skids if isinstance(skids, list) else []
            print(f"   ✓ {len(skids)} suporte(s) encontrados.")
            return skids
        print(f"   ✗ Erro {r.status_code}")
        return []
    except Exception as e:
        print(f"   ✗ {e}")
        return []


def clean_all():
    print("\n[1/5] A limpar estado anterior...")

    # Apagar entidades Skid
    r = requests.get(f"{ORION_URL}/ngsi-ld/v1/entities?type=Skid&limit=100", headers=HEADERS_JSON)
    if r.status_code == 200:
        for entity in r.json():
            eid = entity.get("id", "")
            rd  = requests.delete(f"{ORION_URL}/ngsi-ld/v1/entities/{eid}", headers=HEADERS_JSON)
            print(f"   {'✓' if rd.status_code in (200,204) else '✗'} Entidade: {eid}")

    # Apagar subscrições
    r = requests.get(f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100", headers=HEADERS_JSON)
    if r.status_code == 200:
        subs = r.json()
        print(f"   Subscrições a remover: {len(subs)}")
        for sub in subs:
            full_id = sub.get("id", "")
            rd = requests.delete(
                f"{ORION_URL}/ngsi-ld/v1/subscriptions/{full_id}",
                headers=HEADERS_JSON
            )
            print(f"   {'✓' if rd.status_code in (200,204) else f'✗({rd.status_code})'} Sub: {full_id[-20:]}")

    # Apagar dispositivos IoT Agent
    r = requests.get("http://localhost:4041/iot/devices?limit=100", headers=IOT_HEADERS)
    if r.status_code == 200:
        for device in r.json().get("devices", []):
            device_id = device.get("device_id", "")
            rd = requests.delete(f"http://localhost:4041/iot/devices/{device_id}", headers=IOT_HEADERS)
            print(f"   {'✓' if rd.status_code in (200,204) else '✗'} Dispositivo IoT: {device_id}")


def register_iot_devices(skids: list):
    print("\n[2/5] A registar dispositivos no IoT Agent...")
    for skid in skids:
        if not skid.get("rfidTag"):
            print(f"   ⚠ Suporte ID {skid['id']} sem tag RFID — a ignorar.")
            continue
        device_id = f"skid-{skid['rfidTag']}"
        payload = {
            "devices": [{
                "device_id":   device_id,
                "entity_name": f"urn:ngsi-ld:Skid:{device_id}",
                "entity_type": "Skid",
                "protocol":    "IoTA-JSON",
                "transport":   "HTTP",
                "attributes": [
                    {"object_id": "rfidTag",            "name": "rfidTag",            "type": "Property"},
                    {"object_id": "currentWorkstation", "name": "currentWorkstation", "type": "Property"},
                    {"object_id": "productId",          "name": "productId",          "type": "Property"},
                    {"object_id": "supportId",          "name": "supportId",          "type": "Property"},
                ],
                "static_attributes": [
                    {"name": "rfidTag",   "type": "Property", "value": skid["rfidTag"]},
                    {"name": "supportId", "type": "Property", "value": skid["id"]},
                ]
            }]
        }
        r = requests.post("http://localhost:4041/iot/devices", headers=IOT_HEADERS, json=payload)
        print(f"   {'✓' if r.status_code in (200,201) else f'✗({r.status_code})'} {device_id}")


def create_entities(skids: list):
    print("\n[3/5] A criar entidades no Orion...")
    for skid in skids:
        if not skid.get("rfidTag"):
            continue
        entity_id = f"urn:ngsi-ld:Skid:skid-{skid['rfidTag']}"
        payload = {
            "@context": CONTEXT_URL,
            "id":   entity_id,
            "type": "Skid",
            "rfidTag":            {"type": "Property", "value": skid["rfidTag"]},
            "supportId":          {"type": "Property", "value": skid["id"]},
            "currentWorkstation": {"type": "Property", "value": 0},
            "productId":          {"type": "Property", "value": 0},
            "skidType":           {"type": "Property", "value": skid.get("type", "")},
        }
        r = requests.post(f"{ORION_URL}/ngsi-ld/v1/entities", headers=HEADERS_LD, data=json.dumps(payload))
        print(f"   {'✓' if r.status_code in (200,201) else f'✗({r.status_code}) ' + r.text[:80]} {entity_id}")


def create_subscriptions():
    print("\n[4/5] A criar subscrições...")

    # Subscrição 1 — Notifica a API Drivolution
    sub_api = {
        "@context": CONTEXT_URL,
        "description": "DRIVOLUTION: skid muda de workstation → notifica API",
        "type": "Subscription",
        "entities": [{"type": "Skid"}],
        "watchedAttributes": ["currentWorkstation"],
        "notification": {
            "endpoint": {
                "uri":    API_NOTIFY_URL,
                "accept": "application/json",
            },
            "attributes": ["rfidTag", "currentWorkstation", "productId", "supportId"],
        }
    }
    r1 = requests.post(f"{ORION_URL}/ngsi-ld/v1/subscriptions", headers=HEADERS_LD, data=json.dumps(sub_api))
    if r1.status_code in (200, 201):
        print(f"   ✓ Subscrição API criada: {r1.headers.get('Location', '?')[-30:]}")
    else:
        print(f"   ✗ Subscrição API erro {r1.status_code}: {r1.text}")

    # Subscrição 2 — Notifica o QuantumLeap (histórico temporal)
    sub_ql = {
        "@context": CONTEXT_URL,
        "description": "DRIVOLUTION: skid muda → QuantumLeap guarda histórico",
        "type": "Subscription",
        "entities": [{"type": "Skid"}],
        "watchedAttributes": ["currentWorkstation", "productId"],
        "notification": {
            "endpoint": {
                "uri":    QL_NOTIFY_URL,
                "accept": "application/json",
            },
            "attributes": ["rfidTag", "currentWorkstation", "productId", "supportId", "skidType"],
        }
    }
    r2 = requests.post(f"{ORION_URL}/ngsi-ld/v1/subscriptions", headers=HEADERS_LD, data=json.dumps(sub_ql))
    if r2.status_code in (200, 201):
        print(f"   ✓ Subscrição QuantumLeap criada: {r2.headers.get('Location', '?')[-30:]}")
    else:
        print(f"   ✗ Subscrição QuantumLeap erro {r2.status_code}: {r2.text}")


def verify(skids: list):
    print("\n[5/5] Verificação final...")

    r = requests.get(f"{ORION_URL}/ngsi-ld/v1/entities?type=Skid&limit=50", headers=HEADERS_JSON)
    if r.status_code == 200:
        entities = r.json()
        print(f"   Entidades no Orion: {len(entities)}")
        for e in entities:
            ws = e.get("currentWorkstation", {}).get("value", "?")
            print(f"   - {e['id']} | WS: {ws}")

    r2 = requests.get(f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100", headers=HEADERS_JSON)
    if r2.status_code == 200:
        n    = len(r2.json())
        icon = "✓" if n == 2 else "⚠"
        print(f"   {icon} Subscrições ativas: {n} (deve ser 2 — API + QuantumLeap)")

    # Verificar se o QuantumLeap está acessível
    try:
        r3 = requests.get("http://localhost:8668/version", timeout=3)
        if r3.status_code == 200:
            print(f"   ✓ QuantumLeap acessível: v{r3.json().get('version', '?')}")
        else:
            print(f"   ✗ QuantumLeap não responde ({r3.status_code})")
    except:
        print(f"   ✗ QuantumLeap inacessível")


if __name__ == "__main__":
    print("=" * 55)
    print("  DRIVOLUTION - FIWARE Setup")
    print("=" * 55)
    skids = load_skids()
    if not skids:
        print("\n✗ Sem suportes. Cria suportes no dashboard primeiro.")
        exit(1)
    clean_all()
    register_iot_devices(skids)
    create_entities(skids)
    create_subscriptions()
    verify(skids)
    print("\n✅ Setup completo. Próximo: python drivolution_agent.py")