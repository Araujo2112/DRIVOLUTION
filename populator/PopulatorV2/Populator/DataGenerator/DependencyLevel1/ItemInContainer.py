import random
from datetime import datetime, timedelta, timezone
from typing import List
from typing import Optional

import requests

from PopulatorV2.Populator.ContextManager.ContextManager import context
from PopulatorV2.Populator.GlobalConfig.Config import BASE_URL
from PopulatorV2.Populator.RequestPrinter.log_http_request import log_http_request


def generate_items_in_container(num: int) -> List[dict]:
    container_ids = context.get_sorted_db_ids('containers')

    if not container_ids:
        raise ValueError("É necessário ter containers no contexto!")

    items = []
    for i in range(num):
        container_id = random.choice(container_ids)
        item_code = f"Test-{i + 1:03d}"

        dt_in = datetime.now(timezone.utc)
        random_minutes = random.randint(15, 180)
        dt_out = dt_in + timedelta(minutes=random_minutes)
        dt_in_str = dt_in.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
        dt_out_str = dt_out.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'

        items.append({
            "itemCode": item_code,
            "containerId": container_id,
            "dateTimeIn": dt_in_str,
            "dateTimeOut": dt_out_str
        })
    return items


def send_item_in_container(item_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ItemInContainer"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=item_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            log_http_request("POST", url, item_data)
            created_item = response.json()
            context.add_entity(
                entity_type="items_in_container",
                entity_id=created_item['itemCode']['name'],
                entity_data=created_item,
                db_id=created_item['itemInContainerId']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar item no contentor: {e}")
        return None
