from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_manufacturing_process_phases() -> List[dict]:
    process_ids = context.get_sorted_db_ids('manufacturing_processes')
    phase_ids = context.get_sorted_db_ids('manufacturing_phases')

    if not process_ids or not phase_ids:
        raise ValueError("É necessário ter processos e fases no contexto!")

    process_phases = []
    for proc_idx, process_id in enumerate(process_ids):
        for phase_idx, phase_id in enumerate(phase_ids):
            if process_id == phase_id:
                process_phases.append({
                    "manufacturingProcessId": process_id,
                    "manufacturingPhaseId": phase_id,
                    "numberStepOrder": phase_idx + 1  # começa em 1
                })
    return process_phases


def send_manufacturing_process_phase(phase_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ManufacturingProcessPhase"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=phase_data)
        if response.status_code in (200, 201):
            created_phase = response.json()
            context.add_entity(
                entity_type="manufacturing_process_phases",
                entity_id=f"{created_phase['manufacturingProcessId']}_{created_phase['manufacturingPhaseId']}",
                entity_data=created_phase,
                db_id=None
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar processo-fase: {e}")
        return None
