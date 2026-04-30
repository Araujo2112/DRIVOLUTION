import os
import requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[CHECKPOINT] API_BASE = {API_BASE}")


class NameField(BaseModel):
    name: str

class CheckpointModel(BaseModel):
    checkpointId: int
    id: str
    name: NameField
    status: bool
    sectionId: int


def fetch_checkpoints() -> list[CheckpointModel]:
    url = f"{API_BASE}/Checkpoint"
    resp = requests.get(url)
    resp.raise_for_status()
    data = resp.json()
    checkpoints = []
    for item in data:
        try:
            checkpoints.append(CheckpointModel(**item))
        except ValidationError as e:
            print(f"[fetch_checkpoints] Validation failed for {item}: {e}")
    return checkpoints


def fetch_checkpoint_by_id(checkpoint_id: int) -> Optional[CheckpointModel]:
    url = f"{API_BASE}/Checkpoint/{checkpoint_id}"
    resp = requests.get(url)
    if resp.status_code == 404:
        return None
    resp.raise_for_status()
    try:
        return CheckpointModel(**resp.json())
    except ValidationError as e:
        print(f"[fetch_checkpoint_by_id] Validation failed for checkpoint_id {checkpoint_id}: {e}")
        return None


def fetch_checkpoints_by_section(section_id: int) -> List[CheckpointModel]:
    all_checkpoints = fetch_checkpoints()
    return [cp for cp in all_checkpoints if cp.sectionId == section_id]
