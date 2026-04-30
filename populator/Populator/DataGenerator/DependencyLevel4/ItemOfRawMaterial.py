from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_items_of_raw_material() -> List[dict]:
    raw_materials = context.storage.get('raw_materials')
    lot_ids = context.get_sorted_db_ids('lots_of_raw_material')
    order_ids = context.get_sorted_db_ids('manufacturing_orders')
    item_in_container_ids = context.get_sorted_db_ids('items_in_container')
    order_phase_ids = context.get_sorted_db_ids('manufacturing_order_phases')
    ## Id Rastreability
    items_of_raw_material_id = context.get_sorted_db_ids('items_of_raw_material_id_manager')
    print("Os items of raw material são:", items_of_raw_material_id)

    min_len = min(len(raw_materials), len(lot_ids), len(order_ids), len(item_in_container_ids), len(order_phase_ids))
    if min_len == 0:
        raise ValueError("É necessário ter todas as entidades associadas no contexto!")

    items = []
    for i in range(min_len):
        raw_material = raw_materials[i]
        raw_id = raw_material['db_id']
        raw_data = raw_material['data']
        quantity = raw_data.get('quantidade', 10)
        unit = raw_data.get('unidade', 'kg')

        items.append({
            "itemRawId": raw_id,
            "itemCode": f"ITEM-{items_of_raw_material_id}-{i + 1:03d}",
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
    print(item_data)
    try:
        response = requests.post(url, headers=headers, json=item_data)
        # response.raise_for_status()
        print("Status code is", response.status_code)
        if response.status_code in (200, 201):
            created_item = response.json()
            context.add_entity(
                entity_type="items_of_raw_material",
                entity_id=created_item['code']['name'],
                entity_data=created_item,
                db_id=created_item['itemRawId']
            )
            context.add_entity(
                entity_type="items_of_raw_material_id_manager",
                entity_id=created_item['code']['name'],
                entity_data=created_item,
                db_id=created_item['itemRawId']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar item de matéria-prima: {e}")
        return None
