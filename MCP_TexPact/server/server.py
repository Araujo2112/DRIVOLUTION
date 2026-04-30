import os
import sys
import grpc
from concurrent import futures
from dotenv import load_dotenv

from grpc_reflection.v1alpha import reflection

HERE = os.path.dirname(__file__)
ROOT = os.path.dirname(HERE)

sys.path.insert(0, ROOT)
sys.path.insert(0, os.path.join(HERE, "py_server"))
sys.path.insert(0, os.path.dirname(HERE))

import clients_pb2, clients_pb2_grpc
import containers_pb2, containers_pb2_grpc
import rawmaterials_pb2, rawmaterials_pb2_grpc
import products_pb2, products_pb2_grpc
import plantfloorsection_pb2, plantfloorsection_pb2_grpc
import checkpoint_pb2, checkpoint_pb2_grpc
import container_localization_pb2 as cl_pb2
import container_localization_pb2_grpc as cl_rpc
import manufacturing_pb2 as m_pb2
import manufacturing_pb2_grpc as m_rpc
import manufacturing
import prediction_pb2, prediction_pb2_grpc


from prediction_client import trigger_train, trigger_predict
from client import fetch_clients, fetch_client_by_id
from containers import fetch_containers
from rawmaterials import fetch_rawmaterials
from products import fetch_products, fetch_product_by_id
from plantfloorsection import fetch_plantfloorsections, fetch_plantfloorsection_by_id
from checkpoints import fetch_checkpoints, fetch_checkpoint_by_id, fetch_checkpoints_by_section
from container_localization import (
    fetch_container_localizations,
    fetch_localizations_by_section,
    fetch_localization_by_id,
    fetch_localizations_with_container_names_by_section,
    fetch_sections_for_container,
    fetch_sections,
)
from utils import format_clients_list, format_containers_list

load_dotenv()

class ClientService(clients_pb2_grpc.ClientServiceServicer):
    def ListClients(self, request, context):
        lista = fetch_clients()
        return clients_pb2.ClientList(
            clients=[
                clients_pb2.Client(
                    id=c.id,
                    name=c.name,
                    fiscalNumber=c.fiscalNumber
                ) for c in lista
            ]
        )

    def GetClientById(self, request, context):
        cliente = fetch_client_by_id(request.client_id)
        if cliente is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Cliente não encontrado")
        return clients_pb2.Client(
            id=cliente.id,
            name=cliente.name,
            fiscalNumber=cliente.fiscalNumber
        )

class ContainerService(containers_pb2_grpc.ContainerServiceServicer):
    def ListContainers(self, request, context):
        lista = fetch_containers()
        return containers_pb2.ContainerList(
            containers=[
                containers_pb2.Container(
                    containerId=c.containerId,
                    id=containers_pb2.NameField(name=c.id.name),
                    containerName=containers_pb2.NameField(name=c.containerName.name),
                    containerVolume=c.containerVolume,
                    activate=c.activate
                )
                for c in lista
            ]
        )

class RawMaterialService(rawmaterials_pb2_grpc.RawMaterialServiceServicer):
    def ListRawMaterials(self, request, context):
        lista = fetch_rawmaterials()
        return rawmaterials_pb2.RawMaterialList(
            rawmaterials=[
                rawmaterials_pb2.RawMaterial(
                    rawId=r.rawId,
                    name=rawmaterials_pb2.RawNameField(name=r.name),
                    info=rawmaterials_pb2.RawNameField(name=r.info)
                )
                for r in lista
            ]
        )

class ProductService(products_pb2_grpc.ProductServiceServicer):
    def ListProducts(self, request, context):
        lista = fetch_products()
        return products_pb2.ProductList(
            products=[
                products_pb2.Product(
                    id=p.id,
                    name=products_pb2.ProductsField(name=p.name),
                    info=products_pb2.ProductsField(name=p.info),
                    productId=products_pb2.ProductsField(name=p.productId)
                )
                for p in lista
            ]
        )

    def GetProductById(self, request, context):
        produto = fetch_product_by_id(request.ProductId)
        if produto is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Produto não encontrado")
        return products_pb2.Product(
            id=produto.id,
            name=products_pb2.ProductsField(name=produto.name),
            info=products_pb2.ProductsField(name=produto.info),
            productId=products_pb2.ProductsField(name=produto.productId)
        )

