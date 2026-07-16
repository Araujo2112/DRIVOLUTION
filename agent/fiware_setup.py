"""
DRIVOLUTION - FIWARE Setup corrigido
Configura automaticamente a infraestrutura FIWARE do projeto, registando skids e crachás como dispositivos IoT, 
criando as respetivas entidades no Orion e definindo subscrições para enviar atualizações à API e ao QuantumLeap.
"""

import os
import requests
import json
from dotenv import load_dotenv

# Carrega as variáveis definidas no ficheiro .env
load_dotenv()


# Endereço do Orion Context Broker
ORION_URL = "http://localhost:1026"

# Endereço da API de gestão do IoT Agent
IOT_AGENT_URL = "http://localhost:4041"

# Endereço da API principal DRIVOLUTION
API_BASE_URL = "http://localhost:8080/api"

# Endereço interno usado pelo Orion para enviar notificações à API.
# Como os serviços estão dentro do Docker, utiliza-se o nome "api"
# em vez de localhost.
API_NOTIFY_URL = "http://api:8080/api/FiwareNotification"

# Endereço interno usado para enviar alterações ao QuantumLeap,
# responsável por guardar o histórico temporal.
QL_NOTIFY_URL = "http://quantumleap:8668/v2/notify"

# Contexto padrão NGSI-LD usado na criação das entidades
CONTEXT_URL = "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld"

# Identificação do serviço e do caminho FIWARE
FIWARE_SERVICE = "drivolution"
FIWARE_SERVICEPATH = "/"

# Chave usada pelos dispositivos do tipo Skid
APIKEY = "drivolution-key"

# Recurso HTTP pelo qual o IoT Agent recebe dados
RESOURCE = "/iot/json"

# Badge (Card L — Presença por Workstation via FIWARE)
# Chave específica usada pelos dispositivos do tipo Badge
BADGE_APIKEY = "drivolution-badge-key"

# Guarda o token JWT depois do primeiro login
_cached_token = None


def login():
    """Autentica contra a API DRIVOLUTION e devolve um JWT.
    Mesmo padrão já usado em drivolution_quality_agent.py:
    DRIVOLUTION_EMAIL / DRIVOLUTION_PASSWORD, ou DRIVOLUTION_TOKEN direto."""

    # Permite alterar a variável global
    global _cached_token

    # Se já existe um token guardado, reutiliza-o
    if _cached_token:
        return _cached_token

    # Primeiro tenta obter diretamente um token do ficheiro .env
    manual_token = os.getenv("DRIVOLUTION_TOKEN")

    if manual_token:
        # Remove "Bearer " caso tenha sido colocado no valor
        _cached_token = manual_token.replace("Bearer ", "").strip()
        return _cached_token

    # Se não existir token manual, procura email e password
    email = os.getenv("DRIVOLUTION_EMAIL")
    password = os.getenv("DRIVOLUTION_PASSWORD")

    # Se não existirem credenciais, não é possível autenticar
    if not email or not password:
        print("   ⚠ Sem autenticação definida.")
        print("   Define DRIVOLUTION_EMAIL e DRIVOLUTION_PASSWORD ou DRIVOLUTION_TOKEN.")
        return None

    # Faz login através da API
    r = requests.post(
        f"{API_BASE_URL}/Auth/login",
        json={
            "email": email,
            "password": password
        },
        timeout=10,
    )

    # Verifica se o login foi aceite
    if r.status_code not in (200, 201):
        print(f"   ✗ Login falhou ({r.status_code}): {r.text}")
        return None

    # Obtém o token da resposta
    token = r.json().get("token")

    if not token:
        print(f"   ✗ Login respondeu mas sem token: {r.json()}")
        return None

    # Guarda o token para os próximos pedidos
    _cached_token = token

    print("   ✓ Login efetuado com sucesso.")

    return _cached_token


def api_headers():
    """Headers para chamadas à API DRIVOLUTION (não ao Orion/IoT Agent)."""

    # Cabeçalho base para pedidos JSON
    h = {
        "Content-Type": "application/json"
    }

    # Obtém o token JWT
    token = login()

    # Se existir token, acrescenta-o ao pedido
    if token:
        h["Authorization"] = f"Bearer {token}"

    return h


# Cabeçalhos usados para criar entidades e subscrições NGSI-LD
HEADERS_LD = {
    "Content-Type": "application/ld+json",
    "FIWARE-Service": FIWARE_SERVICE,
    "FIWARE-ServicePath": FIWARE_SERVICEPATH,
}

