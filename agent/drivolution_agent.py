"""
DRIVOLUTION - Agente Simulado RFID
Carrega skids e workstations dinamicamente da API.
"""

import argparse
import json
import os
import time
import requests

# Permite carregar variáveis do ficheiro .env
from dotenv import load_dotenv

# Carrega as variáveis existentes no ficheiro .env
load_dotenv()


# Endereço usado para enviar dados ao IoT Agent
IOT_AGENT_URL = "http://localhost:7896/iot/json"

# Endereço base da API ASP.NET
API_BASE_URL = "http://localhost:8080/api"

# Chave usada para identificar/autenticar os dispositivos no IoT Agent
API_KEY = "drivolution-key"


# Cabeçalhos enviados nos pedidos para o IoT Agent
HEADERS_IOT = {
    # Indica que o corpo do pedido está em JSON
    "Content-Type": "application/json",

    # Identifica o serviço FIWARE
    "FIWARE-Service": "drivolution",

    # Define o caminho do serviço FIWARE
    "FIWARE-ServicePath": "/",
}


# Guarda o token JWT depois do primeiro login,
# para evitar repetir o login em cada pedido
_cached_token = None


def login():
    """Mesmo padrão de drivolution_quality_agent.py:
    DRIVOLUTION_EMAIL / DRIVOLUTION_PASSWORD, ou DRIVOLUTION_TOKEN direto."""

    # Indica que vamos alterar a variável global
    global _cached_token

    # Se já existe token guardado, devolve-o diretamente
    if _cached_token:
        return _cached_token

    # Primeiro tenta obter um token diretamente do ficheiro .env
    manual_token = os.getenv("DRIVOLUTION_TOKEN")

    if manual_token:
        # Remove a palavra "Bearer", caso tenha sido incluída
        _cached_token = (
            manual_token
            .replace("Bearer ", "")
            .strip()
        )

        return _cached_token

    # Se não existir token manual, procura email e password
    email = os.getenv("DRIVOLUTION_EMAIL")
    password = os.getenv("DRIVOLUTION_PASSWORD")

    # Se as credenciais não estiverem definidas,
    # os pedidos serão feitos sem autenticação
    if not email or not password:
        print("   ⚠ Sem autenticação definida.")
        print(
            "   Define DRIVOLUTION_EMAIL e "
            "DRIVOLUTION_PASSWORD ou DRIVOLUTION_TOKEN."
        )
        return None

    # Envia o pedido de login para a API
    r = requests.post(
        f"{API_BASE_URL}/Auth/login",

        # Dados enviados no corpo do pedido
        json={
            "email": email,
            "password": password
        },

        # Cancela o pedido se demorar mais de 10 segundos
        timeout=10,
    )

    # Verifica se o login foi aceite
    if r.status_code not in (200, 201):
        print(
            f"   ✗ Login falhou "
            f"({r.status_code}): {r.text}"
        )
        return None

    # Tenta retirar o token da resposta JSON
    token = r.json().get("token")

    if not token:
        print(
            f"   ✗ Login respondeu mas sem token: "
            f"{r.json()}"
        )
        return None

    # Guarda o token para pedidos seguintes
    _cached_token = token

    print("   ✓ Login efetuado com sucesso.")

    return _cached_token


# Cria os cabeçalhos usados nos pedidos à API ASP.NET
def api_headers():
    # Cabeçalho básico para pedidos JSON
    h = {
        "Content-Type": "application/json"
    }

    # Obtém o token, fazendo login se necessário
    token = login()

    # Se existir token, adiciona-o ao pedido
    if token:
        h["Authorization"] = f"Bearer {token}"

    return h