class PlantFloorSectionService(plantfloorsection_pb2_grpc.PlantFloorSectionServiceServicer):
    def ListPlantFloorSections(self, request, context):
        lista = fetch_plantfloorsections()
        return plantfloorsection_pb2.PlantFloorSectionList(
            sections=[
                plantfloorsection_pb2.PlantFloorSection(
                    sectionId=s.sectionId,
                    id=plantfloorsection_pb2.PlantFloorSectionField(name=s.id.name),
                    name=plantfloorsection_pb2.PlantFloorSectionField(name=s.name.name)
                )
                for s in lista
            ]
        )

    def GetPlantFloorSectionById(self, request, context):
        sec = fetch_plantfloorsection_by_id(request.sectionId)
        if sec is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Seção não encontrada")
        return plantfloorsection_pb2.PlantFloorSection(
            sectionId=sec.sectionId,
            id=plantfloorsection_pb2.PlantFloorSectionField(name=sec.id.name),
            name=plantfloorsection_pb2.PlantFloorSectionField(name=sec.name.name)
        )

class CheckpointService(checkpoint_pb2_grpc.CheckpointServiceServicer):
    def ListCheckpoints(self, request, context):
        lista = fetch_checkpoints()
        return checkpoint_pb2.CheckpointList(
            checkpoints=[
                checkpoint_pb2.Checkpoint(
                    checkpointId=c.checkpointId,
                    id=c.id,
                    name=checkpoint_pb2.CheckpointField(name=c.name.name),
                    status=c.status,
                    sectionId=c.sectionId
                )
                for c in lista
            ]
        )

    def GetCheckpointById(self, request, context):
        cp = fetch_checkpoint_by_id(request.checkpointId)
        if cp is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Checkpoint não encontrado")
        return checkpoint_pb2.Checkpoint(
            checkpointId=cp.checkpointId,
            id=cp.id,
            name=checkpoint_pb2.CheckpointField(name=cp.name.name),
            status=cp.status,
            sectionId=cp.sectionId
        )

    def ListCheckpointsBySection(self, request, context):
        lista = fetch_checkpoints_by_section(request.sectionId)
        return checkpoint_pb2.CheckpointList(
            checkpoints=[
                checkpoint_pb2.Checkpoint(
                    checkpointId=c.checkpointId,
                    id=c.id,
                    name=checkpoint_pb2.CheckpointField(name=c.name.name),
                    status=c.status,
                    sectionId=c.sectionId
                )
                for c in lista
            ]
        )

