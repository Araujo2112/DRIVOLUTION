"""
DRIVOLUTION - FIWARE Setup corrigido
"""

import requests
import json

ORION_URL = "http://localhost:1026"
IOT_AGENT_URL = "http://localhost:4041"
API_BASE_URL = "http://localhost:8080/api"

API_NOTIFY_URL = "http://api:8080/api/FiwareNotification"
QL_NOTIFY_URL = "http://quantumleap:8668/v2/notify"

CONTEXT_URL = "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld"

FIWARE_SERVICE = "drivolution"
FIWARE_SERVICEPATH = "/"
APIKEY = "drivolution-key"
RESOURCE = "/iot/json"

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


def load_skids() -> list:
    print("\n[0/6] A carregar suportes da API...")
    try:
        r = requests.get(f"{API_BASE_URL}/Support", timeout=5)
        if r.status_code == 200:
            data = r.json()
            skids = data.get("$values", data) if isinstance(data, dict) else data
            skids = skids if isinstance(skids, list) else []
            print(f"   ✓ {len(skids)} suporte(s) encontrados.")
            return skids

        print(f"   ✗ Erro {r.status_code}: {r.text}")
        return []
    except Exception as e:
        print(f"   ✗ {e}")
        return []


def clean_all():
    print("\n[1/6] A limpar estado anterior...")

    # Apagar entidades Skid no Orion
    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/entities?type=Skid&limit=100",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        for entity in r.json():
            eid = entity.get("id", "")
            rd = requests.delete(
                f"{ORION_URL}/ngsi-ld/v1/entities/{eid}",
                headers=HEADERS_JSON,
            )
            print(f"   {'✓' if rd.status_code in (200, 204) else '✗'} Entidade: {eid}")

    # Apagar subscrições Orion
    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        subs = r.json()
        print(f"   Subscrições a remover: {len(subs)}")

        for sub in subs:
            sid = sub.get("id", "")
            rd = requests.delete(
                f"{ORION_URL}/ngsi-ld/v1/subscriptions/{sid}",
                headers=HEADERS_JSON,
            )
            print(f"   {'✓' if rd.status_code in (200, 204) else f'✗({rd.status_code})'} Sub: {sid[-20:]}")

    # Apagar devices IoT Agent
    r = requests.get(
        f"{IOT_AGENT_URL}/iot/devices?limit=100",
        headers=IOT_HEADERS,
    )

    if r.status_code == 200:
        devices = r.json().get("devices", [])
        print(f"   Devices IoT a remover: {len(devices)}")

        for device in devices:
            device_id = device.get("device_id", "")
            rd = requests.delete(
                f"{IOT_AGENT_URL}/iot/devices/{device_id}",
                headers=IOT_HEADERS,
            )
            print(f"   {'✓' if rd.status_code in (200, 204) else f'✗({rd.status_code})'} Device: {device_id}")

    # Apagar service group IoT Agent
    rd = requests.delete(
        f"{IOT_AGENT_URL}/iot/services?resource={RESOURCE}&apikey={APIKEY}",
        headers=IOT_HEADERS,
    )

    if rd.status_code in (200, 204):
        print("   ✓ Service group IoT removido.")
    elif rd.status_code == 404:
        print("   - Service group IoT não existia.")
    else:
        print(f"   ⚠ Service group IoT não removido ({rd.status_code}): {rd.text[:120]}")


def create_iot_service_group():
    print("\n[2/6] A criar service group no IoT Agent...")

    payload = {
        "services": [
            {
                "apikey": APIKEY,
                "cbroker": "http://orion:1026",
                "entity_type": "Skid",
                "resource": RESOURCE,
            }
        ]
    }

    r = requests.post(
        f"{IOT_AGENT_URL}/iot/services",
        headers=IOT_HEADERS,
        json=payload,
    )

    if r.status_code in (200, 201):
        print("   ✓ Service group criado.")
    elif r.status_code == 409:
        print("   ✓ Service group já existia.")
    else:
        print(f"   ✗ Erro ao criar service group ({r.status_code}): {r.text}")


def register_iot_devices(skids: list):
    print("\n[3/6] A registar dispositivos no IoT Agent...")

    for skid in skids:
        if not skid.get("rfidTag"):
            print(f"   ⚠ Suporte ID {skid.get('id')} sem tag RFID — ignorado.")
            continue

        device_id = f"skid-{skid['rfidTag']}"

        payload = {
            "devices": [
                {
                    "device_id": device_id,
                    "entity_name": f"urn:ngsi-ld:Skid:{device_id}",
                    "entity_type": "Skid",
                    "protocol": "PDI-IoTA-JSON",
                    "transport": "HTTP",
                    "attributes": [
                        {
                            "object_id": "currentWorkstation",
                            "name": "currentWorkstation",
                            "type": "Integer",
                        }
                    ],
                    "static_attributes": [
                        {
                            "name": "rfidTag",
                            "type": "Text",
                            "value": str(skid["rfidTag"]),
                        },
                        {
                            "name": "supportId",
                            "type": "Integer",
                            "value": int(skid["id"]),
                        },
                        {
                            "name": "skidType",
                            "type": "Text",
                            "value": skid.get("type", ""),
                        },
                    ],
                }
            ]
        }

        r = requests.post(
            f"{IOT_AGENT_URL}/iot/devices",
            headers=IOT_HEADERS,
            json=payload,
        )

        print(f"   {'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} {device_id}")


