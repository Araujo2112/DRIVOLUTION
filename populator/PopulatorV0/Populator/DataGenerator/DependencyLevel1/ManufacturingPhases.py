import random
from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData.StaticData import dados


def generate_manufacturing_phases() -> List[dict]:
    """
    Gera fases de produção sequencialmente a partir de dados['fases_producao'],
    associando a seções existentes (também sequencialmente).
    """
    section_ids = context.get_sorted_db_ids('plant_floor_sections')
    fases = dados['fases_producao']

    if not section_ids:
        raise ValueError("É necessário ter seções no contexto!")
    if not fases:
        raise ValueError("Não há fases definidas em dados['fases_producao']!")

    phases = []
    for i, fase in enumerate(fases):
        # Associa cada fase a uma seção, ciclando se necessário
        section_id = section_ids[i % len(section_ids)]
        phases.append({
            "phaseInfo": fase['info'],
            "phaseDuration": int(fase['duracao_horas'] * 60),  # converte horas para minutos se necessário
            "plantFloorSectionId": section_id
        })
    return phases



def send_manufacturing_phase(phase_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ManufacturingPhase"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=phase_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_phase = response.json()
            context.add_entity(
                entity_type="manufacturing_phases",
                entity_id=created_phase['manufacturingPhaseId'],  # URN
                entity_data=created_phase,
                db_id=created_phase['id']  # ID da base de dados!
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar fase de produção: {e}")
        return None
