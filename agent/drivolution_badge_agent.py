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

# Carrega as variáveis existentes no ficheiro .env
load_dotenv()

# Endereço do IoT Agent que recebe os eventos dos crachás
IOT_AGENT_URL = "http://localhost:7896/iot/json"

# Endereço base da API ASP.NET
API_BASE_URL = "http://localhost:8080/api"

# Chave usada pelos dispositivos do tipo Badge no IoT Agent
BADGE_APIKEY = "drivolution-badge-key"

# Cabeçalhos necessários para enviar dados ao IoT Agent/FIWARE
HEADERS_IOT = {
    # Indica que o conteúdo enviado está em JSON
    "Content-Type": "application/json",

    # Identifica o serviço FIWARE
    "FIWARE-Service": "drivolution",

    # Define o caminho dentro do serviço FIWARE
    "FIWARE-ServicePath": "/",
}

# Guarda o token JWT depois do primeiro login,
# evitando repetir o login em todos os pedidos
_cached_token = None


def login():
    """Mesmo padrão de drivolution_agent.py:
    DRIVOLUTION_EMAIL / DRIVOLUTION_PASSWORD, ou DRIVOLUTION_TOKEN direto."""

    # Indica que vamos usar e alterar a variável global
    global _cached_token

    # Se já existe um token guardado, reutiliza-o
    if _cached_token:
        return _cached_token

    # Tenta obter diretamente um token definido no ficheiro .env
    manual_token = os.getenv("DRIVOLUTION_TOKEN")

    if manual_token:
        # Remove "Bearer " caso tenha sido incluído no valor
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

    # Envia as credenciais para o endpoint de login
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

    # Obtém o token devolvido pela API
    token = r.json().get("token")

    # Se a resposta não trouxer token, não é possível continuar autenticado
    if not token:
        print(f"   ✗ Login respondeu mas sem token: {r.json()}")
        return None

    # Guarda o token para os pedidos seguintes
    _cached_token = token

    print("   ✓ Login efetuado com sucesso.")

    return _cached_token


# Cria os cabeçalhos usados nos pedidos à API ASP.NET
def api_headers():
    # Cabeçalho básico para pedidos em JSON
    h = {
        "Content-Type": "application/json"
    }

    # Obtém o token, fazendo login se necessário
    token = login()

    # Se existir token, adiciona-o ao cabeçalho Authorization
    if token:
        h["Authorization"] = f"Bearer {token}"

    return h


def load_users(role: str | None = None) -> list:
    """Carrega utilizadores (operator/manager) da API. Requer conta admin no .env."""

    try:
        # Lista onde serão acumulados os utilizadores encontrados
        users = []

        # Se foi indicada uma role, pesquisa apenas essa.
        # Caso contrário, pesquisa operadores e managers.
        roles = [role] if role else ["operator", "manager"]

        # Faz um pedido separado para cada role
        for r_ in roles:
            r = requests.get(
                f"{API_BASE_URL}/User",
                headers=api_headers(),
                params={
                    "role": r_,
                    "pageSize": 100
                },
                timeout=5,
            )

            if r.status_code == 200:
                # Converte a resposta para JSON
                data = r.json()

                # Tenta obter a lista em vários formatos possíveis
                items = (
                    data.get("data")
                    or data.get("Data")
                    or data.get("$values", data)
                )

                # Só adiciona o resultado se for realmente uma lista
                users.extend(
                    items if isinstance(items, list) else []
                )
            else:
                print(
                    f"   ✗ Erro ao carregar role "
                    f"'{r_}': {r.status_code}"
                )

        return users

    except Exception as e:
        # Se ocorrer um erro de rede ou outro problema,
        # devolve uma lista vazia
        print(f"   ✗ Erro: {e}")
        return []


def load_workstations() -> list:
    """Carrega workstations da API e filtra só as elegíveis para presença humana."""

    try:
        # Obtém todas as workstations
        r = requests.get(
            f"{API_BASE_URL}/Workstation",
            headers=api_headers(),
            timeout=5
        )

        if r.status_code == 200:
            data = r.json()

            # Algumas respostas do .NET podem trazer a lista
            # dentro da propriedade "$values"
            raw = (
                data.get("$values", data)
                if isinstance(data, dict)
                else data
            )

            raw = raw if isinstance(raw, list) else []

            # Só permite presença humana em workstations
            # classificadas como human ou hybrid
            return [
                w for w in raw
                if w.get("kind") in ("human", "hybrid")
            ]

        print(
            f"   ✗ Erro ao carregar workstations: "
            f"{r.status_code}"
        )

        return []

    except Exception as e:
        print(f"   ✗ Erro: {e}")
        return []


