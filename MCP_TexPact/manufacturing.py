import os
import requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")


class ClientModel(BaseModel):
    id: int
    name: str
    fiscalNumber: Optional[str]

class ManufacturingProcessModel(BaseModel):
    id: int
    processName: str

class ProductModel(BaseModel):
    id: int
    name: str

class ManufacturingOrderModel(BaseModel):
    id: int
    orderNumber: int
    sheduleInit: str
    observations: str
    manufacturingOrderId: str

    clientId: int
    manufacturingProcessId: int
    productLotId: int

    # Campos enriquecidos
    clientName: str
    manufacturingProcessName: str
    productLotName: str

class LotOfRawMaterialModel(BaseModel):
    lotId: int
    id: dict
    lotNumber: dict
    lotQuantity: int
    lotUnit: str
    rawMaterialId: int
    historicalWeights: List[int]

    @property
    def lot_number_value(self) -> str:
        return self.lotNumber.get("name", f"LOT-{self.lotId}")

    @property
    def id_value(self) -> str:
        return self.id.get("name", f"urn:ngsi-ld:LotOfRawMaterial:{self.lotId}")


def fetch_clients() -> List[ClientModel]:
    resposta = requests.get(f"{API_BASE}/Client")
    resposta.raise_for_status()
    return [ClientModel(**item) for item in resposta.json()]

def fetch_client_by_id(client_id: int) -> Optional[ClientModel]:
    return next((c for c in fetch_clients() if c.id == client_id), None)

def fetch_manufacturing_processes() -> List[ManufacturingProcessModel]:
    resposta = requests.get(f"{API_BASE}/ManufacturingProcess")
    resposta.raise_for_status()
    return [ManufacturingProcessModel(**item) for item in resposta.json()]

def fetch_manufacturing_process_by_id(process_id: int) -> Optional[ManufacturingProcessModel]:
    return next((p for p in fetch_manufacturing_processes() if p.id == process_id), None)

def fetch_products() -> List[ProductModel]:
    resposta = requests.get(f"{API_BASE}/Product")
    resposta.raise_for_status()
    return [ProductModel(**item) for item in resposta.json()]

def fetch_product_by_id(prod_id: int) -> Optional[ProductModel]:
    return next((p for p in fetch_products() if p.id == prod_id), None)


def fetch_lots_of_raw_material() -> List[LotOfRawMaterialModel]:
    print(f"Buscando lotes de matéria-prima em {API_BASE}/LotOfRawMaterial")
    resposta = requests.get(f"{API_BASE}/LotOfRawMaterial")
    resposta.raise_for_status()

    resultados: List[LotOfRawMaterialModel] = []
    for item in resposta.json():
        try:
            resultados.append(LotOfRawMaterialModel(**item))
        except ValidationError as e:
            print(f"Falha de validação em item {item}: {e}")

    print(f"Foram obtidos {len(resultados)} lotes de matéria-prima")
    return resultados

def fetch_lot_of_raw_material_by_id(lot_id: int) -> Optional[LotOfRawMaterialModel]:
    print(f"Buscando lote de matéria-prima com ID {lot_id}")
    try:
        lotes = fetch_lots_of_raw_material()
        lote = next((l for l in lotes if l.lotId == lot_id), None)
        if lote:
            print(f"Lote {lot_id} encontrado: {lote.lot_number_value}")
            print(f"Quantidade: {lote.lotQuantity} {lote.lotUnit}")
            print(f"ID da matéria-prima associada: {lote.rawMaterialId}")
        else:
            print(f"Lote {lot_id} não encontrado")
        return lote
    except Exception as e:
        print(f"Erro ao buscar lote {lot_id}: {e}")
        return None



def fetch_manufacturing_orders() -> List[ManufacturingOrderModel]:
    resposta = requests.get(f"{API_BASE}/ManufacturingOrder")
    resposta.raise_for_status()
    lista_bruta = resposta.json()

    enriquecidos: List[ManufacturingOrderModel] = []
    for registro in lista_bruta:
        base = {
            "id": registro["id"],
            "orderNumber": registro["orderNumber"],
            "sheduleInit": registro["sheduleInit"],
            "observations": registro["observations"],
            "manufacturingOrderId": registro["manufacturingOrderId"],
            "clientId": registro["clientId"],
            "manufacturingProcessId": registro["manufacturingProcessId"],
            "productLotId": registro["productLotId"],
        }

        cliente = fetch_client_by_id(registro["clientId"])
        processo = fetch_manufacturing_process_by_id(registro["manufacturingProcessId"])
        produto = fetch_product_by_id(registro["productLotId"])

        base.update({
            "clientName": cliente.name if cliente else "",
            "manufacturingProcessName": processo.processName if processo else "",
            "productLotName": produto.name if produto else "",
        })

        enriquecidos.append(ManufacturingOrderModel(**base))

    return enriquecidos



