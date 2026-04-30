from datetime import datetime, timezone, timedelta
from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_manufacturing_order_phases() -> List[dict]:
    """
    Gera fases da ordem de fabrico associando ordens e fases existentes,
    usando dados obrigatórios da tabela.
    """
    order_ids = context.get_sorted_db_ids('manufacturing_orders')
    phase_ids = context.get_sorted_db_ids('manufacturing_phases')

    if not order_ids or not phase_ids:
        raise ValueError("É necessário ter ordens e fases no contexto!")

    phases = []
    now = datetime.now(timezone.utc)
    for order_id in order_ids:
        for idx, phase_id in enumerate(phase_ids):
            shedule_init = now.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
            date_time_init = shedule_init
            date_time_end = (now + timedelta(hours=idx+1)).strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
            phases.append({
                "customizationParams": f"Phase {idx+1} customization",
                "quantity": 10,  # Ou busque do contexto se já tiver
                "sheduleInit": shedule_init,
                "dateTimeInit": date_time_init,
                "dateTimeEnd": date_time_end,
                "manufacturingOrderId": order_id,
                "manufacturingPhaseId": phase_id
            })
    return phases



def send_manufacturing_order_phase(phase_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ManufacturingOrderPhase"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=phase_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_phase = response.json()
            context.add_entity(
                entity_type="manufacturing_order_phases",
                entity_id=created_phase['id'],
                entity_data=created_phase,
                db_id=created_phase['id']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar ManufacturingOrderPhase: {e}")
        return None
