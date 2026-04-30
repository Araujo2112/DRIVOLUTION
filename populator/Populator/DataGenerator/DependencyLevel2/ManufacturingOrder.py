from datetime import datetime, timezone
from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_manufacturing_orders() -> List[dict]:
    client_ids = context.get_sorted_db_ids('clients')
    process_ids = context.get_sorted_db_ids('manufacturing_processes')
    product_lot_ids = context.get_sorted_db_ids('product_lots')

    num_orders = min(len(client_ids), len(process_ids), len(product_lot_ids))
    if num_orders == 0:
        raise ValueError("É necessário ter clientes, processos e product lots no contexto!")

    orders = []
    for i in range(num_orders):
        order_number = i + 1
        schedule_init = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
        orders.append({
            "orderNumber": order_number,
            "sheduleInit": schedule_init,
            "observations": f"Ordem sequencial {order_number}",
            "manufacturingOrderId": f"urn:ngsi-ld:ManufacturingOrder:{order_number}",
            "clientId": client_ids[i],
            "manufacturingProcessId": process_ids[i],
            "productLotId": product_lot_ids[i]
        })
    return orders


def send_manufacturing_order(order_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ManufacturingOrder"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=order_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_order = response.json()
            context.add_entity(
                entity_type="manufacturing_orders",
                entity_id=created_order['manufacturingOrderId'],
                entity_data=created_order,
                db_id=created_order['id']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar ordem de fabrico: {e}")
        return None
