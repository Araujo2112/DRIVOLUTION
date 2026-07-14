"""
DRIVOLUTION - Orquestrador de Simulação Completa da Linha
"""

import argparse
import os
import random
import time
import requests
from dotenv import load_dotenv

# Carrega as variáveis existentes no ficheiro .env
load_dotenv()

# Importa funções auxiliares do agente de qualidade
from drivolution_quality_agent import (
    login,
    headers,
    normalize_list,
    get_existing_quality_checks,
    create_quality_check,
    simulate_quality_result,
    get_current_phase_from_timeline,
)

# Endereço da API principal do projeto
API_BASE_URL = os.getenv(
    "DRIVOLUTION_API_URL",
    "http://localhost:8080/api"
)

# Endereço usado para enviar leituras RFID ao IoT Agent
IOT_AGENT_URL = os.getenv(
    "DRIVOLUTION_IOT_URL",
    "http://localhost:7896/iot/json"
)

# Chave usada para identificar os dispositivos no IoT Agent
API_KEY = os.getenv(
    "DRIVOLUTION_IOT_KEY",
    "drivolution-key"
)

# Cabeçalhos enviados nos pedidos ao IoT Agent/FIWARE
HEADERS_IOT = {
    "Content-Type": "application/json",
    "FIWARE-Service": "drivolution",
    "FIWARE-ServicePath": "/",
}

# Cache local das sequências de fases de cada modelo.
# Evita consultar repetidamente a mesma informação durante um ciclo.
_model_phase_sequence_cache = {}

# Um QualityCheck não guarda a que ProductPhase pertence (só a manufacturing_phase_id),
# por isso, para saber se já inspecionámos ESTA tentativa em concreto (e não só "alguma
# vez, nesta fase"), guardamos localmente quais os productPhaseId já inspecionados.
_inspected_phase_instances = set()


# Converte uma duração em segundos para um texto legível.
# Exemplo: 125 segundos -> "2m05s"
def format_duration(seconds: float) -> str:
    # Impede valores negativos e converte para inteiro
    seconds = max(0, int(seconds))

    # Divide os segundos em minutos e segundos restantes
    m, s = divmod(seconds, 60)

    return f"{m}m{s:02d}s"


def completion_probability(
    elapsed: float,
    predicted: float,
    fixed_estimated: float,
    threshold_pct: float
) -> float:
    """Sistema 'anti-azar': a probabilidade de terminar a fase neste ciclo
    sobe sempre com o tempo, nunca desce, e nunca chega a 100% antes do
    limite de alerta de tempo excedido (150% da duração FIXA, a mesma
    referência usada pelo AlertBackgroundService) — para garantir que uma
    fatia realista de carros dispara mesmo esse alerta.

        0% ─────────────── 40% da duração prevista: nunca termina (carência)
       40% ── sobe até 85% ── 150% da duração FIXA (limite de alerta)
      150% ── sobe até 99% ── 200% da duração FIXA (quase-certeza, evita
                                                     ficar preso para sempre)
    """

    # Durante os primeiros 40% da duração prevista,
    # a fase nunca pode terminar
    grace = predicted * 0.40

    if elapsed < grace:
        return 0.0

    # Momento em que será disparado o alerta de tempo excedido
    threshold_seconds = fixed_estimated * (threshold_pct / 100.0)

    # Limite usado para aproximar a probabilidade de 100%
    ceiling_seconds = fixed_estimated * 2.0

    # Entre o fim da carência e o limite de alerta,
    # a probabilidade sobe gradualmente até 85%
    if elapsed <= threshold_seconds:
        span = max(threshold_seconds - grace, 1)
        progress = (elapsed - grace) / span
        return min(0.85, 0.85 * progress)

    # Quando já passou o dobro da duração fixa,
    # existe 99% de probabilidade de terminar
    if elapsed >= ceiling_seconds:
        return 0.99

    # Entre o limite de alerta e o dobro da duração,
    # a probabilidade sobe de 85% para 99%
    span = max(ceiling_seconds - threshold_seconds, 1)
    progress = (elapsed - threshold_seconds) / span

    return 0.85 + 0.14 * progress