def load_skids() -> list:
    """Carrega suportes da API."""

    try:
        # /Support é paginado — /Support/all devolve lista, e precisa de token.
        r = requests.get(
            f"{API_BASE_URL}/Support/all",
            headers=api_headers(),
            timeout=5
        )

        # Se o pedido correu bem
        if r.status_code == 200:
            # Converte a resposta JSON
            data = r.json()

            # Algumas respostas do .NET podem trazer a lista
            # dentro da propriedade "$values"
            raw = (
                data.get("$values", data)
                if isinstance(data, dict)
                else data
            )

            # Garante que o resultado devolvido é uma lista
            return raw if isinstance(raw, list) else []

        print(
            f"   ✗ Erro ao carregar suportes: "
            f"{r.status_code}"
        )

        return []

    except Exception as e:
        # Evita que uma falha na API termine o programa
        print(f"   ✗ Erro: {e}")
        return []


def load_workstations() -> list:
    """Carrega workstations da API."""

    try:
        # Obtém todas as workstations através da API
        r = requests.get(
            f"{API_BASE_URL}/Workstation",
            headers=api_headers(),
            timeout=5
        )

        if r.status_code == 200:
            data = r.json()

            # Trata respostas normais ou respostas com "$values"
            raw = (
                data.get("$values", data)
                if isinstance(data, dict)
                else data
            )

            return raw if isinstance(raw, list) else []

        print(
            f"   ✗ Erro ao carregar workstations: "
            f"{r.status_code}"
        )

        return []

    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return []


# Obtém o produto que está atualmente associado a um suporte
def get_current_product(support_id: int) -> int | None:
    try:
        # Consulta a associação ativa entre suporte e produto
        r = requests.get(
            f"{API_BASE_URL}/SupportedProduct/"
            f"support/{support_id}/current",
            headers=api_headers(),
            timeout=5
        )

        # Se existir associação, devolve o ProductId
        # Caso contrário, devolve None
        return (
            r.json().get("productId")
            if r.status_code == 200
            else None
        )

    except (requests.RequestException, ValueError):
        # RequestException trata erros de rede.
        # ValueError trata respostas que não sejam JSON válido.
        return None


# Envia um evento RFID para o IoT Agent
def send_rfid_event(
    skid: dict,
    workstation: dict
) -> bool:
    # Verifica que produto está atualmente no skid
    product_id = get_current_product(skid["id"])

    # Dados que simulam a leitura RFID
    payload = {
        # Workstation onde o skid foi detetado
        "currentWorkstation": workstation["id"],

        # Produto transportado pelo skid, se existir
        "productId": product_id,

        # ID interno do suporte
        "supportId": skid["id"],

        # Tag RFID física do suporte
        "rfidTag": skid["rfidTag"],
    }

    # Constrói o endereço do IoT Agent.
    # "i" identifica o dispositivo.
    # "k" contém a API key.
    url = (
        f"{IOT_AGENT_URL}"
        f"?i=skid-{skid['rfidTag']}"
        f"&k={API_KEY}"
    )

    # Obtém informação legível para mostrar no terminal
    line_name = workstation.get(
        "productionLineName",
        f"Linha #{workstation['productionLineId']}"
    )

    phase_name = workstation.get(
        "phaseName",
        "?"
    )

    ws_type = workstation.get(
        "type",
        "?"
    )

    # Mostra o evento que será simulado
    print("\n📡 A simular leitura RFID:")

    print(
        f"   Skid:        {skid['rfidTag']} "
        f"({skid.get('type', '?')})"
    )

    print(
        f"   Workstation: {workstation['id']} — "
        f"{line_name} / WS {ws_type} / {phase_name}"
    )

    print(
        f"   Produto:     "
        f"{product_id if product_id else 'nenhum'}"
    )

    try:
        # Envia o evento para o IoT Agent
        r = requests.post(
            url,
            headers=HEADERS_IOT,
            json=payload,
            timeout=10
        )

        # Códigos considerados sucesso
        if r.status_code in (200, 201, 204):
            print(
                f"   ✓ IoT Agent aceitou "
                f"({r.status_code})."
            )
            return True

        # O IoT Agent respondeu, mas rejeitou o pedido
        else:
            print(
                f"   ✗ Rejeitado "
                f"({r.status_code}): {r.text}"
            )
            return False

    except Exception as e:
        # Trata falhas de rede ou outros erros
        print(f"   ✗ Erro: {e}")
        return False


