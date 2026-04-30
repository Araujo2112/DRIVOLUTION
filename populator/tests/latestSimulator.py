import random
from datetime import datetime, timedelta

import requests

BASE_URL = "http://localhost:5181/api"

CLIENT_NAMES = [f"Cliente {x}" for x in ["Alpha", "Beta", "Gamma", "Delta", "Epsilon",
                                         "Zeta", "Eta", "Theta", "Iota", "Kappa",
                                         "Lambda", "Mu", "Nu", "Xi", "Omicron",
                                         "Pi", "Rho", "Sigma", "Tau", "Upsilon"]]
CLIENT_FISCALS = [f"FN{1000 + i}" for i in range(len(CLIENT_NAMES))]

CONTAINER_CODES = [f"CONT{100 + i}" for i in range(10)]
CONTAINER_NAMES = ["Cont-A", "Cont-B", "Cont-C", "Cont-D", "Cont-E"]
CONTAINER_VOLUMES = [10, 20, 50, 75, 100]

PLANT_SECTIONS = [
    {"code": "SEC1", "name": "Recepção"},
    {"code": "SEC2", "name": "Montagem"},
    {"code": "SEC3", "name": "Acabamento"},
    {"code": "SEC4", "name": "Embalagem"}
]

PRODUCT_IDS = ["PROD1", "PROD2", "PROD3"]
PRODUCT_NAMES = ["ProdutoX", "ProdutoY", "ProdutoZ"]
PRODUCT_INFOS = ["Info X", "Info Y", "Info Z"]

RAW_IDS = ["RAW1", "RAW2", "RAW3"]
RAW_MATERIAL_NAMES = ["Matéria1", "Matéria2", "Matéria3"]
RAW_INFOS = ["Info M1", "Info M2", "Info M3"]

CHECKPOINT_CODES = ["CP01", "CP02", "CP03"]
CHECKPOINT_NAMES = ["Entrada", "Meio", "Saída"]

PHASE_IDS = ["PH1", "PH2", "PH3"]
PHASE_INFOS = ["Corte", "Soldagem", "Pintura"]
PHASE_DURATIONS = [15, 30, 45]

PROCESS_IDS = ["PR1", "PR2"]
PROCESS_NAMES = ["Processo Alfa", "Processo Beta"]
PROCESS_INFOS = ["Detalhe A", "Detalhe B"]

LOT_IDS = ["LOT1", "LOT2", "LOT3"]
LOT_NUMBERS = ["L100", "L200", "L300"]
LOT_UNITS = ["kg", "un", "L"]
LOT_QUANTITIES = [100, 200, 300]


def random_datetime(start=None, end=None):
    if start is None: start = datetime.now() - timedelta(days=30)
    if end is None: end = datetime.now()
    delta = end - start
    return (start + timedelta(seconds=random.randint(0, int(delta.total_seconds())))).isoformat()