# Obtém da API os produtos em produção e os produtos em espera
def get_wip():
    r = requests.get(
        f"{API_BASE_URL}/production-lines/wip",
        headers=headers(),
        timeout=10
    )

    # Lança uma exceção se a API devolver erro
    r.raise_for_status()

    # Converte a resposta JSON para um dicionário Python
    return r.json()


# Obtém os suportes/skids
def get_supports(occupied: bool | None = None):
    # Pede até 500 suportes
    params = {"pageSize": 500}

    # Se occupied tiver valor, aplica o filtro
    if occupied is not None:
        params["occupied"] = str(occupied).lower()

    r = requests.get(
        f"{API_BASE_URL}/Support",
        headers=headers(),
        params=params,
        timeout=10
    )

    r.raise_for_status()

    data = r.json()

    # Normaliza o resultado para garantir que é uma lista
    return normalize_list(data.get("data", []))


# Obtém todas as workstations de uma linha de produção
def get_workstations_by_line(production_line_id: int):
    r = requests.get(
        f"{API_BASE_URL}/Workstation/line/{production_line_id}",
        headers=headers(),
        timeout=10
    )

    r.raise_for_status()

    return normalize_list(r.json())


# Obtém a sequência de fases definida para um modelo de veículo
def get_phase_sequence(model_id: int):
    # Se já estiver no cache, devolve sem voltar a consultar a API
    if model_id in _model_phase_sequence_cache:
        return _model_phase_sequence_cache[model_id]

    r = requests.get(
        f"{API_BASE_URL}/PhaseSequence/model/{model_id}",
        headers=headers(),
        timeout=10
    )

    r.raise_for_status()

    # Ordena as fases pelo campo "order"
    sequence = sorted(
        normalize_list(r.json()),
        key=lambda p: p.get("order", 0)
    )

    # Guarda no cache
    _model_phase_sequence_cache[model_id] = sequence

    return sequence


# Obtém um produto pelo ID
def get_product(product_id: int):
    r = requests.get(
        f"{API_BASE_URL}/Product/{product_id}",
        headers=headers(),
        timeout=10
    )

    # Se o produto não existir, devolve None
    if r.status_code == 404:
        return None

    r.raise_for_status()

    return r.json()


# Verifica se um produto pode receber um novo skid
def is_eligible_for_new_skid(product_id: int) -> bool:
    r = requests.get(
        f"{API_BASE_URL}/products/{product_id}/timeline",
        headers=headers(),
        timeout=10
    )

    # Se ainda não existe timeline, o produto pode começar
    if r.status_code in (400, 404):
        return True

    # Outros erros impedem a atribuição
    if r.status_code != 200:
        return False

    data = r.json()

    # Só permite atribuir se o produto não estiver concluído
    # nem já estiver em produção
    return data.get("status") not in (
        "completed",
        "in_progress"
    )


# Associa um produto a um skid
def assign_product_to_skid(
    support_id: int,
    product_id: int
):
    payload = {
        "supportId": support_id,
        "productId": product_id
    }

    r = requests.post(
        f"{API_BASE_URL}/SupportedProduct",
        headers=headers(),
        json=payload,
        timeout=10
    )

    if r.status_code in (200, 201):
        return r.json()

    print(
        f"   ✗ Erro ao associar produto {product_id} "
        f"ao skid {support_id} ({r.status_code}): {r.text}"
    )

    return None


# Obtém a associação ativa de um skid a um produto
def get_current_supported_product(support_id: int):
    r = requests.get(
        f"{API_BASE_URL}/SupportedProduct/support/{support_id}/current",
        headers=headers(),
        timeout=10
    )

    if r.status_code == 404:
        return None

    r.raise_for_status()

    return r.json()


# Fecha a associação entre produto e skid,
# tornando o skid novamente disponível
def close_supported_product(
    supported_product_id: int
):
    r = requests.put(
        f"{API_BASE_URL}/SupportedProduct/"
        f"{supported_product_id}/close",
        headers=headers(),
        timeout=10
    )

    if r.status_code not in (200, 204):
        print(
            f"   ✗ Erro ao libertar skid "
            f"(SupportedProduct {supported_product_id}): "
            f"{r.status_code} {r.text}"
        )
        return False

    return True


