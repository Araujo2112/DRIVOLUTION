import random
from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData.StaticData import dados


def generate_lots_of_raw_material() -> List[dict]:
    """
    Gera lotes de matéria-prima sequencialmente a partir de dados['lotes'],
    associando a matérias-primas existentes (também sequencialmente).
    """
    raw_material_ids = context.get_sorted_db_ids('raw_materials')
    lotes = dados['lotes']

    if not raw_material_ids:
        raise ValueError("É necessário ter matérias-primas no contexto!")
    if not lotes:
        raise ValueError("Não há lotes definidos em dados['lotes']!")

    lots = []
    for i, lote in enumerate(lotes):
        # Associa cada lote a uma matéria-prima, ciclando se necessário
        raw_material_id = raw_material_ids[i % len(raw_material_ids)]
        lots.append({
            "lotCode": lote['numero_lote'],
            "lotNumber": lote['numero_lote'],
            "lotQuantity": lote['quantidade'],
            "lotUnit": lote['unidade'],
            "rawMaterialId": raw_material_id
        })
    return lots


def send_lot_of_raw_material(lot_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/LotOfRawMaterial"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=lot_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_lot = response.json()
            context.add_entity(
                entity_type="lots_of_raw_material",
                entity_id=created_lot['id']['name'],  # URN
                entity_data=created_lot,
                db_id=created_lot['lotId']  # ID da base de dados!
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar lote de matéria-prima: {e}")
        return None