class ContainerLocalizationService(cl_rpc.ContainerLocalizationServiceServicer):
    def _get_name_mappings(self):
        containers = fetch_containers()
        secoes = fetch_sections()

        def get_name(field):
            if hasattr(field, "name"):
                return field.name
            if isinstance(field, dict):
                return field.get("name", "<desconhecido>")
            if isinstance(field, str):
                return field
            return "<desconhecido>"

        mapa_containers = {c.containerId: get_name(c.containerName) for c in containers}
        mapa_secoes = {s.sectionId: get_name(s.name) for s in secoes}
        return mapa_containers, mapa_secoes

    def ListContainerLocalizations(self, request, context):
        itens = fetch_container_localizations()
        mapa_containers, mapa_secoes = self._get_name_mappings()
        return cl_pb2.ContainerLocalizationList(
            items=[
                cl_pb2.ContainerLocalization(
                    id=loc.id,
                    containerId=loc.containerId,
                    sectionId=loc.sectionId,
                    datetime=loc.datetime,
                    containerName=mapa_containers.get(loc.containerId, f"id {loc.containerId} desconhecido"),
                    sectionName=mapa_secoes.get(loc.sectionId, f"id {loc.sectionId} desconhecido")
                )
                for loc in itens
            ]
        )

    def ListBySection(self, request, context):
        itens = fetch_localizations_by_section(request.sectionId)
        mapa_containers, mapa_secoes = self._get_name_mappings()
        return cl_pb2.ContainerLocalizationList(
            items=[
                cl_pb2.ContainerLocalization(
                    id=loc.id,
                    containerId=loc.containerId,
                    sectionId=loc.sectionId,
                    datetime=loc.datetime,
                    containerName=mapa_containers.get(loc.containerId, f"id {loc.containerId} desconhecido"),
                    sectionName=mapa_secoes.get(loc.sectionId, f"id {loc.sectionId} desconhecido")
                )
                for loc in itens
            ]
        )

    def GetById(self, request, context):
        loc = fetch_localization_by_id(request.id)
        if loc is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Localização não encontrada")
        mapa_containers, mapa_secoes = self._get_name_mappings()
        return cl_pb2.ContainerLocalization(
            id=loc.id,
            containerId=loc.containerId,
            sectionId=loc.sectionId,
            datetime=loc.datetime,
            containerName=mapa_containers.get(loc.containerId, f"id {loc.containerId} desconhecido"),
            sectionName=mapa_secoes.get(loc.sectionId, f"id {loc.sectionId} desconhecido")
        )

    def ListWithContainerNamesBySection(self, request, context):
        enriquecidos = fetch_localizations_with_container_names_by_section(request.sectionId)
        mapa_containers, mapa_secoes = self._get_name_mappings()
        return cl_pb2.ContainerLocalizationList(
            items=[
                cl_pb2.ContainerLocalization(
                    id=item["id"],
                    containerId=item["containerId"],
                    sectionId=request.sectionId,
                    datetime=item["datetime"],
                    containerName=item["containerName"],
                    sectionName=mapa_secoes.get(request.sectionId, f"id {request.sectionId} desconhecido")
                )
                for item in enriquecidos
            ]
        )

    def ListSectionsByContainer(self, request, context):
        enriquecidos = fetch_sections_for_container(request.containerId)
        mapa_containers, _ = self._get_name_mappings()
        return cl_pb2.ContainerLocalizationList(
            items=[
                cl_pb2.ContainerLocalization(
                    id=item["id"],
                    containerId=request.containerId,
                    sectionId=item["sectionId"],
                    datetime=item["datetime"],
                    containerName=mapa_containers.get(request.containerId, f"id {request.containerId} desconhecido"),
                    sectionName=item["sectionName"]
                )
                for item in enriquecidos
            ]
        )

    def ListBySectionAtTime(self, request, context):
        locs = fetch_localizations_by_section(request.sectionId)
        filtradas = [loc for loc in locs if loc.datetime.startswith(request.datetime)]
        mapa_containers, mapa_secoes = self._get_name_mappings()
        return cl_pb2.ContainerLocalizationList(
            items=[
                cl_pb2.ContainerLocalization(
                    id=loc.id,
                    containerId=loc.containerId,
                    sectionId=loc.sectionId,
                    datetime=loc.datetime,
                    containerName=mapa_containers.get(loc.containerId, f"id {loc.containerId} desconhecido"),
                    sectionName=mapa_secoes.get(loc.sectionId, f"id {loc.sectionId} desconhecido")
                )
                for loc in filtradas
            ]
        )

