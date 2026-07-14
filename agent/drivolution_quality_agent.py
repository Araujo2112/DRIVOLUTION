import argparse
import os
import random
import time
import requests
from dotenv import load_dotenv

# Carrega as variáveis do ficheiro .env
load_dotenv()


# Endereço base da API.
# Se a variável DRIVOLUTION_API_URL não existir,
# usa o endereço local por defeito.
API_BASE_URL = os.getenv(
    "DRIVOLUTION_API_URL",
    "http://localhost:8080/api"
)


# Severidades que podem ser simuladas pelo agente
SEVERITIES = [
    "none",
    "minor",
    "major",
    "critical"
]


# Guarda o token JWT depois do primeiro login,
# evitando repetir o login em todos os pedidos
_cached_token = None


# Garante que os dados recebidos são devolvidos como lista
def normalize_list(data):
    # Alguns resultados JSON do .NET podem vir
    # dentro de uma propriedade chamada "$values"
    if isinstance(data, dict) and "$values" in data:
        return data["$values"]

    # Se já for uma lista, devolve diretamente
    if isinstance(data, list):
        return data

    # Se não tiver nenhum dos formatos esperados,
    # devolve uma lista vazia
    return []


# Tenta encontrar o token JWT na resposta do login
def extract_token(data):
    # Se a resposta não for um dicionário,
    # não é possível procurar o token
    if not isinstance(data, dict):
        return None

    # Possíveis nomes que a propriedade do token pode ter
    possible_keys = [
        "token",
        "accessToken",
        "jwt",
        "jwtToken",
        "bearerToken",
    ]

    # Procura primeiro pelas propriedades mais comuns
    for key in possible_keys:
        value = data.get(key)

        if isinstance(value, str) and value:
            return value

    # Se não encontrou pelas chaves conhecidas,
    # procura uma string com o formato típico de JWT:
    # três partes separadas por dois pontos finais
    for value in data.values():
        if isinstance(value, str) and value.count(".") == 2:
            return value

    return None


# Faz login na API e devolve o token JWT
def login():
    # Indica que será usada a variável global
    global _cached_token

    # Se já existe um token guardado,
    # não volta a fazer login
    if _cached_token:
        return _cached_token

    # Primeiro verifica se foi fornecido
    # diretamente um token no ficheiro .env
    manual_token = os.getenv("DRIVOLUTION_TOKEN")

    if manual_token:
        # Remove "Bearer " caso tenha sido incluído
        _cached_token = (
            manual_token
            .replace("Bearer ", "")
            .strip()
        )

        return _cached_token

    # Se não existir token manual,
    # procura email e password
    email = os.getenv("DRIVOLUTION_EMAIL")
    password = os.getenv("DRIVOLUTION_PASSWORD")

    # Se não houver dados de autenticação,
    # os pedidos serão feitos sem token
    if not email or not password:
        print("   ⚠ Sem autenticação.")
        print(
            "   Define DRIVOLUTION_EMAIL e "
            "DRIVOLUTION_PASSWORD ou DRIVOLUTION_TOKEN."
        )
        return None

    # Dados enviados para o endpoint de login
    payload = {
        "email": email,
        "password": password
    }

    # Faz o pedido de login à API
    r = requests.post(
        f"{API_BASE_URL}/Auth/login",
        json=payload,
        timeout=10
    )

    # Verifica se o login foi aceite
    if r.status_code not in (200, 201):
        print(
            f"   ✗ Login falhou "
            f"({r.status_code}): {r.text}"
        )
        return None

    # Converte a resposta JSON para dicionário
    data = r.json()

    # Tenta retirar o token da resposta
    token = extract_token(data)

    if not token:
        print(
            "   ✗ Login respondeu, "
            "mas não encontrei token."
        )
        print(f"   Resposta: {data}")
        return None

    # Guarda o token para os pedidos seguintes
    _cached_token = token

    print("   ✓ Login efetuado com sucesso.")

    return _cached_token


