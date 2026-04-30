import json


def log_http_request(method: str = "POST", url: str = "", headers: dict = "", body: dict = ""):
    print("\n--- HTTP REQUEST ---")
    print(f"Method: {method}")
    print(f"URL: {url}")
    print("Headers:")
    for k, v in headers.items():
        print(f"  {k}: {v}")
    print("Body:")
    print(json.dumps(body, indent=2, ensure_ascii=False))
    print("---------------------\n")
