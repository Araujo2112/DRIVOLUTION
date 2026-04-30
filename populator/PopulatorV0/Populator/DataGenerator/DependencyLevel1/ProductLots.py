from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData.StaticData import dados


def generate_product_lots() -> List[dict]:
    """
    Gera lotes de produto sequencialmente a partir de dados['lotes_produto'],
    associando a produtos existentes (também sequencialmente).
    """
    product_ids = context.get_sorted_db_ids('products')
    lotes_produto = dados['lotes_produto']

    if not product_ids:
        raise ValueError("É necessário ter produtos no contexto!")
    if not lotes_produto:
        raise ValueError("Não há lotes de produto definidos em dados['lotes_produto']!")

    product_lots = []
    for i, lote in enumerate(lotes_produto):
        product_id = product_ids[i % len(product_ids)]  # Cicla se faltar IDs
        product_lots.append({
            "lotNumber": lote['lotNumber'],
            "lotUnit": lote['lotUnit'],
            "lotQuantity": lote['lotQuantity'],
            "ready": lote['ready'],
            "productLotId": lote['productLotId'],
            "productId": product_id
        })
    return product_lots


def send_product_lot(product_lot_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ProductLot"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=product_lot_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_lot = response.json()
            context.add_entity(
                entity_type="product_lots",
                entity_id=created_lot['productLotId'],  # URN
                entity_data=created_lot,
                db_id=created_lot['id']  # ID da base de dados!
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar product lot: {e}")
        return None
