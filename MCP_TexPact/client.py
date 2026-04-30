import os, requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[CLIENT] API_BASE = {API_BASE}")

class ClientModel(BaseModel):
    id: int
    name: str
    fiscalNumber: str

def fetch_clients() -> List[ClientModel]:
    url = f"{API_BASE}/Client"
    resp = requests.get(url)
    resp.raise_for_status()
    data = resp.json()
    clients: List[ClientModel] = []
    for item in data:
        try:
            clients.append(ClientModel(**item))
        except ValidationError as e:
            print(f"[fetch_clients] Validation failed for {item}: {e}")
    return clients

def fetch_client_by_id(client_id: int) -> Optional[ClientModel]:
    url = f"{API_BASE}/Client/{client_id}"
    resp = requests.get(url)
    if resp.status_code == 404:
        return None
    resp.raise_for_status()
    try:
        return ClientModel(**resp.json())
    except ValidationError as e:
        print(f"[fetch_client_by_id] Validation failed for client_id {client_id}: {e}")
        return None