class ApiSimulator:
    def __init__(self):
        self.clients = []
        self.containers = []
        self.plant_sections = []
        self.products = []
        self.raw_materials = []
        self.checkpoints = []
        self.phases = []
        self.processes = []
        self.product_lots = []
        self.process_phases = []
        self.container_localizations = []
        self.items_in_container = []
        self.lots_of_raw = []
        self.manufacturing_orders = []
        self.order_histories = []
        self.items_of_raw = []
        self.item_localizations = []

    def create_primary_clients(self):
        for name, fiscal in zip(CLIENT_NAMES, CLIENT_FISCALS):
            payload = {"name": name, "fiscalNumber": fiscal}
            resp = requests.post(f"{BASE_URL}/Client", json=payload).json()
            self.clients.append(resp)

    def create_containers(self):
        for code, name, vol in zip(CONTAINER_CODES, CONTAINER_NAMES, CONTAINER_VOLUMES):
            payload = {
                "containerId": 0,
                "containerCode": code,
                "containerName": name,
                "containerVolume": vol,
                "activate": True
            }
            resp = requests.post(f"{BASE_URL}/Container", json=payload).json()
            self.containers.append(resp)

    def create_plant_floor_sections(self):
        for sec in PLANT_SECTIONS:
            payload = {
                "sectionId": 0,
                "sectionCode": sec["code"],
                "name": sec["name"]
            }
            resp = requests.post(f"{BASE_URL}/PlantFloorSection", json=payload).json()
            self.plant_sections.append(resp)

    def create_products(self):
        for pid, name, info in zip(PRODUCT_IDS, PRODUCT_NAMES, PRODUCT_INFOS):
            payload = {"productId": pid, "name": name, "info": info}
            resp = requests.post(f"{BASE_URL}/Product", json=payload).json()
            self.products.append(resp)

    def create_raw_materials(self):
        for rid, name, info in zip(RAW_IDS, RAW_MATERIAL_NAMES, RAW_INFOS):
            payload = {"id": 0, "name": name, "info": info}
            resp = requests.post(f"{BASE_URL}/RawMaterial", json=payload).json()
            self.raw_materials.append(resp)

    def create_checkpoints(self):
        for sec in self.plant_sections:
            for code, name in zip(CHECKPOINT_CODES, CHECKPOINT_NAMES):
                payload = {
                    "checkpointId": 0,
                    "checkpointCode": code,
                    "name": name,
                    "status": True,
                    "sectionId": sec["sectionId"]
                }
                resp = requests.post(f"{BASE_URL}/Checkpoint", json=payload).json()
                self.checkpoints.append(resp)

    def create_manufacturing_phases(self):
        for sec in self.plant_sections:
            for pid, info, dur in zip(PHASE_IDS, PHASE_INFOS, PHASE_DURATIONS):
                payload = {
                    "manufacturingPhaseId": pid,
                    "phaseInfo": info,
                    "phaseDuration": dur,
                    "plantFloorSectionId": sec["sectionId"]
                }
                resp = requests.post(f"{BASE_URL}/ManufacturingPhase", json=payload).json()
                self.phases.append(resp)

    def create_manufacturing_processes(self):
        for pid, name, info in zip(PROCESS_IDS, PROCESS_NAMES, PROCESS_INFOS):
            prod = random.choice(self.products)
            payload = {
                "processName": name,
                "info": info,
                "productId": prod["id"]
            }
            resp = requests.post(f"{BASE_URL}/ManufacturingProcess", json=payload).json()
            self.processes.append(resp)

    def create_product_lots(self):
        for lid, num, unit, qty in zip(LOT_IDS, LOT_NUMBERS, LOT_UNITS, LOT_QUANTITIES):
            prod = random.choice(self.products)
            payload = {
                "productLotId": lid,
                "lotNumber": num,
                "lotUnit": unit,
                "lotQuantity": qty,
                "productId": prod["id"],
                "ready": True
            }
            resp = requests.post(f"{BASE_URL}/ProductLot", json=payload).json()
            self.product_lots.append(resp)

    def create_manufacturing_process_phases(self):
        for proc in self.processes:
            for phase in self.phases:
                payload = {
                    "manufacturingProcessId": proc["id"],
                    "manufacturingPhaseId": phase["id"],
                    "numberStepOrder": 1
                }
                resp = requests.post(f"{BASE_URL}/ManufacturingProcessPhase", json=payload).json()
                self.process_phases.append(resp)

    def run_fixed(self):
        self.create_primary_clients()
        self.create_containers()
        self.create_plant_floor_sections()
        self.create_products()
        self.create_raw_materials()
        self.create_checkpoints()
        self.create_manufacturing_phases()
        self.create_manufacturing_processes()
        self.create_product_lots()
        self.create_manufacturing_process_phases()

    def create_container_localization(self):
        cont = random.choice(self.containers)
        sec = random.choice(self.plant_sections)
        payload = {
            "containerId": cont["containerId"],
            "sectionId": sec["sectionId"],
            "datetime": random_datetime()
        }
        resp = requests.post(f"{BASE_URL}/ContainerLocalizationHistory", json=payload).json()
        self.container_localizations.append(resp)

    def create_item_in_container(self):
        cont = random.choice(self.containers)
        payload = {
            "itemInContainerId": 0,
            "itemCode": "ITM-" + random.choice(LOT_IDS),
            "containerId": cont["containerId"],
            "dateTimeIn": random_datetime(),
            "dateTimeOut": random_datetime()
        }
        resp = requests.post(f"{BASE_URL}/ItemInContainer", json=payload).json()
        self.items_in_container.append(resp)

    def create_lot_of_raw_material(self):
        rm = random.choice(self.raw_materials)
        idx = random.randrange(len(LOT_IDS))
        payload = {
            "lotId": 0,
            "lotCode": LOT_IDS[idx],
            "lotNumber": LOT_NUMBERS[idx],
            "lotQuantity": LOT_QUANTITIES[idx],
            "lotUnit": LOT_UNITS[idx],
            "rawMaterialId": rm["rawId"]
        }
        resp = requests.post(f"{BASE_URL}/LotOfRawMaterial", json=payload).json()
        self.lots_of_raw.append(resp)

    def create_manufacturing_orders(self, per_cycle=50):
        for _ in range(per_cycle):
            client = random.choice(self.clients)
            proc = random.choice(self.processes)
            lot = random.choice(self.product_lots)
            payload = {
                "orderNumber": random.choice([1001, 1002, 1003, 1004, 1005]),
                "sheduleInit": random_datetime(),
                "manufacturingProcessId": proc["id"],
                "productLotId": lot["id"],
                "clientId": client["id"]
            }
            resp = requests.post(f"{BASE_URL}/ManufacturingOrder", json=payload).json()
            self.manufacturing_orders.append(resp)

    def create_manufacturing_order_histories(self):
        order = random.choice(self.manufacturing_orders)
        sec = random.choice(self.plant_sections)
        payload = {
            "manufacturingOrderId": order["id"],
            "plantFloorSectionId": sec["sectionId"],
            "dateTime": random_datetime(),
            "statusName": random.choice(["Iniciado", "Em Progresso", "Concluído"])
        }
        resp = requests.post(f"{BASE_URL}/ManufacturingOrderHistory", json=payload).json()
        self.order_histories.append(resp)

    def create_item_of_raw_material(self):
        lot = random.choice(self.lots_of_raw)
        order = random.choice(self.manufacturing_orders)
        phase = random.choice(self.process_phases)
        conti = random.choice(self.items_in_container)
        payload = {
            "itemCode": "IR-" + random.choice(PHASE_IDS),
            "quantity": random.choice([5, 10, 15, 20]),
            "unit": random.choice(LOT_UNITS),
            "lotOfRawMaterialId": lot["lotId"],
            "manufacturingOrderId": order["id"],
            "manufacturingOrderPhaseId": phase["manufacturingPhaseId"],
            "itemInContainerId": conti["itemInContainerId"]
        }
        resp = requests.post(f"{BASE_URL}/ItemOfRawMaterial", json=payload).json()
        self.items_of_raw.append(resp)

    def create_item_localization(self):
        item = random.choice(self.items_of_raw)
        loc = random.choice(self.container_localizations)
        payload = {
            "itemRawId": item["itemRawId"],
            "containerLocalizationId": loc["id"],
            "dateTime": random_datetime()
        }
        resp = requests.post(f"{BASE_URL}/ItemLocalization", json=payload).json()
        self.item_localizations.append(resp)

    def run_dynamic(self, cycles=10, orders_per_cycle=50):
        for _ in range(cycles):
            self.create_container_localization()
            self.create_item_in_container()
            self.create_lot_of_raw_material()
            self.create_manufacturing_orders(per_cycle=orders_per_cycle)
            self.create_manufacturing_order_histories()
            self.create_item_of_raw_material()
            self.create_item_localization()


if __name__ == "__main__":
    sim = ApiSimulator()
    sim.run_fixed()
    sim.run_dynamic(cycles=20, orders_per_cycle=50)
