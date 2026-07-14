# DRIVOLUTION — Scripts de Dados e Ferramentas

Esta pasta contém os scripts de apoio ao desenvolvimento e demo do projeto. **Não fazem parte do sistema em produção** — servem para criar a infraestrutura, popular a base de dados, treinar o modelo ML e limpar dados de teste.

---

## Pré-requisitos gerais

O Docker tem de estar a correr antes de qualquer script:

```bash
docker compose up -d
```

Para os scripts Python, instalar as dependências uma vez:

```bash
pip install psycopg2-binary scikit-learn requests --break-system-packages
python -m pip install bcrypt
```

---

## Ordem recomendada para setup completo (BD limpa)

```
0. docker compose up -d
1. python scripts/create_admin.py         
2. python scripts/setup_environment.py
3. .\scripts\seed-fake-models.ps1
4. python scripts/seed_synthetic_history.py
5. curl -X POST http://localhost:8080/api/ml/retrain
6. python scripts/seed_wip_state.py
7. python agent/drivolution_agent.py --auto
```

---

## 0. Limpar a BD (recomeçar do zero)

Quando precisares de apagar tudo e começar de novo:

```bash
docker exec -i drivolution-db psql -U drivolution -d drivolution -c "
TRUNCATE TABLE phase_time_coefficient, alert, product_phase, localization_history,
  supported_product, product_config, product, manufacturing_order, client_order,
  phase_sequence, config_option, config, model, workstation, support,
  production_line, manufacturing_phase RESTART IDENTITY CASCADE;
"
```

---

## 1. `setup_environment.py` — Criar toda a infraestrutura do zero ⭐

**O que faz:** Cria via API as fases de fabrico, linhas de produção, workstations e suportes (skids) com os mesmos dados do ambiente de referência. No final regista tudo no FIWARE (IoT Agent, Orion, subscrições).

Este script **substitui** o `fiware_setup.py` para uma instalação nova — faz o mesmo que ele mas também cria a infra na BD antes.

```bash
python scripts/setup_environment.py
```

Se a BD já tiver dados, o script avisa e pergunta antes de continuar.

---

## 2. `seed-fake-models.ps1` — Criar modelos de carro e configurações

**O que faz:** Cria 4 modelos de carro fictícios (Bravon Halo, Velora Astra, etc.) com todas as configurações (cor, motorização, jantes, acessórios) e opções via API REST.

**Como correr (Windows — PowerShell):**
```powershell
.\scripts\seed-fake-models.ps1
```

**Mac/Linux:** instalar PowerShell com `brew install powershell` (Mac) ou `snap install powershell` (Linux), depois:
```bash
pwsh scripts/seed-fake-models.ps1
```

> Alternativa sem PowerShell: criar os modelos manualmente no dashboard em **Configuração → Modelos de Carro**.

---

## 3. `seed_synthetic_history.py` — Gerar histórico para treinar o modelo ML

**O que faz:** Cria 200 produtos históricos já concluídos com tempos realistas por fase, modelo e configuração. É este histórico que o modelo Ridge usa para aprender. Usa `time-deltas-by-option-phase.json` como base dos deltas.

```bash
python scripts/seed_synthetic_history.py
```

Depois treinar o modelo:
```bash
# Pelo endpoint da API (recomendado para a demo)
curl -X POST http://localhost:8080/api/ml/retrain
```

---

## 4. `seed_wip_state.py` — Popular o WIP Dashboard com estado ao vivo

**O que faz:** Cria produtos em diferentes estados para demonstrar o WIP Dashboard e os endpoints de ETA:
- 5 produtos sem skid → fila de espera (motivo: "Sem skid")
- 2 produtos com skid mas sem linha → fila de espera ("À espera de linha")
- 6 produtos em curso (normal, warning, critical com alerta Andon)
- 2 produtos completados → KPI "Concluídos"

```bash
python scripts/seed_wip_state.py
```

---

## 5. `cleanup_wip_test.sql` — Apagar dados do seed WIP

**O que faz:** Apaga apenas o que o `seed_wip_state.py` criou (encomenda `ORD-WIP-TEST`). Não toca no histórico ML nem noutros dados.

```bash
# Pelo container do PostgreSQL (não precisa de cliente psql instalado)
docker exec -i drivolution-db psql -U drivolution -d drivolution < scripts/cleanup_wip_test.sql
```

---

## 6. `time-deltas-by-option-phase.json`

Ficheiro de configuração usado internamente pelo `seed_synthetic_history.py`. Não é para correr diretamente.

---

## Pasta `agent/` — Integração FIWARE

### `agent/fiware_setup.py` — Re-registar FIWARE (sem criar infra)

Usa-se quando a infra já existe na BD mas o FIWARE foi reiniciado e perdeu o estado (devices, entidades, subscrições). Lê os suportes existentes da API e re-regista tudo — **skids e crachás (Badge)**.

```bash
python agent/fiware_setup.py
```

Além dos skids, o script agora também:
- Carrega utilizadores com role `operator`/`manager` (precisa de conta `admin` no `.env`).
- Cria um service group `Badge` no IoT Agent (apikey `drivolution-badge-key`).
- Regista um device `badge-{userId}` por utilizador e a respetiva entidade `Badge` no Orion (atributo estático `appUserId`).
- Cria uma 3ª subscrição: `Badge.currentWorkstation` → `FiwareNotificationController`.

Se não houver utilizadores `operator`/`manager`, o passo dos crachás é ignorado sem falhar o resto do script.

### `agent/drivolution_agent.py` — Simular leituras RFID (skids)

```bash
# Modo automático — percorre workstations aleatoriamente em loop
python agent/drivolution_agent.py --auto

# Modo manual — envia uma leitura específica
python agent/drivolution_agent.py --tag 3542100258 --ws 1
```

### `agent/drivolution_badge_agent.py` — Simular leituras de crachá (presença)

Simula um operador a "passar o cartão" num leitor associado a uma workstation Humana/Híbrida. Mesmo padrão do `drivolution_agent.py`: envia para o IoT Agent → Orion → notifica a API, que faz check-in/check-out em `WorkstationPresence` **sem precisar de sessão/JWT do operador**.

Toggle: ler o crachá na mesma workstation onde já está presente faz check-out; numa workstation diferente faz check-out automático da anterior (se existir) e check-in na nova.

```bash
# Modo automático — 1 operador faz check-in e check-out na 1ª workstation elegível
python agent/drivolution_badge_agent.py --auto

# Modo manual — leitura específica (user id + workstation id)
python agent/drivolution_badge_agent.py --user 3 --ws 1

# Modo interativo
python agent/drivolution_badge_agent.py
```


### `agent/fiware_diagnose.py` — Diagnosticar estado do FIWARE

```bash
python agent/fiware_diagnose.py
```