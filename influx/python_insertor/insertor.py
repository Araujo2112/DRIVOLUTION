import requests
import time

def testar_pedido_http(metodo, url, headers=None, body=None):
    try:
        metodo = metodo.lower()

        # Dicionário de métodos suportados
        metodos_suportados = {
            "get": requests.get,
            "post": requests.post,
            "put": requests.put,
            "delete": requests.delete,
            "patch": requests.patch,
            "head": requests.head,
            "options": requests.options,
            "connect": None,  # CONNECT não é suportado diretamente no requests
            "trace": None  # TRACE também não é suportado diretamente no requests
        }

        # Verifica se o método é suportado pelo requests
        if metodo in metodos_suportados and metodos_suportados[metodo]:
            resposta = metodos_suportados[metodo](url, headers=headers, json=body)

            # Imprimir código de status e resposta (HEAD não tem corpo de resposta)
            print("Status Code:", resposta.status_code)
            if metodo != "head":
                print("Resposta:", resposta.text)
        elif metodo == "connect":
            print("CONNECT não é suportado diretamente pelo requests.")
        elif metodo == "trace":
            print("TRACE não é suportado diretamente pelo requests.")
        else:
            print("Método HTTP não suportado.")

    except requests.exceptions.RequestException as e:
        print("Erro ao fazer o pedido:", e)

def executar_pedidos_durante_tempo(metodo, url, headers=None, body=None, duracao_total=60, intervalo=5):
    """
    Executa pedidos HTTP continuamente durante um tempo total definido, com intervalo entre os pedidos.

    :param metodo: Método HTTP (GET, POST, etc.).
    :param url: URL do pedido.
    :param headers: Cabeçalhos do pedido (opcional).
    :param body: Corpo do pedido (opcional).
    :param duracao_total: Tempo total para executar os pedidos em segundos.
    :param intervalo: Intervalo entre os pedidos em segundos.
    """
    tempo_inicio = time.time()
    while (time.time() - tempo_inicio) < duracao_total:
        testar_pedido_http(metodo, url, headers, body)
        time.sleep(intervalo)  # Espera pelo intervalo antes de fazer o próximo pedido

# Exemplo de uso
metodo = "PATCH"
url = "http://localhost:1026/ngsi-ld/v1/entities/urn:ngsi-ld:MotionSensor:002/attrs"
headers = {
    "Content-Type": "application/ld+json",
    "Accept": "application/ld+json"
}
body = {

        "status": {
          "type": "Property",
          "value": "Ativo"
        },
        "lastReading": {
          "type": "Property",
          "value": "0"
        },
        "@context": [
          "https://ruicarvalho1.github.io/test-JsonLd/datamodels.context-ngsi.jsonld"
        ]

}

# Executa pedidos durante 60 segundos, com 5 segundos de intervalo entre cada pedido
executar_pedidos_durante_tempo(metodo, url, headers, body, duracao_total=240, intervalo=0)