# Fecha uma fase do produto
def close_product_phase(
    product_phase_id: int,
    result: str,
    quality_id: int | None
):
    payload = {
        "result": result,
        "qualityId": quality_id
    }

    r = requests.put(
        f"{API_BASE_URL}/ProductPhase/"
        f"{product_phase_id}/close",
        headers=headers(),
        json=payload,
        timeout=10
    )

    if r.status_code not in (200, 204):
        print(
            f"   ✗ Erro ao fechar ProductPhase "
            f"{product_phase_id}: {r.status_code} {r.text}"
        )
        return False

    return True


# Simula a leitura RFID de um skid numa workstation
def send_rfid_event(
    skid: dict,
    workstation: dict
) -> bool:
    # Dados enviados ao IoT Agent
    payload = {
        "currentWorkstation": workstation["id"],
        "productId": skid.get("currentProductId"),
        "supportId": skid["id"],
        "rfidTag": skid.get("rfidTag"),
    }

    # Identificador do dispositivo e chave de acesso
    url = (
        f"{IOT_AGENT_URL}"
        f"?i=skid-{skid.get('rfidTag')}"
        f"&k={API_KEY}"
    )

    print(
        f"   📡 Skid {skid.get('rfidTag')} "
        f"-> Workstation {workstation['id']} "
        f"({workstation.get('phaseName', '?')})"
    )

    try:
        # Envia o evento para o IoT Agent
        r = requests.post(
            url,
            headers=HEADERS_IOT,
            json=payload,
            timeout=10
        )

        if r.status_code in (200, 201, 204):
            return True

        print(
            f"   ✗ IoT Agent rejeitou "
            f"({r.status_code}): {r.text}"
        )

        return False

    except Exception as e:
        # Impede que uma falha no IoT Agent termine o orquestrador
        print(f"   ✗ Erro ao enviar evento RFID: {e}")
        return False


# Inicia o percurso de um produto na primeira fase
def start_journey(
    skid: dict,
    product_id: int
) -> bool:
    # Obtém o produto
    product = get_product(product_id)

    if not product:
        return False

    # Obtém a sequência de fases do modelo
    phase_seq = get_phase_sequence(product["modelId"])

    if not phase_seq:
        print(
            f"   ✗ Modelo {product['modelId']} "
            "sem phase_sequence definida."
        )
        return False

    # A primeira posição da sequência corresponde à primeira fase
    first_phase = phase_seq[0]

    # Obtém as workstations da linha do skid
    line_ws = get_workstations_by_line(
        skid["productionLineId"]
    )

    # Procura a workstation correspondente à primeira fase
    target_ws = next(
        (
            w for w in line_ws
            if w.get("manufacturingPhaseId")
            == first_phase.get("manufacturingPhaseId")
        ),
        None,
    )

    if not target_ws:
        print(
            f"   ✗ Linha {skid['productionLineId']} "
            "não tem workstation para a 1ª fase do modelo."
        )
        return False

    # Guarda localmente o produto atual do skid
    skid["currentProductId"] = product_id

    # Envia uma leitura RFID para iniciar a primeira fase
    return send_rfid_event(skid, target_ws)