# Verifica no Orion se a entidade do skid foi atualizada
def verify_orion(rfid_tag: str):
    # Cria o identificador NGSI-LD da entidade
    entity_id = (
        f"urn:ngsi-ld:Skid:skid-{rfid_tag}"
    )

    try:
        # Consulta diretamente a entidade no Orion
        r = requests.get(
            f"http://localhost:1026/"
            f"ngsi-ld/v1/entities/{entity_id}",

            # O Orion precisa dos mesmos cabeçalhos FIWARE
            headers={
                "FIWARE-Service": "drivolution",
                "FIWARE-ServicePath": "/"
            },

            timeout=5
        )

        if r.status_code == 200:
            # Obtém o valor atual da workstation
            ws = (
                r.json()
                .get("currentWorkstation", {})
                .get("value", "?")
            )

            print(
                f"   🔍 Orion — "
                f"currentWorkstation: {ws}"
            )

        else:
            print(
                "   ⚠ Entidade não encontrada "
                f"no Orion ({r.status_code})"
            )

    except Exception as e:
        print(f"   ⚠ {e}")


# Modo em que o utilizador escolhe manualmente
# o skid e a workstation
def interactive_mode(
    skids,
    workstations
):
    print("\n" + "=" * 55)
    print(
        "  DRIVOLUTION - Agente RFID "
        "(Modo Interativo)"
    )
    print("=" * 55)

    # Repete até o utilizador escolher sair
    while True:
        # Recarregar skids a cada ciclo para mostrar estado atual
        skids = load_skids()

        print(
            "\n── Skids "
            "──────────────────────────────────────────────"
        )

        if not skids:
            print(
                "  ✗ Nenhum suporte encontrado na API."
            )
            break

        # Mostra todos os skids disponíveis
        for i, s in enumerate(skids):
            # Verifica se existe produto no skid
            product_id = get_current_product(s["id"])

            prod_str = (
                f"Produto #{product_id}"
                if product_id
                else "Livre"
            )

            tag = s.get("rfidTag") or "sem tag"
            tipo = s.get("type") or "?"

            print(
                f"  [{i + 1}] "
                f"ID:{s['id']:3} | "
                f"{tag:15} | "
                f"{tipo:15} | "
                f"{prod_str}"
            )

        try:
            # Pede ao utilizador que escolha um skid
            escolha = int(
                input(
                    "\nEscolhe um skid "
                    "(0 para sair): "
                )
            )

            if escolha == 0:
                break

            # A lista começa no índice 0,
            # mas as opções apresentadas começam em 1
            skid = skids[escolha - 1]

        except (ValueError, IndexError):
            print("Opção inválida.")
            continue

        # Um suporte sem RFID não pode ser simulado
        if not skid.get("rfidTag"):
            print(
                "   ✗ Este suporte não tem "
                "tag RFID definida."
            )
            continue

        print(
            "\n── Workstations "
            "───────────────────────────────────────"
        )

        # Mostra todas as workstations
        for ws in workstations:
            line_name = ws.get(
                "productionLineName",
                f"Linha #{ws['productionLineId']}"
            )

            phase_name = ws.get(
                "phaseName",
                "?"
            )

            print(
                f"  [{ws['id']}] "
                f"{line_name:10} | "
                f"WS {ws.get('type', '?'):3} | "
                f"{phase_name}"
            )

        try:
            # Pede o ID da workstation de destino
            ws_id = int(
                input(
                    "\nWorkstation de destino: "
                )
            )

            # Procura a workstation pelo ID
            workstation = next(
                (
                    w for w in workstations
                    if w["id"] == ws_id
                ),
                None
            )

            if not workstation:
                print("Workstation inválida.")
                continue

        except ValueError:
            print("Opção inválida.")
            continue

        # Envia o evento RFID
        sucesso = send_rfid_event(
            skid,
            workstation
        )

        if sucesso:
            # Dá algum tempo para o FIWARE processar o evento
            print("\n⏳ A aguardar 3s...")
            time.sleep(3)

            # Confirma no Orion se a entidade foi atualizada
            verify_orion(skid["rfidTag"])

            print("✅ Verifica o WIP Dashboard.")


