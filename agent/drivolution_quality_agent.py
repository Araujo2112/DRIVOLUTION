import argparse
import os
import random
import time
import requests
from dotenv import load_dotenv
load_dotenv()

API_BASE_URL = os.getenv("DRIVOLUTION_API_URL", "http://localhost:8080/api")

SEVERITIES = ["none", "minor", "major", "critical"]

_cached_token = None


def normalize_list(data):
    if isinstance(data, dict) and "$values" in data:
        return data["$values"]
    if isinstance(data, list):
        return data
    return []


def extract_token(data):
    if not isinstance(data, dict):
        return None

    possible_keys = [
        "token",
        "accessToken",
        "jwt",
        "jwtToken",
        "bearerToken",
    ]

    for key in possible_keys:
        value = data.get(key)
        if isinstance(value, str) and value:
            return value

    for value in data.values():
        if isinstance(value, str) and value.count(".") == 2:
            return value

    return None


def login():
    global _cached_token

    if _cached_token:
        return _cached_token

    manual_token = os.getenv("DRIVOLUTION_TOKEN")
    if manual_token:
        _cached_token = manual_token.replace("Bearer ", "").strip()
        return _cached_token

    email = os.getenv("DRIVOLUTION_EMAIL")
    password = os.getenv("DRIVOLUTION_PASSWORD")

    if not email or not password:
        print("   ⚠ Sem autenticação.")
        print("   Define DRIVOLUTION_EMAIL e DRIVOLUTION_PASSWORD ou DRIVOLUTION_TOKEN.")
        return None

    payload = {
        "email": email,
        "password": password
    }

    r = requests.post(
        f"{API_BASE_URL}/Auth/login",
        json=payload,
        timeout=10
    )

    if r.status_code not in (200, 201):
        print(f"   ✗ Login falhou ({r.status_code}): {r.text}")
        return None

    data = r.json()
    token = extract_token(data)

    if not token:
        print("   ✗ Login respondeu, mas não encontrei token.")
        print(f"   Resposta: {data}")
        return None

    _cached_token = token
    print("   ✓ Login efetuado com sucesso.")
    return _cached_token


def headers():
    token = login()

    h = {"Content-Type": "application/json"}

    if token:
        h["Authorization"] = f"Bearer {token}"

    return h


def get_wip_items():
    r = requests.get(
        f"{API_BASE_URL}/production-lines/wip",
        headers=headers(),
        timeout=10
    )

    if r.status_code == 401:
        raise Exception("401 Unauthorized em /production-lines/wip. Verifica email/password/token.")

    r.raise_for_status()
    data = r.json()
    return normalize_list(data.get("items", []))


def get_product_timeline(product_id: int):
    r = requests.get(
        f"{API_BASE_URL}/products/{product_id}/timeline",
        headers=headers(),
        timeout=10
    )

    if r.status_code == 401:
        raise Exception("401 Unauthorized em /products/{id}/timeline. Verifica email/password/token.")

    r.raise_for_status()
    return r.json()


def get_existing_quality_checks(product_id: int):
    r = requests.get(
        f"{API_BASE_URL}/QualityCheck/product/{product_id}",
        headers=headers(),
        timeout=10
    )

    if r.status_code == 404:
        return []

    if r.status_code == 401:
        raise Exception("401 Unauthorized em /QualityCheck/product/{id}. Verifica email/password/token.")

    r.raise_for_status()
    return normalize_list(r.json())


def get_current_phase_from_timeline(product_id: int):
    timeline = get_product_timeline(product_id)
    phases = normalize_list(timeline.get("phases", []))

    active = [p for p in phases if not p.get("endedAt")]

    if not active:
        return None

    return active[-1]


def create_quality_check(product_id: int, manufacturing_phase_id: int, severity: str, notes: str):
    payload = {
        "productId": product_id,
        "manufacturingPhaseId": manufacturing_phase_id,
        "notes": notes,
        "status": None,
        "severity": severity
    }

    r = requests.post(
        f"{API_BASE_URL}/QualityCheck",
        headers=headers(),
        json=payload,
        timeout=10
    )

    if r.status_code in (200, 201):
        return r.json()

    print(f"   ✗ Erro ao criar QualityCheck ({r.status_code}): {r.text}")
    return None


def already_checked(product_id: int, manufacturing_phase_id: int):
    checks = get_existing_quality_checks(product_id)

    for check in checks:
        if check.get("manufacturingPhaseId") == manufacturing_phase_id:
            return True

    return False


def simulate_quality_result():
    return random.choices(
        SEVERITIES,
        weights=[70, 18, 9, 3],
        k=1
    )[0]


def run_once():
    print("\n[Quality Agent] A procurar produtos em produção...")

    items = get_wip_items()

    if not items:
        print("   - Nenhum produto em produção.")
        return

    for item in items:
        product_id = item.get("productId")
        serial = item.get("serialNumber")
        phase_name = item.get("currentPhase") or ""

        if not product_id:
            continue

        current_phase = get_current_phase_from_timeline(product_id)

        if not current_phase:
            print(f"   - Produto {serial}: sem fase ativa na timeline.")
            continue

        manufacturing_phase_id = current_phase.get("manufacturingPhaseId")

        if not manufacturing_phase_id:
            print(f"   - Produto {serial}: manufacturingPhaseId não encontrado.")
            continue

        if already_checked(product_id, manufacturing_phase_id):
            print(f"   - Produto {serial}: QualityCheck já existe para esta fase.")
            continue

        severity = simulate_quality_result()

        print(f"   → Produto {serial} | Fase {phase_name} | Severidade simulada: {severity}")

        created = create_quality_check(
            product_id=product_id,
            manufacturing_phase_id=manufacturing_phase_id,
            severity=severity,
            notes=f"Quality Check automático gerado pelo agente de visão simulada. Severidade observada: {severity}"
        )

        if created:
            print(f"   ✓ QualityCheck criado | Status final: {created.get('status')} | Severity: {created.get('severity')}")


def main():
    parser = argparse.ArgumentParser(description="DRIVOLUTION - Agente de Visão Simulada para Quality Check")
    parser.add_argument("--once", action="store_true", help="Executa uma vez e termina.")
    parser.add_argument("--interval", type=int, default=10, help="Intervalo entre verificações em segundos.")
    args = parser.parse_args()

    print("=" * 60)
    print(" DRIVOLUTION - Quality Check Agent")
    print("=" * 60)
    print(f" API: {API_BASE_URL}")

    try:
        if args.once:
            run_once()
            return

        while True:
            run_once()
            time.sleep(args.interval)

    except requests.HTTPError as e:
        print(f"   ✗ Erro HTTP: {e}")
    except Exception as e:
        print(f"   ✗ Erro: {e}")


if __name__ == "__main__":
    main()