# Trata produtos que ainda estão à espera
def assign_free_products(waiting_items: list):
    # Produtos que ainda não têm skid
    waiting_for_skid = [
        w for w in waiting_items
        if w.get("queueReason") == "waiting_support"
    ]

    # Produtos que já têm skid, mas ainda não entraram na linha
    waiting_for_line = [
        w for w in waiting_items
        if w.get("queueReason") == "waiting_line"
    ]

    # Trata os produtos sem skid
    if waiting_for_skid:
        # Obtém skids livres
        free_skids = get_supports(occupied=False)

        if not free_skids:
            print(
                "   - Há produtos à espera de skid, "
                "mas não há skids livres."
            )
        else:
            skid_index = 0

            for item in waiting_for_skid:
                # Termina se já não houver mais skids livres
                if skid_index >= len(free_skids):
                    print(
                        "   - Sem mais skids livres neste ciclo."
                    )
                    break

                product_id = item.get("productId")
                serial = item.get("serialNumber")

                # Evita associar um produto que já esteja ativo
                if not is_eligible_for_new_skid(product_id):
                    continue

                skid = free_skids[skid_index]

                # Cria a associação entre o produto e o skid
                assigned = assign_product_to_skid(
                    skid["id"],
                    product_id
                )

                if not assigned:
                    continue

                print(
                    f"   ✓ Produto {serial} associado ao skid "
                    f"{skid.get('rfidTag')} "
                    f"(linha {skid.get('productionLineId')})"
                )

                skid_index += 1

                # Envia o produto para a primeira workstation
                start_journey(skid, product_id)

    # Trata produtos que já têm skid,
    # mas ainda não começaram o percurso
    if waiting_for_line:
        occupied_skids = get_supports(occupied=True)

        for item in waiting_for_line:
            product_id = item.get("productId")
            serial = item.get("serialNumber")
            support_id = item.get("supportId")

            if not support_id:
                print(
                    f"   ✗ Produto {serial} em 'waiting_line' "
                    "sem supportId — dado inconsistente."
                )
                continue

            # Procura o skid indicado
            skid = next(
                (
                    s for s in occupied_skids
                    if s.get("id") == support_id
                ),
                None
            )

            if not skid:
                print(
                    f"   ✗ Produto {serial}: skid {support_id} "
                    "não encontrado entre os ocupados."
                )
                continue

            print(
                f"   ▶ Produto {serial} já tem skid "
                f"{skid.get('rfidTag')} mas nunca arrancou "
                "— a iniciar jornada."
            )

            start_journey(skid, product_id)


# Associa um skid a um produto que está em produção,
# mas perdeu ou não tem uma associação válida
def adopt_orphan_product(
    item: dict,
    product_id: int,
    serial: str
):
    line_id = item.get("productionLineId")

    if not line_id:
        print(
            f"   ✗ Produto {serial} em progresso sem skid "
            "e sem linha conhecida — não é possível adotar."
        )
        return None

    # Procura skids livres na mesma linha do produto
    free_skids = [
        s for s in get_supports(occupied=False)
        if s.get("productionLineId") == line_id
    ]

    if not free_skids:
        print(
            f"   ⚠ Produto {serial} em progresso sem skid, "
            f"e não há skids livres na linha {line_id} para adotar."
        )
        return None

    # Escolhe o primeiro skid disponível
    skid = free_skids[0]

    assigned = assign_product_to_skid(
        skid["id"],
        product_id
    )

    if not assigned:
        return None

    print(
        f"   🩹 Produto {serial} adotado pelo skid "
        f"{skid.get('rfidTag')} da linha {line_id}."
    )

    return skid


# Procura o skid que transporta um determinado produto
def find_skid_for_product(product_id: int):
    occupied_skids = get_supports(occupied=True)

    return next(
        (
            s for s in occupied_skids
            if s.get("currentProductId") == product_id
        ),
        None
    )


# Liberta um skid para poder ser usado por outro produto
def release_skid(skid: dict):
    # Obtém a associação ativa do skid
    supported_product = get_current_supported_product(
        skid["id"]
    )

    if supported_product:
        # Fecha a associação entre produto e skid
        close_supported_product(
            supported_product["id"]
        )

        print(
            f"   ✓ Skid {skid.get('rfidTag')} "
            "libertado para reutilização."
        )


