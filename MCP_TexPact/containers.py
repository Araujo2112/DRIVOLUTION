import os, requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[CONTAINERS] API_BASE = {API_BASE}")

class NameField(BaseModel):
    name: str

class ContainersModel(BaseModel):
    containerId: int
    id: NameField
    containerName: NameField
    containerVolume: int
    activate: bool

def fetch_containers() -> List[ContainersModel]:
    url = f"{API_BASE}/Container"
    r = requests.get(url)
    r.raise_for_status()
    out: List[ContainersModel] = []
    for item in r.json():
        try:
            out.append(ContainersModel(**item))
        except ValidationError as e:
            print(f"[CONTAINERS] validation failed for item {item}: {e}")

    return out

def fetch_container_by_id(container_id: int) -> Optional[ContainersModel]:

    url = f"{API_BASE}/Container/{container_id}"
    r = requests.get(url)
    if r.status_code == 404:
        return None
    r.raise_for_status()
    try:
        return ContainersModel(**r.json())
    except ValidationError as e:
        print(f"[CONTAINERS] validation failed for container_id {container_id}: {e}")
        return None
