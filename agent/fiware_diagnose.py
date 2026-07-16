"""
DRIVOLUTION - Diagnóstico FIWARE
Verifica o estado atual do IoT Agent e Orion.
"""

import requests
import json

# Endereço do Orion Context Broker
ORION_URL = "http://localhost:1026"

# Endereço da API de gestão do IoT Agent
IOT_AGENT_URL = "http://localhost:4041"

# Cabeçalhos FIWARE usados nos pedidos
HEADERS_JSON = {
    # Identifica o serviço FIWARE
    "FIWARE-Service": "drivolution",

    # Define o caminho dentro desse serviço
    "FIWARE-ServicePath": "/",
}


# Função genérica que faz um pedido GET
# e mostra o resultado no terminal
def check(label, url, headers=None):
    try:
        # Faz o pedido ao endereço recebido
        r = requests.get(
            url,

            # Se forem enviados headers, usa-os.
            # Caso contrário, usa um dicionário vazio.
            headers=headers or {},

            # Cancela o pedido se demorar mais de 5 segundos
            timeout=5
        )

        # Mostra uma separação visual no terminal
        print(f"\n{'=' * 55}")

        # Mostra o nome do teste
        print(f"  {label}")

        # Mostra o código HTTP devolvido
        print(f"  Status: {r.status_code}")

        print(f"{'=' * 55}")

        try:
            # Tenta converter a resposta para JSON
            data = r.json()

            # Mostra o JSON formatado
            print(
                json.dumps(
                    data,

                    # Adiciona indentação para facilitar a leitura
                    indent=2,

                    # Mantém corretamente os caracteres portugueses
                    ensure_ascii=False
                )[:2000]
            )

        except json.JSONDecodeError:
            # Se a resposta não for JSON,
            # mostra os primeiros 500 caracteres do texto
            print(r.text[:500])

    except Exception as e:
        # Trata erros de rede, timeout ou outros problemas
        print(f"\n✗ {label}: {e}")


# Este bloco só é executado quando o ficheiro
# é iniciado diretamente
if __name__ == "__main__":

    # Verifica se o Orion está ativo
    # e mostra a versão instalada
    check(
        "Orion - versão",
        f"{ORION_URL}/version"
    )

    # Verifica o estado geral do IoT Agent
    check(
        "IoT Agent - estado",
        f"{IOT_AGENT_URL}/iot/about"
    )

    # Lista os dispositivos registados no IoT Agent
    check(
        "IoT Agent - dispositivos",
        f"{IOT_AGENT_URL}/iot/devices",
        HEADERS_JSON
    )

    # Lista os service groups configurados no IoT Agent
    check(
        "IoT Agent - serviços",
        f"{IOT_AGENT_URL}/iot/services",
        HEADERS_JSON
    )

    # Lista até 10 entidades do tipo Skid existentes no Orion
    check(
        "Orion - entidades Skid",
        f"{ORION_URL}/ngsi-ld/v1/entities"
        f"?type=Skid&limit=10",
        HEADERS_JSON
    )

    # Lista as subscrições configuradas no Orion
    check(
        "Orion - subscrições",
        f"{ORION_URL}/ngsi-ld/v1/subscriptions",
        HEADERS_JSON
    )