# Envia uma leitura simulada de crachá para o IoT Agent
def send_badge_event(
    user: dict,
    workstation: dict
) -> bool:
    # O evento indica apenas a workstation onde o crachá foi lido
    payload = {
        "currentWorkstation": workstation["id"]
    }

    # Cria o URL do dispositivo:
    # i identifica o crachá e k contém a API key
    url = (
        f"{IOT_AGENT_URL}"
        f"?i=badge-{user['id']}"
        f"&k={BADGE_APIKEY}"
    )

    # Obtém nomes legíveis para mostrar no terminal
    line_name = workstation.get(
        "productionLineName",
        f"Linha #{workstation.get('productionLineId', '?')}"
    )

    phase_name = workstation.get(
        "phaseName",
        "?"
    )

    print("\n🪪 A simular leitura de crachá:")

    print(
        f"   Operador:    "
        f"{user.get('name', '?')} "
        f"(id {user['id']})"
    )

    print(
        f"   Workstation: "
        f"{workstation['id']} — "
        f"{line_name} / {phase_name}"
    )

    try:
        # Envia o evento ao IoT Agent
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

        # O serviço respondeu, mas rejeitou o evento
        print(
            f"   ✗ Rejeitado "
            f"({r.status_code}): {r.text}"
        )

        return False

    except Exception as e:
        # Trata falhas de rede ou outros erros
        print(f"   ✗ Erro: {e}")
        return False