def auto_mode(skids, workstations):
    """Simula o primeiro skid a percorrer as workstations da sua linha."""

    print("\n" + "=" * 55)
    print(
        "  DRIVOLUTION - Agente RFID "
        "(Modo Automático)"
    )
    print("=" * 55)

    # Verifica se existem suportes
    if not skids:
        print("✗ Nenhum suporte disponível.")
        return

    # Escolhe o primeiro skid da lista
    skid = skids[0]

    if not skid.get("rfidTag"):
        print("✗ Primeiro suporte sem tag RFID.")
        return

    # Procura apenas workstations da linha do skid
    linha_ws = [
        ws for ws in workstations
        if ws["productionLineId"]
        == skid.get("productionLineId", 1)
    ]

    # Se não encontrar workstations da mesma linha,
    # usa as três primeiras como alternativa
    if not linha_ws:
        linha_ws = workstations[:3]

    print(
        f"\n🚗 Skid {skid['rfidTag']} "
        f"a percorrer {len(linha_ws)} workstations..."
    )

    # Percorre as workstations em sequência
    for ws in linha_ws:
        send_rfid_event(skid, ws)

        print("   ⏳ 5s...")

        # Aguarda antes do próximo evento
        time.sleep(5)

        # Confirma a atualização no Orion
        verify_orion(skid["rfidTag"])

    print(
        "\n✅ Simulação automática concluída."
    )


# Modo direto:
# recebe uma tag RFID e um ID de workstation pelo terminal
def direct_mode(
    skids,
    workstations,
    tag: str,
    ws_id: int
):
    # Procura o skid através da tag RFID
    skid = next(
        (
            s for s in skids
            if s.get("rfidTag") == tag
        ),
        None
    )

    # Procura a workstation através do ID
    ws = next(
        (
            w for w in workstations
            if w["id"] == ws_id
        ),
        None
    )

    if not skid:
        print(
            f"✗ Tag '{tag}' não encontrada."
        )
        return

    if not ws:
        print(
            f"✗ Workstation {ws_id} "
            "não encontrada."
        )
        return

    # Envia diretamente o evento
    send_rfid_event(skid, ws)

    # Aguarda o processamento no Orion
    time.sleep(3)

    # Confirma o resultado
    verify_orion(tag)


# Este bloco só é executado quando o ficheiro
# é iniciado diretamente
if __name__ == "__main__":
    # Cria o leitor de argumentos do terminal
    parser = argparse.ArgumentParser(
        description="DRIVOLUTION RFID Agent"
    )

    # Ativa o modo automático
    parser.add_argument(
        "--auto",
        action="store_true"
    )

    # Permite indicar diretamente uma tag RFID
    parser.add_argument(
        "--tag",
        type=str
    )

    # Permite indicar diretamente uma workstation
    parser.add_argument(
        "--ws",
        type=int
    )

    # Lê os argumentos enviados
    args = parser.parse_args()

    print("🔄 A carregar dados da API...")

    # Obtém os suportes e workstations existentes
    skids = load_skids()
    workstations = load_workstations()

    print(
        f"   ✓ {len(skids)} suporte(s) | "
        f"{len(workstations)} workstation(s)"
    )

    # Se foi usado --auto, executa o modo automático
    if args.auto:
        auto_mode(
            skids,
            workstations
        )

    # Se foram indicados uma tag e uma workstation,
    # executa o modo direto
    elif args.tag and args.ws:
        direct_mode(
            skids,
            workstations,
            args.tag,
            args.ws
        )

    # Caso contrário, abre o modo interativo
    else:
        interactive_mode(
            skids,
            workstations
        )