# Cabeçalhos usados para consultar e apagar recursos no Orion
HEADERS_JSON = {
    "FIWARE-Service": FIWARE_SERVICE,
    "FIWARE-ServicePath": FIWARE_SERVICEPATH,
}

# Cabeçalhos usados na API de gestão do IoT Agent
IOT_HEADERS = {
    "Content-Type": "application/json",
    "FIWARE-Service": FIWARE_SERVICE,
    "FIWARE-ServicePath": FIWARE_SERVICEPATH,
}


def load_skids() -> list:
    # Obtém os suportes existentes na aplicação
    print("\n[0/6] A carregar suportes da API...")

    try:
        # /Support é paginado (Data/Total/Page/PageSize) — /Support/all devolve
        # mesmo uma lista, que é o que este script precisa.
        r = requests.get(
            f"{API_BASE_URL}/Support/all",
            headers=api_headers(),
            timeout=5
        )

        if r.status_code == 200:
            data = r.json()

            # Trata respostas normais e respostas com "$values"
            skids = (
                data.get("$values", data)
                if isinstance(data, dict)
                else data
            )

            # Garante que o resultado é uma lista
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

        # Faz uma pesquisa separada para operadores e managers
        for role in ("operator", "manager"):
            r = requests.get(
                f"{API_BASE_URL}/User",
                headers=api_headers(),
                params={
                    "role": role,
                    "pageSize": 100
                },
                timeout=5,
            )

            if r.status_code == 200:
                data = r.json()

                # Tenta encontrar a lista nos vários formatos possíveis
                items = (
                    data.get("data")
                    or data.get("Data")
                    or data.get("$values", data)
                )

                items = items if isinstance(items, list) else []

                # Junta os utilizadores encontrados
                users.extend(items)

            else:
                print(
                    f"   ✗ Erro ao carregar role "
                    f"'{role}': {r.status_code}"
                )

        print(
            f"   ✓ {len(users)} utilizador(es) "
            "elegível(eis) para crachá."
        )

        return users

    except Exception as e:
        print(f"   ✗ {e}")
        return []


def clean_all():
    # Remove configurações anteriores para que o setup
    # possa ser executado novamente sem duplicações
    print("\n[1/6] A limpar estado anterior...")

    # Apagar entidades Skid e Badge no Orion
    for entity_type in ("Skid", "Badge"):
        # Obtém as entidades do tipo atual
        r = requests.get(
            f"{ORION_URL}/ngsi-ld/v1/entities"
            f"?type={entity_type}&limit=100",
            headers=HEADERS_JSON,
        )

        if r.status_code == 200:
            # Apaga cada entidade individualmente
            for entity in r.json():
                eid = entity.get("id", "")

                rd = requests.delete(
                    f"{ORION_URL}/ngsi-ld/v1/entities/{eid}",
                    headers=HEADERS_JSON,
                )

                print(
                    f"   {'✓' if rd.status_code in (200, 204) else '✗'} "
                    f"Entidade: {eid}"
                )

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

            print(
                f"   "
                f"{'✓' if rd.status_code in (200, 204) else f'✗({rd.status_code})'} "
                f"Sub: {sid[-20:]}"
            )

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

            print(
                f"   "
                f"{'✓' if rd.status_code in (200, 204) else f'✗({rd.status_code})'} "
                f"Device: {device_id}"
            )

    # Apagar service groups IoT Agent (skid + badge)
    for apikey in (APIKEY, BADGE_APIKEY):
        rd = requests.delete(
            f"{IOT_AGENT_URL}/iot/services"
            f"?resource={RESOURCE}&apikey={apikey}",
            headers=IOT_HEADERS,
        )

        if rd.status_code in (200, 204):
            print(
                f"   ✓ Service group IoT removido "
                f"({apikey})."
            )

        elif rd.status_code == 404:
            print(
                f"   - Service group IoT não existia "
                f"({apikey})."
            )

        else:
            print(
                f"   ⚠ Service group IoT não removido "
                f"({apikey}, {rd.status_code}): "
                f"{rd.text[:120]}"
            )