class ManufacturingOrderService(m_rpc.ManufacturingOrderServiceServicer):
    def List(self, request, context):
        pedidos = manufacturing.fetch_manufacturing_orders()
        lista = []
        for o in pedidos:
            cliente = fetch_client_by_id(o.clientId)
            processo = manufacturing.fetch_manufacturing_process_by_id(o.manufacturingProcessId)
            produto = fetch_product_by_id(o.productLotId)
            lista.append(
                m_pb2.ManufacturingOrder(
                    id=o.id,
                    orderNumber=o.orderNumber,
                    sheduleInit=o.sheduleInit,
                    observations=o.observations,
                    manufacturingOrderId=o.manufacturingOrderId,
                    clientId=o.clientId,
                    manufacturingProcessId=o.manufacturingProcessId,
                    productLotId=o.productLotId,
                    clientName=cliente.name if cliente else "",
                    manufacturingProcessName=processo.processName if processo else "",
                    productLotName=produto.name if produto else ""
                )
            )
        return m_pb2.ManufacturingOrderList(items=lista)

    def GetById(self, request, context):
        pedido = next((x for x in manufacturing.fetch_manufacturing_orders() if x.id == request.orderId), None)
        if pedido is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Ordem não encontrada")
        cliente = fetch_client_by_id(pedido.clientId)
        processo = manufacturing.fetch_manufacturing_process_by_id(pedido.manufacturingProcessId)
        produto = fetch_product_by_id(pedido.productLotId)
        return m_pb2.ManufacturingOrder(
            id=pedido.id,
            orderNumber=pedido.orderNumber,
            sheduleInit=pedido.sheduleInit,
            observations=pedido.observations,
            manufacturingOrderId=pedido.manufacturingOrderId,
            clientId=pedido.clientId,
            manufacturingProcessId=pedido.manufacturingProcessId,
            productLotId=pedido.productLotId,
            clientName=cliente.name if cliente else "",
            manufacturingProcessName=processo.processName if processo else "",
            productLotName=produto.name if produto else ""
        )

    def ListRawMaterialsByOrder(self, request, context):
        try:
            itens = manufacturing.fetch_items_of_raw_by_order(request.orderId)
            lista = []
            for i in itens:
                dados = {
                    "itemRawId": i.itemRawId,
                    "quantity": i.quantity,
                    "unit": i.unit,
                    "lotOfRawMaterialId": i.lotOfRawMaterialId,
                    "itemInContainerId": i.itemInContainerId,
                    "manufacturingOrderPhaseId": i.manufacturingOrderPhaseId,
                    "manufacturingOrderId": i.manufacturingOrderId
                }
                lote = manufacturing.fetch_lot_of_raw_material_by_id(i.lotOfRawMaterialId)
                if lote:
                    materia = manufacturing.fetch_raw_material_by_id(lote.rawMaterialId)
                    if materia:
                        dados.update({
                            "rawMaterialName": materia.name,
                            "rawMaterialInfo": materia.info,
                            "lotNumber": lote.lot_number_value,
                            "lotQuantity": lote.lotQuantity,
                            "lotUnit": lote.lotUnit
                        })
                lista.append(m_pb2.ItemOfRawMaterial(**dados))
            return m_pb2.ItemOfRawMaterialList(items=lista)
        except Exception as e:
            context.abort(grpc.StatusCode.INTERNAL, f"Erro interno: {e}")

    def ListPhasesByOrder(self, request, context):
        fases = manufacturing.fetch_phases_by_order(request.orderId)
        return m_pb2.ManufacturingOrderPhaseList(
            items=[
                m_pb2.ManufacturingOrderPhase(
                    id=p.id,
                    manufacturingOrderId=p.manufacturingOrderId,
                    manufacturingPhaseId=p.manufacturingPhaseId,
                    quantity=p.quantity,
                    sheduleInit=p.sheduleInit,
                    dateTimeInit=p.dateTimeInit,
                    dateTimeEnd=p.dateTimeEnd
                )
                for p in fases
            ]
        )

    def ListHistoryByOrder(self, request, context):
        historico = manufacturing.fetch_history_by_order(request.orderId)
        return m_pb2.ManufacturingOrderHistoryList(
            items=[
                m_pb2.ManufacturingOrderHistory(
                    manufacturingOrderId=h.manufacturingOrderId,
                    plantFloorSectionId=h.plantFloorSectionId,
                    dateTime=h.dateTime,
                    statusName=h.statusName
                )
                for h in historico
            ]
        )

class ItemLocalizationService(m_rpc.ItemLocalizationServiceServicer):
    def List(self, request, context):
        itens = manufacturing.fetch_item_localizations()
        return m_pb2.ItemLocalizationList(
            items=[
                m_pb2.ItemLocalization(
                    itemLocalizationId=i.itemLocalizationId,
                    itemRawId=i.itemRawId,
                    containerLocalizationId=i.containerLocalizationId,
                    dateTime=i.dateTime
                )
                for i in itens
            ]
        )

    def GetById(self, request, context):
        item = next((x for x in manufacturing.fetch_item_localizations()
                     if x.itemLocalizationId == request.itemLocalizationId), None)
        if item is None:
            context.abort(grpc.StatusCode.NOT_FOUND, "Localização de item não encontrada")
        return m_pb2.ItemLocalization(
            itemLocalizationId=item.itemLocalizationId,
            itemRawId=item.itemRawId,
            containerLocalizationId=item.containerLocalizationId,
            dateTime=item.dateTime
        )

    def ListByOrderItems(self, request, context):
        locais = manufacturing.fetch_item_localizations_by_order(request.orderId)
        return m_pb2.ItemLocalizationList(
            items=[
                m_pb2.ItemLocalization(
                    itemLocalizationId=l.itemLocalizationId,
                    itemRawId=l.itemRawId,
                    containerLocalizationId=l.containerLocalizationId,
                    dateTime=l.dateTime
                )
                for l in locais
            ]
        )


