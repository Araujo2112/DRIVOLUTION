from PopulatorV2.Populator.ContextManager.AuxContextManager import pkl_exists, clear_entity
from PopulatorV2.Populator.ContextManager.ContextManager import context

from PopulatorV2.Populator.DataGenerator.DependencyLevel0.Client import generate_client, send_client
from PopulatorV2.Populator.DataGenerator.DependencyLevel0.Container import generate_container, send_container
from PopulatorV2.Populator.DataGenerator.DependencyLevel0.PlantFloorSection import generate_plant_floor_section, \
    send_plant_floor_section
from PopulatorV2.Populator.DataGenerator.DependencyLevel0.Product import generate_product, send_product
from PopulatorV2.Populator.DataGenerator.DependencyLevel0.RawMaterial import generate_raw_material, send_raw_material
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.Checkpoint import generate_checkpoints, send_checkpoint
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.ContainerLocalization import generate_container_localization_histories, \
    send_container_localization_history
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.ItemInContainer import generate_items_in_container, send_item_in_container
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.LotOfRawMaterial import generate_lots_of_raw_material, \
    send_lot_of_raw_material
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.ManufacturingPhases import generate_manufacturing_phases, \
    send_manufacturing_phase
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.ManufacturingProcess import generate_manufacturing_processes, \
    send_manufacturing_process
from PopulatorV2.Populator.DataGenerator.DependencyLevel1.ProductLots import generate_product_lots, send_product_lot
from PopulatorV2.Populator.DataGenerator.DependencyLevel2.ManufacturingOrder import generate_manufacturing_orders, \
    send_manufacturing_order
from PopulatorV2.Populator.DataGenerator.DependencyLevel2.ManufacturingProcessPhase import generate_manufacturing_process_phases, \
    send_manufacturing_process_phase
from PopulatorV2.Populator.DataGenerator.DependencyLevel3.ManufacturingOrderHistory import generate_manufacturing_order_histories, \
    send_manufacturing_order_history
from PopulatorV2.Populator.DataGenerator.DependencyLevel3.ManufacturingOrderPhase import generate_manufacturing_order_phases, \
    send_manufacturing_order_phase
from PopulatorV2.Populator.DataGenerator.DependencyLevel4.ItemOfRawMaterial import generate_items_of_raw_material, \
    send_item_of_raw_material
from PopulatorV2.Populator.DataGenerator.DependencyLevel5.ItemLocalization import generate_item_localizations, \
    send_item_localization
from PopulatorV2.Populator.StaticData import StaticData

