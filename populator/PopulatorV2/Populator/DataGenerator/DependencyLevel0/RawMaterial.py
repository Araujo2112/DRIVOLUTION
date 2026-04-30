from typing import Optional

import requests

from PopulatorV2.Populator.ContextManager.ContextManager import context
from PopulatorV2.Populator.GlobalConfig.Config import BASE_URL
from PopulatorV2.Populator.RequestPrinter.log_http_request import log_http_request
from PopulatorV2.Populator.StaticData import StaticData


def generate_raw_material(index: int) -> dict:
    material_template = StaticData.dados['materias_primas'][index]
    # print(material_template)
    return {
        'name': material_template['nome'],
        'info': material_template['info']
    }


def send_raw_material(material_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/RawMaterial"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}

    try:
        response = requests.post(url, headers=headers, json=material_data)
        response.raise_for_status()
        # print("Raw Material Response", response.json())

        if response.status_code in (200, 201):
            log_http_request("POST", url, material_data)
            created_material = response.json()
            context.add_entity(
                entity_type="raw_materials",
                entity_id=created_material['rawId'],
                entity_data=created_material,
                db_id=created_material['rawId']
            )
        return response

    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar matéria-prima: {e}")
        return None
