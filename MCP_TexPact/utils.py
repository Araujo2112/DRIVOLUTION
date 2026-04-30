# utils.py
from typing import List
from client import ClientModel
from containers import ContainersModel


##clients
def format_clients_list(clients: List[ClientModel]) -> str:
    lines = [f"ID: {c.id}, Nome: {c.name}, Fiscal: {c.fiscalNumber}" for c in clients]
    return "\n".join(lines)

def format_client(client: ClientModel) -> str:
    return f"ID: {client.id}, Nome: {client.name}, Fiscal: {client.fiscalNumber}"

##containers

def format_containers_list(containers: List[ContainersModel]) -> str:
    lines = [f"ID: {co.containerId}, FIWARE_ID: {co.urn}, ContainerName: {co.containerName}, ContainerVolume: {co.containerVolume}, Status: {co.activate}" for co in containers]
    return "\n".join(lines)