def create_iot_service_group():
    # Cria a configuração comum aos dispositivos do tipo Skid
    print("\n[2/6] A criar service group no IoT Agent...")

    payload = {
        "services": [
            {
                # Chave que os skids têm de usar
                "apikey": APIKEY,

                # Endereço interno do Orion
                "cbroker": "http://orion:1026",

                # Tipo das entidades criadas
                "entity_type": "Skid",

                # Endpoint usado para receber dados
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
        # 409 significa que já existe uma configuração igual
        print("   ✓ Service group já existia.")

    else:
        print(
            f"   ✗ Erro ao criar service group "
            f"({r.status_code}): {r.text}"
        )


def create_badge_iot_service_group():
    # Cria a configuração comum aos dispositivos do tipo Badge
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
        print(
            f"   ✗ Erro ao criar service group de Badges "
            f"({r.status_code}): {r.text}"
        )


def register_iot_devices(skids: list):
    # Regista cada skid como um dispositivo IoT
    print("\n[3/6] A registar dispositivos no IoT Agent...")

    for skid in skids:
        # Um suporte sem RFID não pode ser registado como dispositivo
        if not skid.get("rfidTag"):
            print(
                f"   ⚠ Suporte ID {skid.get('id')} "
                "sem tag RFID — ignorado."
            )
            continue

        # Cria um ID único para o dispositivo
        device_id = f"skid-{skid['rfidTag']}"

        payload = {
            "devices": [
                {
                    # ID do dispositivo no IoT Agent
                    "device_id": device_id,

                    # ID da entidade correspondente no Orion
                    "entity_name": f"urn:ngsi-ld:Skid:{device_id}",

                    "entity_type": "Skid",
                    "protocol": "PDI-IoTA-JSON",
                    "transport": "HTTP",

                    # Atributos que podem ser atualizados pelos eventos
                    "attributes": [
                        {
                            "object_id": "currentWorkstation",
                            "name": "currentWorkstation",
                            "type": "Integer",
                        }
                    ],

                    # Atributos que permanecem fixos
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

        print(
            f"   "
            f"{'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} "
            f"{device_id}"
        )


def register_badge_devices(users: list):
    # Regista um crachá para cada operador ou manager
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

                    # A workstation pode mudar quando o crachá é lido
                    "attributes": [
                        {
                            "object_id": "currentWorkstation",
                            "name": "currentWorkstation",
                            "type": "Integer",
                        }
                    ],

                    # O utilizador associado ao crachá é fixo
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

        print(
            f"   "
            f"{'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} "
            f"{device_id} ({user.get('name', '?')})"
        )


def create_entities(skids: list):
    # Cria no Orion uma entidade inicial para cada skid
    print("\n[4/6] A criar entidades no Orion...")

    for skid in skids:
        if not skid.get("rfidTag"):
            continue

        entity_id = (
            f"urn:ngsi-ld:Skid:"
            f"skid-{skid['rfidTag']}"
        )

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

            # Começa na workstation 0,
            # significando que ainda não está numa estação real
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

            # Converte o dicionário Python para texto JSON
            data=json.dumps(payload),
        )

        print(
            f"   "
            f"{'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} "
            f"{entity_id}"
        )


def create_badge_entities(users: list):
    # Cria no Orion uma entidade inicial para cada crachá
    print("\n[4b/6] A criar entidades Badge no Orion...")

    for user in users:
        entity_id = (
            f"urn:ngsi-ld:Badge:"
            f"badge-{user['id']}"
        )

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

        print(
            f"   "
            f"{'✓' if r.status_code in (200, 201) else f'✗({r.status_code}) ' + r.text[:120]} "
            f"{entity_id}"
        )


def create_subscriptions():
    # Configura o Orion para enviar notificações
    # quando determinados atributos forem alterados
    print("\n[5/6] A criar subscrições...")

    # Subscrição que envia alterações dos skids para a API
    sub_api = {
        "@context": CONTEXT_URL,
        "description": "DRIVOLUTION: skid muda de workstation → notifica API",
        "type": "Subscription",

        # Observa todas as entidades do tipo Skid
        "entities": [
            {
                "type": "Skid"
            }
        ],

        # Só dispara quando currentWorkstation é alterado
        "watchedAttributes": [
            "currentWorkstation"
        ],

        "notification": {
            "endpoint": {
                "uri": API_NOTIFY_URL,
                "accept": "application/json",
            },

            # Atributos enviados na notificação
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
        print(
            f"   ✓ Subscrição API criada: "
            f"{r1.headers.get('Location', '?')[-30:]}"
        )
    else:
        print(
            f"   ✗ Subscrição API erro "
            f"{r1.status_code}: {r1.text}"
        )

    # Subscrição que envia alterações dos skids para o QuantumLeap
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
        print(
            f"   ✓ Subscrição QuantumLeap criada: "
            f"{r2.headers.get('Location', '?')[-30:]}"
        )
    else:
        print(
            f"   ✗ Subscrição QuantumLeap erro "
            f"{r2.status_code}: {r2.text}"
        )

    # Subscrição que envia alterações dos crachás para a API
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
        print(
            f"   ✓ Subscrição Badge → API criada: "
            f"{r3.headers.get('Location', '?')[-30:]}"
        )
    else:
        print(
            f"   ✗ Subscrição Badge erro "
            f"{r3.status_code}: {r3.text}"
        )


def verify():
    # Confirma se o setup foi realizado corretamente
    print("\n[6/6] Verificação final...")

    # Verifica os service groups existentes
    r = requests.get(
        f"{IOT_AGENT_URL}/iot/services",
        headers=IOT_HEADERS,
    )

    if r.status_code == 200:
        print(
            f"   Service groups IoT: "
            f"{r.json().get('count', 0)}"
        )
    else:
        print(
            f"   ✗ Erro ao verificar services IoT: "
            f"{r.status_code}"
        )

    # Verifica os dispositivos registados
    r = requests.get(
        f"{IOT_AGENT_URL}/iot/devices",
        headers=IOT_HEADERS,
    )

    if r.status_code == 200:
        print(
            f"   Devices IoT: "
            f"{r.json().get('count', 0)}"
        )
    else:
        print(
            f"   ✗ Erro ao verificar devices IoT: "
            f"{r.status_code}"
        )

    # Verifica as entidades Skid existentes no Orion
    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/entities"
        "?type=Skid&limit=50",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        entities = r.json()

        print(
            f"   Entidades Orion (Skid): "
            f"{len(entities)}"
        )

        for e in entities:
            ws = (
                e.get("currentWorkstation", {})
                .get("value", "?")
            )

            print(
                f"   - {e.get('id')} | WS: {ws}"
            )

    # Verifica as entidades Badge existentes no Orion
    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/entities"
        "?type=Badge&limit=50",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        entities = r.json()

        print(
            f"   Entidades Orion (Badge): "
            f"{len(entities)}"
        )

        for e in entities:
            ws = (
                e.get("currentWorkstation", {})
                .get("value", "?")
            )

            print(
                f"   - {e.get('id')} | WS: {ws}"
            )

    # Confirma que foram criadas as três subscrições esperadas
    r = requests.get(
        f"{ORION_URL}/ngsi-ld/v1/subscriptions?limit=100",
        headers=HEADERS_JSON,
    )

    if r.status_code == 200:
        n = len(r.json())

        print(
            f"   Subscrições Orion: {n} "
            "(deve ser 3 — API/Skid, "
            "QuantumLeap/Skid, API/Badge)"
        )

    # Verifica se o QuantumLeap está acessível
    try:
        r = requests.get(
            "http://localhost:8668/version",
            timeout=3
        )

        if r.status_code == 200:
            print(
                f"   ✓ QuantumLeap acessível: "
                f"v{r.json().get('version', '?')}"
            )
        else:
            print(
                f"   ⚠ QuantumLeap respondeu "
                f"{r.status_code}"
            )

    except Exception:
        print("   ⚠ QuantumLeap inacessível")


# Só executa este bloco quando o ficheiro é iniciado diretamente
if __name__ == "__main__":
    print("=" * 55)
    print("  DRIVOLUTION - FIWARE Setup")
    print("=" * 55)

    # Obtém os suportes e utilizadores existentes na aplicação
    skids = load_skids()
    users = load_users()

    # Sem suportes não é possível configurar os dispositivos RFID
    if not skids:
        print(
            "\n✗ Sem suportes. "
            "Cria suportes no dashboard primeiro."
        )
        exit(1)

    # Os crachás são opcionais.
    # Se não houver utilizadores, os skids continuam a funcionar.
    if not users:
        print(
            "⚠ Sem utilizadores operator/manager — "
            "crachás não serão criados "
            "(skids continuam a funcionar)."
        )

    # Limpa configurações anteriores
    clean_all()

    # Configura os skids no IoT Agent e no Orion
    create_iot_service_group()
    register_iot_devices(skids)
    create_entities(skids)

    # Se existirem utilizadores elegíveis,
    # configura também os crachás
    if users:
        create_badge_iot_service_group()
        register_badge_devices(users)
        create_badge_entities(users)

    # Cria as subscrições entre Orion, API e QuantumLeap
    create_subscriptions()

    # Confirma o resultado final
    verify()

    print(
        "\n✅ Setup completo. Próximo: "
        "python drivolution_agent.py / "
        "python drivolution_badge_agent.py"
    )