class ItemOfRawMaterialModel(BaseModel):
    itemRawId: int
    quantity: int
    unit: str
    lotOfRawMaterialId: int
    itemInContainerId: int
    manufacturingOrderPhaseId: int
    manufacturingOrderId: int

def fetch_items_of_raw_material() -> List[ItemOfRawMaterialModel]:
    resposta = requests.get(f"{API_BASE}/ItemOfRawMaterial")
    resposta.raise_for_status()
    return [ItemOfRawMaterialModel(**item) for item in resposta.json()]

def fetch_items_of_raw_by_order(order_id: int) -> List[ItemOfRawMaterialModel]:
    return [i for i in fetch_items_of_raw_material() if i.manufacturingOrderId == order_id]

class ManufacturingOrderPhaseModel(BaseModel):
    id: int
    manufacturingOrderId: int
    manufacturingPhaseId: int
    quantity: int
    sheduleInit: str
    dateTimeInit: str
    dateTimeEnd: str

def fetch_manufacturing_order_phases() -> List[ManufacturingOrderPhaseModel]:
    resposta = requests.get(f"{API_BASE}/ManufacturingOrderPhase")
    resposta.raise_for_status()
    return [ManufacturingOrderPhaseModel(**item) for item in resposta.json()]

def fetch_phases_by_order(order_id: int) -> List[ManufacturingOrderPhaseModel]:
    return [p for p in fetch_manufacturing_order_phases() if p.manufacturingOrderId == order_id]

class ManufacturingOrderHistoryModel(BaseModel):
    manufacturingOrderId: int
    plantFloorSectionId: int
    dateTime: str
    statusName: str

def fetch_manufacturing_order_history() -> List[ManufacturingOrderHistoryModel]:
    resposta = requests.get(f"{API_BASE}/ManufacturingOrderHistory")
    resposta.raise_for_status()
    return [ManufacturingOrderHistoryModel(**item) for item in resposta.json()]

def fetch_history_by_order(order_id: int) -> List[ManufacturingOrderHistoryModel]:
    return [h for h in fetch_manufacturing_order_history() if h.manufacturingOrderId == order_id]

class ItemLocalizationModel(BaseModel):
    itemLocalizationId: int
    itemRawId: int
    containerLocalizationId: int
    dateTime: str

def fetch_item_localizations() -> List[ItemLocalizationModel]:
    resposta = requests.get(f"{API_BASE}/ItemLocalization")
    resposta.raise_for_status()
    return [ItemLocalizationModel(**item) for item in resposta.json()]

def fetch_item_localizations_by_order(order_id: int) -> List[ItemLocalizationModel]:
    itens = fetch_items_of_raw_by_order(order_id)
    valid_ids = {i.itemRawId for i in itens}
    return [loc for loc in fetch_item_localizations() if loc.itemRawId in valid_ids]

_lots_cache = {}

def fetch_lot_of_raw_material_by_id_cached(lot_id: int) -> Optional[LotOfRawMaterialModel]:
    if lot_id in _lots_cache:
        print(f"Lote {lot_id} obtido do cache")
        return _lots_cache[lot_id]

    lote = fetch_lot_of_raw_material_by_id(lot_id)
    if lote:
        _lots_cache[lot_id] = lote
    return lote

def clear_lots_cache():
    global _lots_cache
    _lots_cache.clear()
    print("Cache de lotes limpo")


def fetch_raw_material_by_id(raw_material_id: int):
    try:
        resposta = requests.get(f"{API_BASE}/RawMaterial")
        resposta.raise_for_status()
        materiais = resposta.json()

        for data in materiais:
            if data.get("rawId") == raw_material_id:
                print(f"Matéria-prima {raw_material_id} encontrada: {data.get('name')}")
                class RawMaterial:
                    def __init__(self, d):
                        self.id = d.get("rawId")
                        self.rawId = d.get("rawId")
                        self.name = d.get("name", "Desconhecido")
                        self.info = d.get("info", "")
                return RawMaterial(data)

        print(f"Matéria-prima {raw_material_id} não encontrada")
        return None
    except Exception as e:
        print(f"Erro ao buscar matéria-prima {raw_material_id}: {e}")
        return None
