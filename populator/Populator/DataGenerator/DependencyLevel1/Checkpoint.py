from typing import List, Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData import StaticData


def generate_checkpoints() -> List[dict]:
    section_ids = context.get_sorted_db_ids('plant_floor_sections')

    pontos_controlo = StaticData.dados['pontos_controlo']
    num_checkpoints = len(pontos_controlo)

    if len(section_ids) < num_checkpoints:
        raise ValueError(
            f"Faltam seções! Necessárias: {num_checkpoints}, Disponíveis: {len(section_ids)}"
        )

    checkpoints = []
    for i, ponto in enumerate(pontos_controlo):
        checkpoints.append({
            "checkpointCode": f"CPT-{i + 1:03d}",
            "name": ponto,
            "status": True,
            "sectionId": section_ids[i]
        })

    return checkpoints


def send_checkpoint(checkpoint_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/Checkpoint"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    print(checkpoint_data)

    try:
        response = requests.post(url, headers=headers, json=checkpoint_data)
        response.raise_for_status()

        if response.status_code in (200, 201):
            created_checkpoint = response.json()
            print("Createed Checkpoint", created_checkpoint)
            context.add_entity(
                entity_type="checkpoints",
                entity_id=created_checkpoint['id'],
                entity_data=created_checkpoint,
                db_id=created_checkpoint['checkpointId']
            )
        return response

    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar checkpoint: {e}")
        return None
