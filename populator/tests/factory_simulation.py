import json
import random
import uuid
from datetime import datetime

import requests
from faker import Faker

fake = Faker()

NGSI_LD_URL = "http://localhost:1026/ngsi-ld/v1/entities/urn:ngsi-ld:RFIDSensor:002/attrs"
API_BASE_URL = "http://localhost:5181/api"
HEADERS_JSON = {"Content-Type": "application/json"}
HEADERS_LD = {"Content-Type": "application/ld+json"}


def post(endpoint, data):
    url = f"{API_BASE_URL}/{endpoint}"
    try:
        response = requests.post(url, headers=HEADERS_JSON, json=data)
        if response.status_code == 200:
            print(f"[POST] {endpoint} - OK")
            return response.json()
        else:
            print(f"[POST] {endpoint} - ERROR {response.status_code}")
            print(response.text)
            return None
    except Exception as e:
        print(f"[POST] {endpoint} - Exception: {e}")
        return None


def patch_ngsi_ld(sector_id, item_raw_id):
    payload = {
        "tagDetected": {"type": "Property", "value": True},
        "refLocation": {"type": "Relationship", "object": f"urn:ngsi-ld:Sector:{sector_id}"},
        "refRawMaterials": {"type": "Relationship", "object": [f"urn:ngsi-ld:ItemOfRawMaterial:{item_raw_id}"]},
        "@context": ["https://ruicarvalho1.github.io/test-JsonLd/datamodels.context-ngsi.jsonld"]
    }
    try:
        response = requests.patch(NGSI_LD_URL, headers=HEADERS_LD, data=json.dumps(payload))
        if response.status_code in (200, 204):
            print(" NGSI-LD PATCH succeeded.")
        else:
            print(f" NGSI-LD PATCH failed ({response.status_code}): {response.text}")
    except Exception as e:
        print(f" NGSI-LD PATCH exception: {e}")


def generate_and_populate_unit():
    client = post("Client", {
        "name": fake.company(),
        "fiscalNumber": fake.unique.random_number(digits=9)
    })

    product = post("Product", {
        "productId": str(uuid.uuid4())[:8],
        "name": fake.bs().title(),
        "info": fake.catch_phrase()
    })

    section = post("PlantFloorSection", {
        "sectionCode": f"SCT{random.randint(100, 999)}",
        "name": f"Section {fake.word().capitalize()}"
    })

    process = post("ManufacturingProcess", {
        "processName": f"{fake.color_name()} Process",
        "info": fake.sentence(),
        "productId": product["id"]
    })

    lot = post("ProductLot", {
        "productLotId": str(uuid.uuid4())[:8],
        "productId": product["id"],
        "lotNumber": f"LOT-{random.randint(1000, 9999)}",
        "lotUnit": "kg",
        "lotQuantity": random.randint(50, 150),
        "ready": True,
        "info": fake.catch_phrase(),
        "productName": product["name"]
    })

    order = post("ManufacturingOrder", {
        "orderNumber": random.randint(1000, 9999),
        "sheduleInit": datetime.utcnow().isoformat(),
        "clientId": client["id"],
        "manufacturingProcessId": process["id"],
        "productLotId": lot["id"],
        "observations": fake.sentence()
    })

    raw_material = post("RawMaterial", {
        "name": fake.word().capitalize(),
        "info": fake.text(max_nb_chars=20)
    })

    lot_raw = post("LotOfRawMaterial", {
        "lotCode": str(uuid.uuid4())[:6],
        "lotNumber": f"RAW-{random.randint(1000, 9999)}",
        "lotQuantity": random.randint(20, 100),
        "lotUnit": "kg",
        "rawMaterialId": raw_material["id"],
        "lotId": random.randint(100, 999)
    })

    item = post("ItemOfRawMaterial", {
        "itemCode": str(uuid.uuid4())[:6],
        "quantity": random.randint(5, 25),
        "unit": "kg",
        "lotOfRawMaterialId": lot_raw["lotId"],
        "manufacturingOrderId": order["id"],
        "itemInContainerId": 1
    })

    if section and item:
        patch_ngsi_ld(section["sectionId"], item["itemRawId"])


NUM_CICLOS = 5
for i in range(NUM_CICLOS):
    print(f"\n🔁 Criando unidade {i + 1}/{NUM_CICLOS}")
    generate_and_populate_unit()