# Trata os produtos que estão atualmente em produção
def advance_in_progress_products(wip_items: list):
    # Percorre todos os produtos WIP
    for item in wip_items:
        product_id = item.get("productId")
        serial = item.get("serialNumber")
        phase_label = item.get("currentPhase")

        # Obtém a fase aberta através da timeline
        active_phase = get_current_phase_from_timeline(
            product_id
        )

        if not active_phase:
            continue

        manufacturing_phase_id = active_phase.get(
            "manufacturingPhaseId"
        )

        product_phase_id = active_phase.get(
            "productPhaseId"
        )

        # --- Portão de tempo "anti-azar": probabilidade crescente de terminar,
        # nunca 100% antes do limite de alerta (150% da duração fixa). ---
        elapsed = item.get("elapsedSeconds") or 0
        predicted = (
            item.get("predictedPhaseDurationSeconds")
            or 1800
        )
        fixed_estimated = (
            item.get("estimatedDuration")
            or 1800
        )
        threshold_pct = (
            item.get("timeThresholdPct")
            or 150
        )
        is_ml = item.get(
            "predictedPhaseDurationIsMl",
            False
        )

        # Calcula a probabilidade de a fase terminar neste ciclo
        prob = completion_probability(
            elapsed,
            predicted,
            fixed_estimated,
            threshold_pct
        )

        # Gera um número aleatório entre 0 e 1.
        # Se for superior ou igual à probabilidade,
        # a fase continua em execução.
        if random.random() >= prob:
            source = "ML" if is_ml else "fixo"

            print(
                f"   ⏳ Produto {serial} | "
                f"Fase {phase_label} | "
                f"{format_duration(elapsed)} "
                f"(previsto {format_duration(predicted)}, "
                f"{source}) | "
                f"prob. de terminar: {prob * 100:.0f}%"
            )

            continue

        # --- Ainda não inspecionámos ESTA tentativa (productPhaseId) desta fase ---
        if product_phase_id not in _inspected_phase_instances:
            # Simula aleatoriamente uma severidade
            severity = simulate_quality_result()

            print(
                f"   🔍 Produto {serial} | "
                f"Fase {phase_label} | "
                f"Quality Check: {severity}"
            )

            # Cria o controlo de qualidade na API
            create_quality_check(
                product_id=product_id,
                manufacturing_phase_id=
                    manufacturing_phase_id,
                severity=severity,
                notes=(
                    "Quality Check automático gerado "
                    "pelo orquestrador. Severidade "
                    f"observada: {severity}"
                ),
            )

            # Marca esta tentativa como já inspecionada
            _inspected_phase_instances.add(
                product_phase_id
            )

            # O resultado será tratado num ciclo seguinte
            continue

        # Obtém os Quality Checks deste produto e desta fase
        checks_for_phase = [
            c for c in get_existing_quality_checks(product_id)
            if c.get("manufacturingPhaseId")
            == manufacturing_phase_id
        ]

        # Escolhe o controlo mais recente,
        # assumindo que IDs maiores são mais recentes
        latest_check = (
            max(
                checks_for_phase,
                key=lambda c: c.get("id", 0)
            )
            if checks_for_phase
            else None
        )

        if latest_check is None:
            # inconsistência momentânea (API ainda a processar) — tenta no próximo ciclo
            continue

        status = latest_check.get("status")

        # --- Scrapped: sai definitivamente da linha ---
        if status == "scrapped":
            print(
                f"   💥 Produto {serial} foi sucateado "
                f"na fase {phase_label} — a sair da linha."
            )

            # Fecha a fase com o resultado scrapped
            closed = close_product_phase(
                product_phase_id,
                result="scrapped",
                quality_id=latest_check.get("id")
            )

            if not closed:
                continue

            # Procura e liberta o skid
            skid = find_skid_for_product(product_id)

            if skid:
                release_skid(skid)

            continue

        # --- Rework: repete o trabalho na mesma fase (reinicia o relógio) ---
        if status == "rework":
            print(
                f"   🔁 Produto {serial} em rework "
                f"na fase {phase_label} — a repetir "
                "o trabalho nesta fase."
            )

            skid = find_skid_for_product(product_id)

            if not skid:
                continue

            # Obtém as workstations da linha do skid
            line_ws = get_workstations_by_line(
                skid["productionLineId"]
            )

            # Procura a workstation da mesma fase
            same_ws = next(
                (
                    w for w in line_ws
                    if w.get("manufacturingPhaseId")
                    == manufacturing_phase_id
                ),
                None,
            )

            if not same_ws:
                print(
                    f"   ✗ Linha {skid['productionLineId']} "
                    "não tem workstation para repetir esta fase."
                )
                continue

            # Envia novamente o skid para a mesma workstation.
            # O backend fecha a tentativa anterior e abre uma nova.
            send_rfid_event(
                skid,
                same_ws
            )  # reabre a fase com um novo startedAt

            continue

        # --- Passed: pode avançar (ou finalizar, se for a última fase) ---
        product = get_product(product_id)

        if not product:
            continue

        # Obtém a sequência de fases do modelo
        phase_seq = get_phase_sequence(
            product["modelId"]
        )

        # Descobre a ordem da fase atual
        current_order = next(
            (
                p.get("order")
                for p in phase_seq
                if p.get("manufacturingPhaseId")
                == manufacturing_phase_id
            ),
            None,
        )

        if current_order is None:
            continue

        # Procura a fase seguinte
        next_entry = next(
            (
                p for p in phase_seq
                if p.get("order") == current_order + 1
            ),
            None
        )

        # Procura o skid que transporta o produto
        skid = find_skid_for_product(product_id)

        if not skid:
            # Tenta corrigir a situação,
            # atribuindo um skid livre da mesma linha
            skid = adopt_orphan_product(
                item,
                product_id,
                serial
            )

            # Acabou de ser adotado agora — só volta a ser processado no próximo ciclo,
            # para dar tempo à API a refletir a nova associação.
            continue

        if next_entry is None:
            # Última fase do modelo, e passou -> finalizar e libertar o skid.
            print(
                f"   🏁 Produto {serial} concluiu "
                "a última fase — a finalizar."
            )

            # Fecha a última fase com o resultado do controlo
            closed = close_product_phase(
                product_phase_id,
                result=status,
                quality_id=latest_check.get("id")
            )

            if not closed:
                continue

            # Liberta o skid
            release_skid(skid)

        else:
            # Ainda existem fases seguintes
            line_ws = get_workstations_by_line(
                skid["productionLineId"]
            )

            # Procura a workstation correspondente
            # à próxima fase da sequência
            target_ws = next(
                (
                    w for w in line_ws
                    if w.get("manufacturingPhaseId")
                    == next_entry.get(
                        "manufacturingPhaseId"
                    )
                ),
                None,
            )

            if not target_ws:
                print(
                    f"   ✗ Linha {skid['productionLineId']} "
                    "não tem workstation para a próxima fase."
                )
                continue

            # A leitura RFID faz o produto avançar:
            # fecha a fase anterior e abre a próxima
            send_rfid_event(skid, target_ws)


