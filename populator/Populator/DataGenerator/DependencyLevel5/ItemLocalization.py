from datetime import datetime, timezone
from typing import List
from typing import Optional

import requests

from Populator.ContextManager.ContextManager import context
from Populator.GlobalConfig.Config import BASE_URL


def generate_item_localizations() -> List[dict]:
    item_raw_ids = context.get_sorted_db_ids('items_of_raw_material')
    container_localization_ids = context.get_sorted_db_ids('container_localization_histories')

    if not item_raw_ids or not container_localization_ids:
        raise ValueError("É necessário ter itens de matéria-prima e localizações de contentores no contexto!")

    min_len = min(len(item_raw_ids), len(container_localization_ids))
    localizations = []
    for i in range(min_len):
        dt = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
        localizations.append({
            "itemRawId": item_raw_ids[i],
            "containerLocalizationId": container_localization_ids[i],
            "dateTime": dt
        })
    return localizations


def send_item_localization(localization_data: dict) -> Optional[requests.Response]:
    url = f"{BASE_URL}/ItemLocalization"
    headers = {'accept': '*/*', 'Content-Type': 'application/json'}
    try:
        response = requests.post(url, headers=headers, json=localization_data)
        response.raise_for_status()
        if response.status_code in (200, 201):
            created_localization = response.json()
            context.add_entity(
                entity_type="item_localizations",
                entity_id=created_localization['itemLocalizationId'],
                entity_data=created_localization,
                db_id=created_localization['itemLocalizationId']
            )
        return response
    except requests.exceptions.RequestException as e:
        print(f"Erro ao enviar ItemLocalization: {e}")
        return None