for index in range(1, 2):

    context.load_context()

    # Fixo
    if pkl_exists() is False:
        for index in range(len(StaticData.dados["contentores"])):
            container = generate_container(index)
            response = send_container(container)
            # if response.status_code == 201 or 200:
            # print(f"Contentor {container['containerName']} criado")

    # Fixo
    if pkl_exists() is False:
        for index in range(len(StaticData.dados["nomes"])):
            client = generate_client()
            # print(f"Cliete {client} gerado com sucesso")
            response = send_client(client)
            # print(response.status_code, response.text)

    # Fixo
    if pkl_exists() is False:
        for index in range(len(StaticData.dados["seccoes_fabrica"])):
            section = generate_plant_floor_section(index)
            response = send_plant_floor_section(section)
    """

            print(response.status_code, response.text)
            if response and response.status_code == 201 or 200:
                print(f"Secção {section['name']} criada com ID: {response.json()['id']}")
            else:
                print("Falha ao criar secção")
    """

    # Fixo
    if pkl_exists() is False:
        for index in range(len(StaticData.dados['produtos'])):
            produto = generate_product(index)
            # print(f"Produto {produto}")
            response = send_product(produto)
            if response and response.status_code == 201 or 200:
                res_data = response.json()
                # print(f"Produto {produto['name']} criado. ID: {res_data['id']}, ProductID: {produto['productId']}")
            else:
                print("Falha ao criar produto")

    # Fixo
    if pkl_exists() is False:
        for index in range(len(StaticData.dados['materias_primas'])):
            material = generate_raw_material(index)
            response = send_raw_material(material)

            if response and response.status_code == 201:
                res_data = response.json()
                # print(f"Matéria-prima {material['name']} criada. RawID: {res_data['rawId']}")
            else:
                print("Falha ao criar matéria-prima")

    # Fixo
    if pkl_exists() is False:
        try:
            all_checkpoints = generate_checkpoints()
            for checkpoint in all_checkpoints:
                response = send_checkpoint(checkpoint)
                # print(response.status_code, response.text)
                if response and response.status_code == 200 or 201:
                    res_data = response.json()
                    # print(f"Checkpoint {checkpoint['name']} criado. Secção: {checkpoint['sectionId']}")
                else:
                    print("Falha ao criar checkpoint")
        except ValueError as e:
            print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_histories = generate_container_localization_histories()
        for history in all_histories:
            response = send_container_localization_history(history)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                # print(
                #    f"Histórico criado: ID {res_data['id']} | Container: {history['containerId']} | Seção: {history['sectionId']}")
            else:
                print("Falha ao criar histórico de localização")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_items = generate_items_in_container(10)
        for item in all_items:
            response = send_item_in_container(item)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                # print(
                #    f"Item {item['itemCode']} criado. Container: {item['containerId']} | ID DB: {res_data['itemInContainerId']}")
            else:
                print("Falha ao criar item no contentor")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_lots = generate_lots_of_raw_material()
        for lot in all_lots:
            response = send_lot_of_raw_material(lot)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                # print(
                #    f"Lote {lot['lotCode']} criado. Matéria-prima: {lot['rawMaterialId']} | ID DB: {res_data['lotId']}")
            else:
                print("Falha ao criar lote de matéria-prima")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Fixo
    if pkl_exists() is False:
        try:
            all_phases = generate_manufacturing_phases()
            for phase in all_phases:
                response = send_manufacturing_phase(phase)
                if response and response.status_code in (200, 201):
                    res_data = response.json()
                    # print(
                    #   f"Fase '{phase['phaseInfo']}' criada. Seção: {phase['plantFloorSectionId']} | ID DB: {res_data['id']}")
                else:
                    print("Falha ao criar fase de produção")
        except ValueError as e:
            print(f"Erro crítico: {str(e)}")

    # Fixo
    if pkl_exists() is False:
        try:
            all_processes = generate_manufacturing_processes()
            for process in all_processes:
                response = send_manufacturing_process(process)
                if response and response.status_code in (200, 201):
                    res_data = response.json()
                    # print(
                    #   f"Processo '{process['processName']}' criado. Produto: {process['productId']} | ID DB: {res_data['id']}")
                else:
                    print("Falha ao criar processo de fabrico")
        except ValueError as e:
            print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_product_lots = generate_product_lots()
        for product_lot in all_product_lots:
            response = send_product_lot(product_lot)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                # print(
                #   f"Product Lot {product_lot['lotNumber']} criado. Produto: {product_lot['productId']} | ID DB: {res_data['id']}")
            else:
                print("Falha ao criar product lot")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_orders = generate_manufacturing_orders()
        for order in all_orders:
            response = send_manufacturing_order(order)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                # print(
                #    f"Ordem {order['orderNumber']} criada. Cliente: {order['clientId']} | Processo: {order['manufacturingProcessId']} | Lote: {order['productLotId']} | ID DB: {res_data['id']}")
            else:
                print("Falha ao criar ordem de fabrico")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Fixo
    if pkl_exists() is False:
        try:
            all_process_phases = generate_manufacturing_process_phases()
            for process_phase in all_process_phases:
                response = send_manufacturing_process_phase(process_phase)
                print("The response Value is:", response)
                if response and response.status_code in (200, 201):
                    print(
                        f"Processo {process_phase['manufacturingProcessId']} - Fase {process_phase['manufacturingPhaseId']} (Ordem {process_phase['numberStepOrder']}) criada.")
                else:
                    print("Falha ao criar ManufacturingProcessPhase")
        except ValueError as e:
            print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_histories = generate_manufacturing_order_histories()
        for history in all_histories:
            response = send_manufacturing_order_history(history)
            if response and response.status_code in (200, 201):
                print(
                    f"Histórico criado para Ordem {history['manufacturingOrderId']} e Seção {history['plantFloorSectionId']}")
            else:
                print("Falha ao criar histórico da ordem de fabrico")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    print(50 * "_")

    # Dinâmico
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

    # Fixo
    # if pkl_exists() is False:
    try:
        print("Starting Generate Item of raw material")
        all_items = generate_items_of_raw_material()
        for item in all_items:
            print("Iteração")
            response = send_item_of_raw_material(item)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                print(
                    f"ItemOfRawMaterial {item['itemCode']} criado. RawMaterial: {item['itemRawId']} | Lote: {item['lotOfRawMaterialId']} | Ordem: {item['manufacturingOrderId']}")
            else:
                print("Falha ao criar ItemOfRawMaterial")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Dinâmico
    try:
        all_localizations = generate_item_localizations()
        for localization in all_localizations:
            response = send_item_localization(localization)
            if response and response.status_code in (200, 201):
                res_data = response.json()
                print(
                    f"ItemLocalization criada: ItemRawId {localization['itemRawId']} | ContainerLocalizationId {localization['containerLocalizationId']} | ID DB: {res_data['itemLocalizationId']}")
            else:
                print("Falha ao criar ItemLocalization")
    except ValueError as e:
        print(f"Erro crítico: {str(e)}")

    # Cleaning dynamic context and moving to the next:

    """
container_localization_histories
items_in_container
lots_of_raw_material
product_lots
manufacturing_orders
manufacturing_order_histories
manufacturing_order_phases
items_of_raw_material
item_localizations
    """

    clear_entity(context, entity_type="container_localization_histories")
    clear_entity(context, entity_type="items_in_container")
    clear_entity(context, entity_type="lots_of_raw_material")
    clear_entity(context, entity_type="product_lots")
    clear_entity(context, entity_type="manufacturing_orders")
    clear_entity(context, entity_type="manufacturing_order_histories")
    clear_entity(context, entity_type="manufacturing_order_phases")
    clear_entity(context, entity_type="items_of_raw_material")
    clear_entity(context, entity_type="item_localizations")

    context.save_context()
    print("Contexto salvo com sucesso!")
