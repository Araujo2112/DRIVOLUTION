from Populator.ContextManager.ContextManager import context
from Populator.DataGenerator.DependencyLevel3.ManufacturingOrderPhase import generate_manufacturing_order_phases, \
    send_manufacturing_order_phase
from Populator.DataGenerator.DependencyLevel4.ItemOfRawMaterial import generate_items_of_raw_material, \
    send_item_of_raw_material

context.load_context()

"""

for index in range(len(StaticData.dados["contentores"])):

    container = generate_container(index)
    response = send_container(container)
    if response.status_code == 201 or 200:
        print(f"Contentor {container['containerName']} criado")

for index in range(len(StaticData.dados["nomes"])):
    client = generate_client()
    print(f"Cliete {client} gerado com sucesso")
    response = send_client(client)
    print(response.status_code, response.text)

for index in range(len(StaticData.dados["seccoes_fabrica"])):
    section = generate_plant_floor_section(index)
    response = send_plant_floor_section(section)

    print(response.status_code, response.text)
    if response and response.status_code == 201 or 200:
        print(f"Secção {section['name']} criada com ID: {response.json()['id']}")
    else:
        print("Falha ao criar secção")

for index in range(len(StaticData.dados['produtos'])):
    produto = generate_product(index)
    print(f"Produto {produto}")
    response = send_product(produto)
    if response and response.status_code == 201 or 200:
        res_data = response.json()
        print(f"Produto {produto['name']} criado. ID: {res_data['id']}, ProductID: {produto['productId']}")
    else:
        print("Falha ao criar produto")

for index in range(len(StaticData.dados['materias_primas'])):
    material = generate_raw_material(index)
    response = send_raw_material(material)

    if response and response.status_code == 201:
        res_data = response.json()
        print(f"Matéria-prima {material['name']} criada. RawID: {res_data['rawId']}")
    else:
        print("Falha ao criar matéria-prima")

try:
    all_checkpoints = generate_checkpoints()
    for checkpoint in all_checkpoints:
        response = send_checkpoint(checkpoint)
        print(response.status_code, response.text)
        if response and response.status_code == 200 or 201:
            res_data = response.json()
            print(f"Checkpoint {checkpoint['name']} criado. Secção: {checkpoint['sectionId']}")
        else:
            print("Falha ao criar checkpoint")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")

try:
    all_histories = generate_container_localization_histories(10)
    for history in all_histories:
        response = send_container_localization_history(history)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(
                f"Histórico criado: ID {res_data['id']} | Container: {history['containerId']} | Seção: {history['sectionId']}")
        else:
            print("Falha ao criar histórico de localização")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")

try:
    all_items = generate_items_in_container(10)
    for item in all_items:
        response = send_item_in_container(item)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(
                f"Item {item['itemCode']} criado. Container: {item['containerId']} | ID DB: {res_data['itemInContainerId']}")
        else:
            print("Falha ao criar item no contentor")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")



try:
    all_lots = generate_lots_of_raw_material(10)
    for lot in all_lots:
        response = send_lot_of_raw_material(lot)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(f"Lote {lot['lotCode']} criado. Matéria-prima: {lot['rawMaterialId']} | ID DB: {res_data['lotId']}")
        else:
            print("Falha ao criar lote de matéria-prima")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")



try:
    all_phases = generate_manufacturing_phases()
    for phase in all_phases:
        response = send_manufacturing_phase(phase)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(f"Fase '{phase['phaseInfo']}' criada. Seção: {phase['plantFloorSectionId']} | ID DB: {res_data['id']}")
        else:
            print("Falha ao criar fase de produção")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")


try:
    all_processes = generate_manufacturing_processes()
    for process in all_processes:
        response = send_manufacturing_process(process)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(f"Processo '{process['processName']}' criado. Produto: {process['productId']} | ID DB: {res_data['id']}")
        else:
            print("Falha ao criar processo de fabrico")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")



try:
    all_product_lots = generate_product_lots()
    for product_lot in all_product_lots:
        response = send_product_lot(product_lot)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(f"Product Lot {product_lot['lotNumber']} criado. Produto: {product_lot['productId']} | ID DB: {res_data['id']}")
        else:
            print("Falha ao criar product lot")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")


try:
    all_orders = generate_manufacturing_orders()
    for order in all_orders:
        response = send_manufacturing_order(order)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(f"Ordem {order['orderNumber']} criada. Cliente: {order['clientId']} | Processo: {order['manufacturingProcessId']} | Lote: {order['productLotId']} | ID DB: {res_data['id']}")
        else:
            print("Falha ao criar ordem de fabrico")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")


try:
    all_process_phases = generate_manufacturing_process_phases()
    for process_phase in all_process_phases:
        response = send_manufacturing_process_phase(process_phase)
        if response and response.status_code in (200, 201):
            print(f"Processo {process_phase['manufacturingProcessId']} - Fase {process_phase['manufacturingPhaseId']} (Ordem {process_phase['numberStepOrder']}) criada.")
        else:
            print("Falha ao criar ManufacturingProcessPhase")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")



try:
    all_histories = generate_manufacturing_order_histories()
    for history in all_histories:
        response = send_manufacturing_order_history(history)
        if response and response.status_code in (200, 201):
            print(f"Histórico criado para Ordem {history['manufacturingOrderId']} e Seção {history['plantFloorSectionId']}")
        else:
            print("Falha ao criar histórico da ordem de fabrico")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")
"""

try:
    all_phases = generate_manufacturing_order_phases()
    for phase in all_phases:
        response = send_manufacturing_order_phase(phase)
        if response and response.status_code in (200, 201):
            print(f"Fase criada para ordem {phase['manufacturingOrderId']} e fase {phase['manufacturingPhaseId']}")
        else:
            print("Falha ao criar fase da ordem de fabrico")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")

print(50 * "_")

try:
    all_items = generate_items_of_raw_material()
    for item in all_items:
        response = send_item_of_raw_material(item)
        if response and response.status_code in (200, 201):
            res_data = response.json()
            print(
                f"ItemOfRawMaterial {item['itemCode']} criado. RawMaterial: {item['itemRawId']} | Lote: {item['lotOfRawMaterialId']} | Ordem: {item['manufacturingOrderId']}")
        else:
            print("Falha ao criar ItemOfRawMaterial")
except ValueError as e:
    print(f"Erro crítico: {str(e)}")

context.save_context()
print("Contexto salvo com sucesso!")
