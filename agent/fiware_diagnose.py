"""
DRIVOLUTION - Diagnóstico FIWARE
Verifica o estado atual do IoT Agent e Orion.
"""

import requests
import json

ORION_URL     = "http://localhost:1026"
IOT_AGENT_URL = "http://localhost:4041"

HEADERS_JSON = {
    "FIWARE-Service":     "drivolution",
    "FIWARE-ServicePath": "/",
}


def check(label, url, headers=None):
    try:
        r = requests.get(url, headers=headers or {}, timeout=5)
        print(f"\n{'='*55}")
        print(f"  {label}")
        print(f"  Status: {r.status_code}")
        print(f"{'='*55}")
        try:
            data = r.json()
            print(json.dumps(data, indent=2, ensure_ascii=False)[:2000])
        except json.JSONDecodeError:
            print(r.text[:500])
    except Exception as e:
        print(f"\n✗ {label}: {e}")


if __name__ == "__main__":
    check("Orion - versão",          f"{ORION_URL}/version")
    check("IoT Agent - estado",      f"{IOT_AGENT_URL}/iot/about")
    check("IoT Agent - dispositivos",f"{IOT_AGENT_URL}/iot/devices",     HEADERS_JSON)
    check("IoT Agent - serviços",    f"{IOT_AGENT_URL}/iot/services",    HEADERS_JSON)
    check("Orion - entidades Skid",  f"{ORION_URL}/ngsi-ld/v1/entities?type=Skid&limit=10", HEADERS_JSON)
    check("Orion - subscrições",     f"{ORION_URL}/ngsi-ld/v1/subscriptions", HEADERS_JSON)