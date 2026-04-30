import os
import requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[PlantFloorSection] API_BASE = {API_BASE}")

class PlantFloorSectionField(BaseModel):
    name: str

class PlantFloorSectionModel(BaseModel):
    sectionId: int
    id: PlantFloorSectionField
    name: PlantFloorSectionField

def fetch_plantfloorsections() -> List[PlantFloorSectionModel]:
    url = f"{API_BASE}/PlantFloorSection"
    r = requests.get(url)
    r.raise_for_status()
    out: List[PlantFloorSectionModel] = []
    for item in r.json():
        if isinstance(item.get("name"), str):
            item["name"] = {"name": item["name"]}
        if isinstance(item.get("id"), str):
            item["id"] = {"name": item["id"]}
        try:
            out.append(PlantFloorSectionModel(**item))
        except ValidationError as e:
            print(f"[PlantFloorSection] validation failed for item {item}: {e}")
    return out


def fetch_plantfloorsection_by_id(section_id: int) -> Optional[PlantFloorSectionModel]:
    url = f"{API_BASE}/PlantFloorSection/{section_id}"
    r = requests.get(url)
    if r.status_code == 404:
        return None
    r.raise_for_status()
    item = r.json()
    if isinstance(item.get("name"), str):
        item["name"] = {"name": item["name"]}
    if isinstance(item.get("id"), str):
        item["id"] = {"name": item["id"]}
    try:
        return PlantFloorSectionModel(**item)
    except ValidationError as e:
        print(f"[PlantFloorSection] validation failed for section_id {section_id}: {e}")
        return None

