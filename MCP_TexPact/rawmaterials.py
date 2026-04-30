import os, requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[RaWMATERIALS] API_BASE = {API_BASE}")


class RawMaterialsModel(BaseModel):
    rawId: int
    name: str
    info: str


def fetch_rawmaterials() -> List[RawMaterialsModel]:
    url = f"{API_BASE}/RawMaterial"
    r = requests.get(url)
    r.raise_for_status()
    out: List[RawMaterialsModel] = []
    for item in r.json():
        try:
            out.append(RawMaterialsModel(**item))
        except ValidationError as e:
            print(f"[RawMaterials] validation failed for item {item}: {e}")

    return out
