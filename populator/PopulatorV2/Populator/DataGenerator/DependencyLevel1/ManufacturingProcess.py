from typing import List
from typing import Optional

import requests

from PopulatorV2.Populator.ContextManager.ContextManager import context
from PopulatorV2.Populator.GlobalConfig.Config import BASE_URL
from PopulatorV2.Populator.RequestPrinter.log_http_request import log_http_request
from PopulatorV2.Populator.StaticData import StaticData


def generate_manufacturing_processes() -> List[dict]:
    product_ids = context.get_sorted_db_ids('products')
    processos = StaticData.dados['processos_fabricacao']

    if not product_ids:
        raise ValueError("É necessário ter produtos no contexto!")
    if not processos:
        raise ValueError("Não há processos definidos em dados['processos_fabricacao']!")

    processes = []
    for i, proc in enumerate(processos):
        product_id = product_ids[i % len(product_ids)]
        processes.append({
            "processName": proc['nome'],
            "info": proc['info'],
            "productId": product_id
        })
    return processes


def send_manufacturing_process(process_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ManufacturingProcess"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=process_data)
        if response.status_code in (200, 201):
            log_http_request("POST", url, process_data)
            created_process = response.json()
            context.add_entity(
                entity_type="manufacturing_processes",
                entity_id=created_process['id'],
                entity_data=created_process,
                db_id=created_process['id']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar processo de fabrico: {e}")
        return None
