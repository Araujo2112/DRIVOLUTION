import random

import requests

from PopulatorV2.Populator.ContextManager.ContextManager import context
from PopulatorV2.Populator.GlobalConfig.Config import BASE_URL
from PopulatorV2.Populator.RequestPrinter.log_http_request import log_http_request
from PopulatorV2.Populator.StaticData import StaticData


def generate_client() -> dict:
    nome = StaticData.dados['nomes'][StaticData.random_index('nomes')] + " " + StaticData.dados['apelidos'][
        StaticData.random_index('apelidos')]
    # print(nome)
    fiscalNumber = str(random.randint(100000000, 999999999))
    return {'name': nome, 'fiscalNumber': fiscalNumber}


def send_client(client: dict, url: str = f'{BASE_URL}/Client'):
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    response = requests.post(url, headers=headers, json=client)

    if response.status_code in (200, 201):
        log_http_request("POST", url, client)
        try:
            created_client = response.json()
            context.add_entity(
                entity_type="clients",
                entity_id=created_client["id"],
                entity_data=client,
                db_id=created_client['id']
            )
        except Exception as e:
            print(f"Erro ao processar resposta: {str(e)}")

    return response