# Cria os cabeçalhos usados nos pedidos à API
def headers():
    # Obtém o token, fazendo login se necessário
    token = login()

    # Todos os pedidos enviam e recebem JSON
    h = {
        "Content-Type": "application/json"
    }

    # Se existir token, adiciona-o ao cabeçalho Authorization
    if token:
        h["Authorization"] = f"Bearer {token}"

    return h


# Obtém os produtos que estão atualmente em produção
def get_wip_items():
    r = requests.get(
        f"{API_BASE_URL}/production-lines/wip",
        headers=headers(),
        timeout=10
    )

    # Dá uma mensagem específica se o token for inválido
    if r.status_code == 401:
        raise Exception(
            "401 Unauthorized em /production-lines/wip. "
            "Verifica email/password/token."
        )

    # Lança uma exceção para outros erros HTTP
    r.raise_for_status()

    # Converte a resposta para dicionário
    data = r.json()

    # Retira apenas a lista de produtos em produção
    return normalize_list(
        data.get("items", [])
    )


# Obtém a timeline completa de um produto
def get_product_timeline(product_id: int):
    r = requests.get(
        f"{API_BASE_URL}/products/"
        f"{product_id}/timeline",
        headers=headers(),
        timeout=10
    )

    if r.status_code == 401:
        raise Exception(
            "401 Unauthorized em "
            "/products/{id}/timeline. "
            "Verifica email/password/token."
        )

    r.raise_for_status()

    return r.json()


# Obtém todos os Quality Checks de um produto
def get_existing_quality_checks(product_id: int):
    r = requests.get(
        f"{API_BASE_URL}/QualityCheck/"
        f"product/{product_id}",
        headers=headers(),
        timeout=10
    )

    # Se não existir nenhum, devolve lista vazia
    if r.status_code == 404:
        return []

    if r.status_code == 401:
        raise Exception(
            "401 Unauthorized em "
            "/QualityCheck/product/{id}. "
            "Verifica email/password/token."
        )

    r.raise_for_status()

    return normalize_list(r.json())


# Procura a fase atualmente aberta na timeline
def get_current_phase_from_timeline(product_id: int):
    # Obtém a timeline completa
    timeline = get_product_timeline(product_id)

    # Obtém a lista de fases
    phases = normalize_list(
        timeline.get("phases", [])
    )

    # Uma fase está ativa quando ainda não tem data de fim
    active = [
        phase
        for phase in phases
        if not phase.get("endedAt")
    ]

    # Se não existir nenhuma fase ativa,
    # devolve None
    if not active:
        return None

    # Devolve a última fase ativa encontrada
    return active[-1]


# Cria um novo Quality Check através da API
def create_quality_check(
    product_id: int,
    manufacturing_phase_id: int,
    severity: str,
    notes: str
):
    # Dados enviados ao backend
    payload = {
        "productId": product_id,
        "manufacturingPhaseId": manufacturing_phase_id,
        "notes": notes,

        # O estado não é escolhido pelo agente.
        # O QualityCheckService decide automaticamente
        # entre passed, rework e scrapped.
        "status": None,

        "severity": severity
    }

    # Envia o controlo de qualidade para a API
    r = requests.post(
        f"{API_BASE_URL}/QualityCheck",
        headers=headers(),
        json=payload,
        timeout=10
    )

    # Se foi criado com sucesso,
    # devolve os dados recebidos
    if r.status_code in (200, 201):
        return r.json()

    print(
        f"   ✗ Erro ao criar QualityCheck "
        f"({r.status_code}): {r.text}"
    )

    return None


# Verifica se já existe um Quality Check
# para este produto e esta fase de fabrico
def already_checked(
    product_id: int,
    manufacturing_phase_id: int
):
    # Obtém todos os controlos do produto
    checks = get_existing_quality_checks(product_id)

    # Percorre os controlos existentes
    for check in checks:
        if (
            check.get("manufacturingPhaseId")
            == manufacturing_phase_id
        ):
            return True

    return False


