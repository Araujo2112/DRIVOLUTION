import random
from datetime import datetime, timezone
from typing import List
from typing import Optional

import requests

from PopulatorV2.Populator.ContextManager.ContextManager import context
from PopulatorV2.Populator.GlobalConfig.Config import BASE_URL
from PopulatorV2.Populator.RequestPrinter.log_http_request import log_http_request


def generate_container_localization_histories() -> List[dict]:
    container_ids = context.get_sorted_db_ids('containers')
    section_ids = context.get_sorted_db_ids('plant_floor_sections')

    if not container_ids or not section_ids:
        raise ValueError("É necessário ter contentores e secções no contexto!")

    histories = []

    for container_id in container_ids:
        modo = random.choice(['sequencial', 'shuffle'])
        # print(f"Container {container_id} modo: {modo}")

        if modo == 'sequencial':
            chosen_sections = section_ids
        else:
            chosen_sections = section_ids[:]
            random.shuffle(chosen_sections)

        for section_id in chosen_sections:
            dt = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
            histories.append({
                "containerId": container_id,
                "sectionId": section_id,
                "datetime": dt
            })

    return histories


def send_container_localization_history(history_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ContainerLocalizationHistory"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=history_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            log_http_request("POST", url, history_data)
            created_history = response.json()
            context.add_entity(
                entity_type="container_localization_histories",
                entity_id=created_history['id'],
                entity_data=created_history,
                db_id=created_history['id']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar histórico de localização: {e}")
        return None
