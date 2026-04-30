import random

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData import StaticData


def generate_container(index: int) -> dict:
    """Gera dados fictícios para um contentor"""
    # Gera um código alfanumérico (ex: "CTN-3A7B")
    container_code = f"CTN-{''.join(random.choices('ABCDEF1234567890', k=4))}"

    # Obtém nome da lista de contentores
    container_name = StaticData.dados['contentores'][index]

    # Volume entre 100-500
    container_volume = random.randint(100, 500)

    return {
        'containerCode': container_code,
        'containerName': container_name,
        'containerVolume': container_volume,
        'activate': True
    }


def send_container(container_data: dict, url: str = f'{BASE_URL}/Container'):
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    response = requests.post(url, headers=headers, json=container_data)

    if response.status_code in (200, 201):
        try:
            created_container = response.json()
            context.add_entity(
                entity_type="containers",
                entity_id=created_container["id"],
                entity_data=container_data,
                db_id=created_container['containerId']
            )
        except Exception as e:
            print(f"Erro ao processar contentor: {str(e)}")

    return response