def create_entities(skids: list):
    print("\n[4/6] A criar entidades no Orion...")

    for skid in skids:
        if not skid.get("rfidTag"):
            continue

        entity_id = f"urn:ngsi-ld:Skid:skid-{skid['rfidTag']}"

        payload = {
            "@context": CONTEXT_URL,
            "id": entity_id,
            "type": "Skid",
            "rfidTag": {
                "type": "Property",
                "value": str(skid["rfidTag"]),
            },
            "supportId": {
                "type": "Property",
                "value": int(skid["id"]),
            },
            "currentWorkstation": {
                "type": "Property",
                "value": 0,
            },
            "skidType": {
                "type": "Property",
                "value": skid.get("type", ""),
            },
        }

        r = requests.post(
            f"{ORION_URL}/ngsi-ld/v1/entities",
            headers=HEADERS_LD,
            data=json.dumps(payload),
        )

        print(f"   {'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} {entity_id}")


def create_subscriptions():
    print("\n[5/6] A criar subscrições...")

    sub_api = {
        "@context": CONTEXT_URL,
        "description": "DRIVOLUTION: skid muda de workstation → notifica API",
        "type": "Subscription",
        "entities": [
            {
                "type": "Skid"
            }
        ],
        "watchedAttributes": [
            "currentWorkstation"
        ],
        "notification": {
            "endpoint": {
                "uri": API_NOTIFY_URL,
                "accept": "application/json",
            },
            "attributes": [
                "rfidTag",
                "currentWorkstation",
                "supportId",
                "skidType",
            ],
        },
    }

    r1 = requests.post(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions",
        headers=HEADERS_LD,
        data=json.dumps(sub_api),
    )

    if r1.status_code in (200, 201):
        print(f"   ✓ Subscrição API criada: {r1.headers.get('Location', '?')[-30:]}")
    else:
        print(f"   ✗ Subscrição API erro {r1.status_code}: {r1.text}")

    sub_ql = {
        "@context": CONTEXT_URL,
        "description": "DRIVOLUTION: skid muda → QuantumLeap guarda histórico",
        "type": "Subscription",
        "entities": [
            {
                "type": "Skid"
            }
        ],
        "watchedAttributes": [
            "currentWorkstation"
        ],
        "notification": {
            "endpoint": {
                "uri": QL_NOTIFY_URL,
                "accept": "application/json",
            },
            "attributes": [
                "rfidTag",
                "currentWorkstation",
                "supportId",
                "skidType",
            ],
        },
    }

    r2 = requests.post(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions",
        headers=HEADERS_LD,
        data=json.dumps(sub_ql),
    )

    if r2.status_code in (200, 201):
        print(f"   ✓ Subscrição QuantumLeap criada: {r2.headers.get('Location', '?')[-30:]}")
    else:
        print(f"   ✗ Subscrição QuantumLeap erro {r2.status_code}: {r2.text}")


def verify():
    print("\n[6/6] Verificação final...")

    r = requests.get(
        f"{IOT_AGENT_URL}/iot/services",
        headers=IOT_HEADERS,
    )

    if r.status_code == 200:
        print(f"   Service groups IoT: {r.json().get('count', 0)}")
    else:
        print(f"   ✗ Erro ao verificar services IoT: {r.status_code}")

    r = requests.get(
        f"{IOT_AGENT_URL}/iot/devices",
        headers=IOT_HEADERS,
    )

    if r.status_code == 200:
        print(f"   Devices IoT: {r.json().get('count', 0)}")
    else:
        print(f"   ✗ Erro ao verificar devices IoT: {r.status_code}")

    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/entities?type=Skid&limit=50",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        entities = r.json()
        print(f"   Entidades Orion: {len(entities)}")
        for e in entities:
            ws = e.get("currentWorkstation", {}).get("value", "?")
            print(f"   - {e.get('id')} | WS: {ws}")

    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        n = len(r.json())
        print(f"   Subscrições Orion: {n} (deve ser 2)")

    try:
        r = requests.get("http://localhost:8668/version", timeout=3)
        if r.status_code == 200:
            print(f"   ✓ QuantumLeap acessível: v{r.json().get('version', '?')}")
        else:
            print(f"   ⚠ QuantumLeap respondeu {r.status_code}")
    except Exception:
        print("   ⚠ QuantumLeap inacessível")


if __name__ == "__main__":
    print("=" * 55)
    print("  DRIVOLUTION - FIWARE Setup")
    print("=" * 55)

    skids = load_skids()

    if not skids:
        print("\n✗ Sem suportes. Cria suportes no dashboard primeiro.")
        exit(1)

    clean_all()
    create_iot_service_group()
    register_iot_devices(skids)
    create_entities(skids)
    create_subscriptions()
    verify()

    print("\n✅ Setup completo. Próximo: python drivolution_agent.py")