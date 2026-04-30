import random

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData import StaticData


def generate_plant_floor_section(index: int) -> dict:
    """Gera dados fictícios para uma secção da fábrica"""
    # Gera código único para a secção
    section_code = f"SEC-{random.randint(100, 999)}"

    # Obtém nome da lista de secções (apenas 2 primeiros caracteres)
    full_name = StaticData.dados['seccoes_fabrica'][index]
    short_name = full_name[:2]  # Formato reduzido como no exemplo

    return {
        'sectionCode': section_code,
        'name': short_name
    }


def send_plant_floor_section(section_data: dict) -> requests.Response:
    """Envia a secção para a API e armazena no contexto"""
    url = f"{BASE_URL}/PlantFloorSection"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}

    try:
        response = requests.post(url, headers=headers, json=section_data)
        response.raise_for_status()

        # Adiciona ao contexto se criada com sucesso
        if response.status_code in (200, 201):
            created_section = response.json()
            context.add_entity(
                entity_type="plant_floor_sections",
                entity_id=created_section['id'],
                entity_data=section_data,
                db_id=created_section['sectionId']
            )
        return response

    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar secção: {e}")
        return None
