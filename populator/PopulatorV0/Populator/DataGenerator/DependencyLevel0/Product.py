import uuid
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL
from Populator.StaticData import StaticData


def generate_product(index: int) -> dict:
    """Gera dados fictícios para um produto"""
    product_template = StaticData.dados['produtos'][index]
    urn_id = f"urn:ngsi-ld:Product:{uuid.uuid4().hex[:3]}"  # 3 dígitos hex
    return {
        "name": product_template["nome"],
        "info": product_template["info"],
        "productId": urn_id  # Usando o formato URN especificado
    }


def send_product(product_data: dict) -> Optional[requests.Response]:
    """Envia o produto para a API e armazena no contexto"""
    url = f"{BASE_URL}/Product"
    headers = {'accept': 'text/plain', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=product_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_product = response.json()
            context.add_entity(
                entity_type="products",
                entity_id=created_product['id'],  # ID real
                entity_data=created_product, # Resposta completa
                db_id=created_product['id']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar produto: {e}")
        return None