class PredictionService(prediction_pb2_grpc.MlpPredictionServiceServicer):

    def Train(self, request, context):
        try:
            resultado = trigger_train()
            return prediction_pb2.TrainResponse(
                message=resultado["message"],
                version=resultado["version"],
                trainedUntil=resultado["trainedUntil"]
            )
        except Exception as e:
            context.abort(grpc.StatusCode.INTERNAL, f"Erro ao treinar: {e}")

    def Predict(self, request, context):
        try:
            features = [{
                "Quantity": f.Quantity,
                "PhaseId": f.PhaseId,
                "SectionId": f.SectionId,
                "ProductId": f.ProductId,
                "ClientId": f.ClientId,
                "LotQty": f.LotQty,
                "Hour": f.Hour,
                "WeekDay": f.WeekDay
            } for f in request.features]

            resultado = trigger_predict(features)

            if not isinstance(resultado, list):
                context.abort(grpc.StatusCode.INTERNAL, "Resposta inválida do serviço Prediction")

            return prediction_pb2.PredictResponse(predictions=resultado)

        except Exception as e:
            context.abort(grpc.StatusCode.INTERNAL, f"Erro ao prever: {e}")


def serve():
    porta = os.getenv("GRPC_PORT", "50051")
    servidor = grpc.server(futures.ThreadPoolExecutor(max_workers=4))

    clients_pb2_grpc.add_ClientServiceServicer_to_server(ClientService(), servidor)
    containers_pb2_grpc.add_ContainerServiceServicer_to_server(ContainerService(), servidor)
    rawmaterials_pb2_grpc.add_RawMaterialServiceServicer_to_server(RawMaterialService(), servidor)
    products_pb2_grpc.add_ProductServiceServicer_to_server(ProductService(), servidor)
    plantfloorsection_pb2_grpc.add_PlantFloorSectionServiceServicer_to_server(PlantFloorSectionService(), servidor)
    checkpoint_pb2_grpc.add_CheckpointServiceServicer_to_server(CheckpointService(), servidor)
    cl_rpc.add_ContainerLocalizationServiceServicer_to_server(ContainerLocalizationService(), servidor)
    m_rpc.add_ManufacturingOrderServiceServicer_to_server(ManufacturingOrderService(), servidor)
    m_rpc.add_ItemLocalizationServiceServicer_to_server(ItemLocalizationService(), servidor)
    prediction_pb2_grpc.add_MlpPredictionServiceServicer_to_server(PredictionService(), servidor)

    nomes_servicos = (
        clients_pb2.DESCRIPTOR.services_by_name['ClientService'].full_name,
        containers_pb2.DESCRIPTOR.services_by_name['ContainerService'].full_name,
        rawmaterials_pb2.DESCRIPTOR.services_by_name['RawMaterialService'].full_name,
        products_pb2.DESCRIPTOR.services_by_name['ProductService'].full_name,
        plantfloorsection_pb2.DESCRIPTOR.services_by_name['PlantFloorSectionService'].full_name,
        checkpoint_pb2.DESCRIPTOR.services_by_name['CheckpointService'].full_name,
        cl_pb2.DESCRIPTOR.services_by_name['ContainerLocalizationService'].full_name,
        m_pb2.DESCRIPTOR.services_by_name['ManufacturingOrderService'].full_name,
        m_pb2.DESCRIPTOR.services_by_name['ItemLocalizationService'].full_name,
        prediction_pb2.DESCRIPTOR.services_by_name['MlpPredictionService'].full_name,
        reflection.SERVICE_NAME,
    )

    reflection.enable_server_reflection(nomes_servicos, servidor)

    servidor.add_insecure_port(f"[::]:{porta}")
    servidor.start()
    print(f"Servidor gRPC rodando na porta {porta}")
    servidor.wait_for_termination()

if __name__ == "__main__":
    serve()
