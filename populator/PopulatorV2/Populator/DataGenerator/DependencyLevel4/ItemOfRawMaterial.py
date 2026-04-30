from typing import List
from typing import Optional

import requests

from PopulatorV2.Populator.ContextManager.ContextManager import context
from PopulatorV2.Populator.GlobalConfig.Config import BASE_URL
from PopulatorV2.Populator.RequestPrinter.log_http_request import log_http_request

# Fazer os for's entre o max values para associação dinâmica proporcional entre dados

# Manufacturing Order
# Item in Container

"""
def generate_items_of_raw_material() -> List[dict]:
    raw_materials = context.storage.get('raw_materials')
    lot_ids = context.get_sorted_db_ids('lots_of_raw_material')
    order_ids = context.get_sorted_db_ids('manufacturing_orders')
    item_in_container_ids = context.get_sorted_db_ids('items_in_container')
    order_phase_ids = context.get_sorted_db_ids('manufacturing_order_phases')
    items_of_raw_material_id = context.get_sorted_db_ids('items_of_raw_material_id_manager')

    # Verificação de tamanhos fixos (podes tornar isso dinâmico se precisares)
    if not (len(raw_materials) == len(lot_ids) == len(order_ids) == 5):
        raise ValueError("raw_materials, lot_ids e order_ids devem ter exatamente 5 elementos.")
    if not (len(item_in_container_ids) == 10 and len(order_phase_ids) == 25):
        raise ValueError("item_in_container_ids deve ter 10 e order_phase_ids deve ter 25 elementos.")

    items = []
    aux = max([int(v) for v in items_of_raw_material_id if str(v).isdigit()], default=0)

    # Proporções exatas
    container_per_raw = len(item_in_container_ids) // len(raw_materials)  # 10 / 5 = 2
    phase_per_container = len(order_phase_ids) // len(item_in_container_ids)  # 25 / 10 = 2 (mas 5 no total por raw)

    for i in range(len(raw_materials)):
        raw_material = raw_materials[i]
        lot_id = lot_ids[i]
        order_id = order_ids[i]
        raw_data = raw_material['data']
        quantity = raw_data.get('quantidade', 10)
        unit = raw_data.get('unidade', 'kg')

        for j in range(container_per_raw):
            container_index = i * container_per_raw + j
            container_id = item_in_container_ids[container_index]

            for k in range(phase_per_container):
                phase_index = container_index * phase_per_container + k
                phase_id = order_phase_ids[phase_index]

                aux += 1
                items.append({
                    "itemRawId": aux,
                    "itemCode": f"ITEM-{aux}-{i + 1:03d}",
                    "quantity": quantity,
                    "unit": unit,
                    "lotOfRawMaterialId": lot_id,
                    "manufacturingOrderId": order_id,
                    "itemInContainerId": container_id,
                    "manufacturingOrderPhaseId": phase_id
                })

                context.add_entity(
                    entity_type="items_of_raw_material_id_manager",
                    entity_id=aux,
                    entity_data=aux,
                    db_id=aux
                )

    return items
"""


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
    # print(item_data)
    try:

        response = requests.post(url, headers=headers, json=item_data)
        # response.raise_for_status()
        print("Status code is", response.status_code)
        log_http_request("POST", url, item_data)

        if response.status_code in (200, 201):
            created_item = response.json()
            context.add_entity(
                entity_type="items_of_raw_material",
                entity_id=created_item['code']['name'],
                entity_data=created_item,
                db_id=created_item['itemRawId']
            )

        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar item de matéria-prima: {e}")
        return None
