from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_items_of_raw_material() -> List[dict]:
    """
    Gera itens de matéria-prima associando todos os dados necessários,
    usando os valores reais de quantidade e unidade da matéria-prima.
    """
    raw_materials = context.storage.get('raw_materials')
    lot_ids = context.get_sorted_db_ids('lots_of_raw_material')
    order_ids = context.get_sorted_db_ids('manufacturing_orders')
    item_in_container_ids = context.get_sorted_db_ids('items_in_container')
    order_phase_ids = context.get_sorted_db_ids('manufacturing_order_phases')

    print("Starting")
    print(raw_materials)
    print(lot_ids)
    print(order_ids)
    print(item_in_container_ids)
    print(order_phase_ids)
    print("Ending")

    min_len = min(len(raw_materials), len(lot_ids), len(order_ids), len(item_in_container_ids), len(order_phase_ids))
    if min_len == 0:
        raise ValueError("É necessário ter todas as entidades associadas no contexto!")

    items = []
    for i in range(min_len):
        raw_material = raw_materials[i]
        raw_id = raw_material['db_id']
        # Pegando dados reais da matéria-prima
        raw_data = raw_material['data']
        # Supondo que você armazenou os campos 'quantity' e 'unit' ou similares
        # Se não, substitua pelos valores corretos do seu contexto/dados estáticos
        quantity = raw_data.get('quantidade', 10)  # fallback 10
        unit = raw_data.get('unidade', 'kg')  # fallback 'kg'

        items.append({
            "itemRawId": raw_id,
            "itemCode": f"ITEM-{raw_id}-{i + 1:03d}",
            "quantity": quantity,
            "unit": unit,
            "lotOfRawMaterialId": lot_ids[i],
            "manufacturingOrderId": order_ids[i],
            "itemInContainerId": item_in_container_ids[i],
            "manufacturingOrderPhaseId": order_phase_ids[i]
        })
    return items


def send_item_of_raw_material(item_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ItemOfRawMaterial"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=item_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_item = response.json()
            context.add_entity(
                entity_type="items_of_raw_material",
                entity_id=created_item['code']['name'],  # URN
                entity_data=created_item,
                db_id=created_item['itemRawId']  # ID da base de dados!
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar item de matéria-prima: {e}")
        return None
