import os
import requests
from typing import List, Optional
from pydantic import BaseModel, ValidationError
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[PRODUCTS] API_BASE = {API_BASE}")

class ProductsModel(BaseModel):
    id: int
    name: str
    info: str
    productId: str

def fetch_products() -> List[ProductsModel]:
    url = f"{API_BASE}/Product"
    resp = requests.get(url)
    resp.raise_for_status()
    data = resp.json()
    products: List[ProductsModel] = []
    for item in data:
        try:
            products.append(ProductsModel(**item))
        except ValidationError as e:
            print(f"[fetch_products] Validation failed for {item}: {e}")
    return products

def fetch_product_by_id(product_id: int) -> Optional[ProductsModel]:
    url = f"{API_BASE}/Product/{product_id}"
    resp = requests.get(url)
    if resp.status_code == 404:
        return None
    resp.raise_for_status()
    try:
        return ProductsModel(**resp.json())
    except ValidationError as e:
        print(f"[fetch_product_by_id] Validation failed for product_id {product_id}: {e}")
        return None
