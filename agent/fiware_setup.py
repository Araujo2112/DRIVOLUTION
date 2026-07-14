"""
DRIVOLUTION - FIWARE Setup corrigido
"""

import os
import requests
import json
from dotenv import load_dotenv
load_dotenv()

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

# Badge (Card L — Presença por Workstation via FIWARE)
BADGE_APIKEY = "drivolution-badge-key"

_cached_token = None


def login():
    """Autentica contra a API DRIVOLUTION e devolve um JWT.
    Mesmo padrão já usado em drivolution_quality_agent.py:
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
    """Headers para chamadas à API DRIVOLUTION (não ao Orion/IoT Agent)."""
    h = {"Content-Type": "application/json"}
    token = login()
    if token:
        h["Authorization"] = f"Bearer {token}"
    return h


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
        # /Support é paginado (Data/Total/Page/PageSize) — /Support/all devolve
        # mesmo uma lista, que é o que este script precisa.
        r = requests.get(f"{API_BASE_URL}/Support/all", headers=api_headers(), timeout=5)
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


def load_users() -> list:
    """Carrega utilizadores com role operator/manager — só estes fazem sentido
    ter um crachá simulado (admin/client não pisam o chão de fábrica)."""
    print("\n[0/6] A carregar utilizadores (operator/manager) da API...")
    try:
        users = []
        for role in ("operator", "manager"):
            r = requests.get(
                f"{API_BASE_URL}/User",
                headers=api_headers(),
                params={"role": role, "pageSize": 100},
                timeout=5,
            )
            if r.status_code == 200:
                data = r.json()
                items = data.get("data") or data.get("Data") or data.get("$values", data)
                items = items if isinstance(items, list) else []
                users.extend(items)
            else:
                print(f"   ✗ Erro ao carregar role '{role}': {r.status_code}")

        print(f"   ✓ {len(users)} utilizador(es) elegível(eis) para crachá.")
        return users
    except Exception as e:
        print(f"   ✗ {e}")
        return []


def clean_all():
    print("\n[1/6] A limpar estado anterior...")

    # Apagar entidades Skid e Badge no Orion
    for entity_type in ("Skid", "Badge"):
        r = requests.get(
            f"{ORION_URL}/ngsi-ld/v1/entities?type={entity_type}&limit=100",
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

    # Apagar devices IoT Agent (skids e badges, todos vêm no mesmo /iot/devices)
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

    # Apagar service groups IoT Agent (skid + badge)
    for apikey in (APIKEY, BADGE_APIKEY):
        rd = requests.delete(
            f"{IOT_AGENT_URL}/iot/services?resource={RESOURCE}&apikey={apikey}",
            headers=IOT_HEADERS,
        )

        if rd.status_code in (200, 204):
            print(f"   ✓ Service group IoT removido ({apikey}).")
        elif rd.status_code == 404:
            print(f"   - Service group IoT não existia ({apikey}).")
        else:
            print(f"   ⚠ Service group IoT não removido ({apikey}, {rd.status_code}): {rd.text[:120]}")


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


def create_badge_iot_service_group():
    print("\n[2b/6] A criar service group de Badges no IoT Agent...")

    payload = {
        "services": [
            {
                "apikey": BADGE_APIKEY,
                "cbroker": "http://orion:1026",
                "entity_type": "Badge",
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
        print("   ✓ Service group de Badges criado.")
    elif r.status_code == 409:
        print("   ✓ Service group de Badges já existia.")
    else:
        print(f"   ✗ Erro ao criar service group de Badges ({r.status_code}): {r.text}")


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


def register_badge_devices(users: list):
    print("\n[3b/6] A registar crachás (Badge) no IoT Agent...")

    for user in users:
        device_id = f"badge-{user['id']}"

        payload = {
            "devices": [
                {
                    "device_id": device_id,
                    "entity_name": f"urn:ngsi-ld:Badge:{device_id}",
                    "entity_type": "Badge",
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
                            "name": "appUserId",
                            "type": "Integer",
                            "value": int(user["id"]),
                        },
                        {
                            "name": "userName",
                            "type": "Text",
                            "value": user.get("name", ""),
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

        print(f"   {'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} {device_id} ({user.get('name', '?')})")


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


def create_badge_entities(users: list):
    print("\n[4b/6] A criar entidades Badge no Orion...")

    for user in users:
        entity_id = f"urn:ngsi-ld:Badge:badge-{user['id']}"

        payload = {
            "@context": CONTEXT_URL,
            "id": entity_id,
            "type": "Badge",
            "appUserId": {
                "type": "Property",
                "value": int(user["id"]),
            },
            "userName": {
                "type": "Property",
                "value": user.get("name", ""),
            },
            # 0 = fora do chão de fábrica (sem presença ativa)
            "currentWorkstation": {
                "type": "Property",
                "value": 0,
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

    sub_badge = {
        "@context": CONTEXT_URL,
        "description": "DRIVOLUTION: crachá muda de workstation → notifica API (presença)",
        "type": "Subscription",
        "entities": [
            {
                "type": "Badge"
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
                "appUserId",
                "userName",
                "currentWorkstation",
            ],
        },
    }

    r3 = requests.post(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions",
        headers=HEADERS_LD,
        data=json.dumps(sub_badge),
    )

    if r3.status_code in (200, 201):
        print(f"   ✓ Subscrição Badge → API criada: {r3.headers.get('Location', '?')[-30:]}")
    else:
        print(f"   ✗ Subscrição Badge erro {r3.status_code}: {r3.text}")


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
        print(f"   Entidades Orion (Skid): {len(entities)}")
        for e in entities:
            ws = e.get("currentWorkstation", {}).get("value", "?")
            print(f"   - {e.get('id')} | WS: {ws}")

    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/entities?type=Badge&limit=50",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        entities = r.json()
        print(f"   Entidades Orion (Badge): {len(entities)}")
        for e in entities:
            ws = e.get("currentWorkstation", {}).get("value", "?")
            print(f"   - {e.get('id')} | WS: {ws}")

    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        n = len(r.json())
        print(f"   Subscrições Orion: {n} (deve ser 3 — API/Skid, QuantumLeap/Skid, API/Badge)")

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
    users = load_users()

    if not skids:
        print("\n✗ Sem suportes. Cria suportes no dashboard primeiro.")
        exit(1)

    if not users:
        print("⚠ Sem utilizadores operator/manager — crachás não serão criados (skids continuam a funcionar).")

    clean_all()
    create_iot_service_group()
    register_iot_devices(skids)
    create_entities(skids)

    if users:
        create_badge_iot_service_group()
        register_badge_devices(users)
        create_badge_entities(users)

    create_subscriptions()
    verify()

    print("\n✅ Setup completo. Próximo: python drivolution_agent.py / python drivolution_badge_agent.py")