# Executa um ciclo completo da simulação
def tick():
    print(
        "\n[Orchestrator] "
        "A processar ciclo da fábrica..."
    )

    # Limpa o cache para obter sequências atualizadas
    # no início de cada novo ciclo
    _model_phase_sequence_cache.clear()

    # Obtém o estado atual do WIP
    wip = get_wip()

    # Produtos que ainda estão à espera
    waiting_items = normalize_list(
        wip.get("waitingItems", [])
    )

    # Produtos atualmente em produção
    in_progress_items = normalize_list(
        wip.get("items", [])
    )

    # Se não existirem produtos ativos, termina o ciclo
    if not waiting_items and not in_progress_items:
        print("   - Fábrica sem produtos ativos.")
        return

    # Primeiro tenta atribuir skids e iniciar produtos
    assign_free_products(waiting_items)

    # Depois trata os produtos já em produção
    advance_in_progress_products(
        in_progress_items
    )


# Ponto principal do programa
def main():
    # Cria o leitor de argumentos do terminal
    parser = argparse.ArgumentParser(
        description=(
            "DRIVOLUTION - Orquestrador de "
            "Simulação Completa da Fábrica"
        )
    )

    # --once executa apenas um ciclo
    parser.add_argument(
        "--once",
        action="store_true",
        help="Executa um ciclo e termina."
    )

    # --interval define o intervalo entre ciclos
    parser.add_argument(
        "--interval",
        type=int,
        default=10,
        help="Intervalo entre ciclos, em segundos."
    )

    # Lê os argumentos
    args = parser.parse_args()

    print("=" * 60)
    print(
        " DRIVOLUTION - Orquestrador "
        "de Simulação da Fábrica"
    )
    print("=" * 60)
    print(f" API: {API_BASE_URL}")
    print(f" IoT Agent: {IOT_AGENT_URL}")

    # Faz login na API antes de iniciar a simulação
    login()

    try:
        # Se foi usado --once, executa apenas um ciclo
        if args.once:
            tick()
            return

        # Caso contrário, executa continuamente
        while True:
            tick()

            # Espera o número de segundos configurado
            time.sleep(args.interval)

    except requests.HTTPError as e:
        # Trata erros devolvidos pela API
        print(f"   ✗ Erro HTTP: {e}")

    except Exception as e:
        # Trata qualquer outro erro inesperado
        print(f"   ✗ Erro: {e}")


# Só executa main() se este ficheiro
# for iniciado diretamente
if __name__ == "__main__":
    main()