# Confirma no Orion a workstation atual do crachá
def verify_orion(user_id: int):
    # Constrói o identificador NGSI-LD da entidade Badge
    entity_id = (
        f"urn:ngsi-ld:Badge:badge-{user_id}"
    )

    try:
        # Consulta diretamente a entidade no Orion
        r = requests.get(
            f"http://localhost:1026/"
            f"ngsi-ld/v1/entities/{entity_id}",
            headers={
                "FIWARE-Service": "drivolution",
                "FIWARE-ServicePath": "/"
            },
            timeout=5,
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
# o operador e a workstation
def interactive_mode(users, workstations):
    print("\n" + "=" * 55)
    print(
        "  DRIVOLUTION - Agente de Crachá "
        "(Modo Interativo)"
    )
    print("=" * 55)

    # Mantém o menu ativo até o utilizador escolher sair
    while True:
        print(
            "\n── Operadores "
            "─────────────────────────────────────────"
        )

        # Se não existirem operadores ou managers, termina
        if not users:
            print(
                "  ✗ Nenhum utilizador "
                "operator/manager encontrado."
            )
            break

        # Mostra todos os utilizadores disponíveis
        for i, u in enumerate(users):
            print(
                f"  [{i + 1}] "
                f"ID:{u['id']:3} | "
                f"{u.get('name', '?')}"
            )

        try:
            # Pede ao utilizador que escolha um operador
            escolha = int(
                input(
                    "\nEscolhe um operador "
                    "(0 para sair): "
                )
            )

            if escolha == 0:
                break

            # A lista começa no índice 0,
            # mas o menu começa no número 1
            user = users[escolha - 1]

        except (ValueError, IndexError):
            print("Opção inválida.")
            continue

        print(
            "\n── Workstations (human/hybrid) "
            "─────────────────────────"
        )

        # Apenas workstations human ou hybrid são permitidas
        if not workstations:
            print(
                "  ✗ Nenhuma workstation elegível "
                "(human/hybrid) encontrada."
            )
            continue

        # Mostra as workstations disponíveis
        for ws in workstations:
            line_name = ws.get(
                "productionLineName",
                f"Linha #{ws.get('productionLineId', '?')}"
            )

            phase_name = ws.get(
                "phaseName",
                "?"
            )

            print(
                f"  [{ws['id']}] "
                f"{line_name:10} | "
                f"{phase_name} | "
                f"kind={ws.get('kind')}"
            )

        try:
            # Pede o ID da workstation onde o crachá foi lido
            ws_id = int(
                input(
                    "\nWorkstation do leitor: "
                )
            )

            # Procura a workstation escolhida
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

        # Simula a leitura do crachá
        sucesso = send_badge_event(
            user,
            workstation
        )

        if sucesso:
            # Dá tempo ao FIWARE para processar o evento
            print("\n⏳ A aguardar 3s...")
            time.sleep(3)

            # Confirma no Orion
            verify_orion(user["id"])

            print(
                "✅ Verifica o ecrã de "
                "Presença no dashboard."
            )


def auto_mode(users, workstations):
    """Simula o primeiro operador a entrar e sair da primeira workstation elegível."""

    print("\n" + "=" * 55)
    print(
        "  DRIVOLUTION - Agente de Crachá "
        "(Modo Automático)"
    )
    print("=" * 55)

    # Verifica se existe pelo menos um utilizador
    if not users:
        print("✗ Nenhum operador disponível.")
        return

    # Verifica se existe uma workstation elegível
    if not workstations:
        print(
            "✗ Nenhuma workstation "
            "human/hybrid disponível."
        )
        return

    # Escolhe o primeiro utilizador e a primeira workstation
    user = users[0]
    ws = workstations[0]

    # Primeira leitura: check-in
    print(
        f"\n🪪 Operador {user.get('name', '?')} "
        f"vai fazer check-in em WS {ws['id']}..."
    )

    send_badge_event(user, ws)

    time.sleep(3)

    verify_orion(user["id"])

    # Simula algum tempo de permanência na workstation
    print("\n⏳ 5s de presença simulada...")
    time.sleep(5)

    # Segunda leitura na mesma workstation: check-out
    print(
        f"\n🪪 Operador {user.get('name', '?')} "
        "vai fazer check-out (mesmo leitor)..."
    )

    send_badge_event(
        user,
        ws
    )  # mesma workstation = toggle → check-out

    time.sleep(3)

    verify_orion(user["id"])

    print("\n✅ Simulação automática concluída.")


# Modo direto: recebe o ID do utilizador
# e o ID da workstation através do terminal
def direct_mode(
    users,
    workstations,
    user_id: int,
    ws_id: int
):
    # Procura o utilizador pelo ID
    user = next(
        (
            u for u in users
            if u["id"] == user_id
        ),
        None
    )

    # Procura a workstation pelo ID
    ws = next(
        (
            w for w in workstations
            if w["id"] == ws_id
        ),
        None
    )

    # Confirma que o utilizador existe e tem role permitida
    if not user:
        print(
            f"✗ Utilizador {user_id} não encontrado "
            "(ou não é operator/manager)."
        )
        return

    # Confirma que a workstation é human ou hybrid
    if not ws:
        print(
            f"✗ Workstation {ws_id} não encontrada "
            "(ou não é human/hybrid)."
        )
        return

    # Envia o evento
    send_badge_event(user, ws)

    # Aguarda o processamento
    time.sleep(3)

    # Confirma a alteração no Orion
    verify_orion(user_id)


# Só é executado quando este ficheiro
# é iniciado diretamente
if __name__ == "__main__":
    # Cria o leitor de argumentos do terminal
    parser = argparse.ArgumentParser(
        description="DRIVOLUTION Badge Agent"
    )

    # Ativa o modo automático
    parser.add_argument(
        "--auto",
        action="store_true"
    )

    # Permite indicar diretamente o ID do utilizador
    parser.add_argument(
        "--user",
        type=int,
        help="ID do utilizador (AppUser)"
    )

    # Permite indicar diretamente o ID da workstation
    parser.add_argument(
        "--ws",
        type=int,
        help="ID da workstation"
    )

    # Lê os argumentos recebidos
    args = parser.parse_args()

    print("🔄 A carregar dados da API...")

    # Obtém os operadores/managers
    users = load_users()

    # Obtém apenas workstations human ou hybrid
    workstations = load_workstations()

    print(
        f"   ✓ {len(users)} operador(es)/manager(s) | "
        f"{len(workstations)} workstation(s) elegível(eis)"
    )

    # Modo automático
    if args.auto:
        auto_mode(
            users,
            workstations
        )

    # Modo direto, quando foram fornecidos os dois IDs
    elif args.user and args.ws:
        direct_mode(
            users,
            workstations,
            args.user,
            args.ws
        )

    # Sem argumentos, abre o modo interativo
    else:
        interactive_mode(
            users,
            workstations
        )