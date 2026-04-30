from datetime import datetime, timezone
from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_manufacturing_order_histories(status_name: str = "completed") -> List[dict]:
    """
    Gera históricos de ordens de fabrico, associando sequencialmente ordens e seções.
    """
    order_ids = context.get_sorted_db_ids('manufacturing_orders')
    section_ids = context.get_sorted_db_ids('plant_floor_sections')

    if not order_ids or not section_ids:
        raise ValueError("É necessário ter ordens de fabrico e seções no contexto!")

    histories = []
    for i, order_id in enumerate(order_ids):
        # Associa cada ordem a cada seção (ciclando se necessário)
        section_id = section_ids[i % len(section_ids)]
        dt = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
        histories.append({
            "manufacturingOrderId": order_id,
            "plantFloorSectionId": section_id,
            "dateTime": dt,
            "statusName": status_name
        })
    return histories


def send_manufacturing_order_history(history_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ManufacturingOrderHistory"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=history_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_history = response.json()
            # Cria uma chave composta para identificar unicamente o histórico
            composite_id = f"{created_history['manufacturingOrderId']}_{created_history['plantFloorSectionId']}"
            context.add_entity(
                entity_type="manufacturing_order_histories",
                entity_id=composite_id,
                entity_data=created_history,
                db_id=None  # Não há um campo único, é chave composta todo(ver array)
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar histórico da ordem de fabrico: {e}")
        return None
