"""
create_admin.py

Cria (ou repõe) o utilizador admin inicial diretamente na BD, com um hash
BCrypt real gerado em Python (compatível com BCrypt.Net-Next, que a API usa
em AuthService.Verify/HashPassword — ambos seguem a spec bcrypt $2a$/$2b$).

Resolve uma lacuna real do setup atual: nem o init-drivolution-schema.sql
nem o setup_environment.py criam o admin — o setup_environment.py só faz
login pressupondo que ele já existe. Sem este script, o passo é manual e
exige gerar o hash à parte (ver nota no ficheiro de contexto do projeto).

Idempotente: se o email já existir, atualiza a password/role/status em vez
de falhar (ON CONFLICT).

Correr ANTES de setup_environment.py, com a BD já criada e vazia:
    python scripts/create_admin.py

Dependência: pip install bcrypt psycopg2-binary --break-system-packages
"""

import sys
import psycopg2
import bcrypt

DB_CONFIG = {
    "host": "localhost",
    "port": 5433,
    "dbname": "drivolution",
    "user": "drivolution",
    "password": "drivolution",
}

ADMIN_NAME = "Administrador"
ADMIN_EMAIL = "admin@drivolution.pt"
ADMIN_PASSWORD = "12345678"  # dev/demo only — trocar em produção
ADMIN_ROLE = "admin"


def main():
    password_hash = bcrypt.hashpw(ADMIN_PASSWORD.encode("utf-8"), bcrypt.gensalt(rounds=11)).decode("utf-8")

    try:
        conn = psycopg2.connect(**DB_CONFIG)
    except psycopg2.OperationalError as e:
        print(f"Não consegui ligar à BD: {e}")
        print("Confirma que o docker compose está up (docker compose up -d) antes de correr este script.")
        sys.exit(1)

    try:
        with conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    INSERT INTO app_user (name, email, password_hash, role, status, must_change_password)
                    VALUES (%s, %s, %s, %s, 'active', false)
                    ON CONFLICT (email) DO UPDATE SET
                        password_hash        = EXCLUDED.password_hash,
                        role                 = EXCLUDED.role,
                        status               = 'active',
                        must_change_password = false
                    RETURNING id;
                    """,
                    (ADMIN_NAME, ADMIN_EMAIL, password_hash, ADMIN_ROLE),
                )
                user_id = cur.fetchone()[0]
    finally:
        conn.close()

    print(f"Admin pronto (id={user_id}).")
    print(f"  email:    {ADMIN_EMAIL}")
    print(f"  password: {ADMIN_PASSWORD}")


if __name__ == "__main__":
    main()