# Simula aleatoriamente a severidade observada
def simulate_quality_result():
    return random.choices(
        SEVERITIES,

        # Probabilidades associadas a cada severidade:
        # none     -> 70%
        # minor    -> 18%
        # major    -> 9%
        # critical -> 3%
        weights=[70, 18, 9, 3],

        # Escolhe apenas um resultado
        k=1
    )[0]


# Executa uma verificação completa
def run_once():
    print(
        "\n[Quality Agent] "
        "A procurar produtos em produção..."
    )

    # Obtém os produtos em produção
    items = get_wip_items()

    # Se não houver produtos, termina esta execução
    if not items:
        print("   - Nenhum produto em produção.")
        return

    # Percorre todos os produtos ativos
    for item in items:
        product_id = item.get("productId")
        serial = item.get("serialNumber")
        phase_name = (
            item.get("currentPhase") or ""
        )

        # Ignora registos sem ProductId
        if not product_id:
            continue

        # Obtém a fase atual através da timeline
        current_phase = (
            get_current_phase_from_timeline(
                product_id
            )
        )

        if not current_phase:
            print(
                f"   - Produto {serial}: "
                "sem fase ativa na timeline."
            )
            continue

        # Obtém o ID da fase de fabrico
        manufacturing_phase_id = (
            current_phase.get(
                "manufacturingPhaseId"
            )
        )

        if not manufacturing_phase_id:
            print(
                f"   - Produto {serial}: "
                "manufacturingPhaseId não encontrado."
            )
            continue

        # Evita criar vários Quality Checks
        # para a mesma fase do produto
        if already_checked(
            product_id,
            manufacturing_phase_id
        ):
            print(
                f"   - Produto {serial}: "
                "QualityCheck já existe para esta fase."
            )
            continue

        # Gera aleatoriamente uma severidade
        severity = simulate_quality_result()

        print(
            f"   → Produto {serial} | "
            f"Fase {phase_name} | "
            f"Severidade simulada: {severity}"
        )

        # Envia o controlo de qualidade para a API
        created = create_quality_check(
            product_id=product_id,
            manufacturing_phase_id=
                manufacturing_phase_id,
            severity=severity,
            notes=(
                "Quality Check automático gerado "
                "pelo agente de visão simulada. "
                f"Severidade observada: {severity}"
            )
        )

        # Mostra o resultado final decidido pelo backend
        if created:
            print(
                "   ✓ QualityCheck criado | "
                f"Status final: {created.get('status')} | "
                f"Severity: {created.get('severity')}"
            )


# Ponto principal do programa
def main():
    # Cria o sistema que lê argumentos do terminal
    parser = argparse.ArgumentParser(
        description=(
            "DRIVOLUTION - Agente de Visão "
            "Simulada para Quality Check"
        )
    )

    # --once executa apenas uma verificação
    parser.add_argument(
        "--once",
        action="store_true",
        help="Executa uma vez e termina."
    )

    # --interval define o tempo entre verificações
    parser.add_argument(
        "--interval",
        type=int,
        default=10,
        help=(
            "Intervalo entre verificações "
            "em segundos."
        )
    )

    # Lê os argumentos enviados
    args = parser.parse_args()

    print("=" * 60)
    print(" DRIVOLUTION - Quality Check Agent")
    print("=" * 60)
    print(f" API: {API_BASE_URL}")

    try:
        # Se foi usado --once,
        # executa apenas uma vez e termina
        if args.once:
            run_once()
            return

        # Caso contrário, executa continuamente
        while True:
            run_once()

            # Espera antes de voltar a verificar
            time.sleep(args.interval)

    except requests.HTTPError as e:
        # Trata erros HTTP devolvidos pela API
        print(f"   ✗ Erro HTTP: {e}")

    except Exception as e:
        # Trata qualquer outro erro inesperado
        print(f"   ✗ Erro: {e}")


# Só executa main() se este ficheiro
# for iniciado diretamente
if __name__ == "__main__":
    main()

