import os
import requests
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.getenv("API_BASE_URL", "http://localhost:5181/api")
print(f"[PREDICTION] API_BASE = {API_BASE}")


def trigger_train():
    url = f"{API_BASE}/Prediction/train"
    try:
        response = requests.post(url, headers={"accept": "*/*"})
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"[trigger_train] Failed to contact {url}: {e}")
        return {
            "message": "Erro ao contactar o serviço Prediction",
            "version": -1,
            "trainedUntil": "N/A"
        }


def trigger_predict(features):
    url = f"{API_BASE}/Prediction/predict"
    payload = {
        "features": features
    }
    try:
        response = requests.post(url, json=payload, headers={"accept": "text/plain"})
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"[trigger_predict] Failed to contact {url}: {e}")
        return {
            "message": "Erro ao contactar o serviço Prediction",
            "prediction": None
        }
