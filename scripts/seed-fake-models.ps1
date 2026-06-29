# seed-fake-models.ps1
# Gera modelos de carro fictícios + configurações + opções via API, para popular a BD de teste.
# Sem versões, packs ou preços comerciais — apenas o que é relevante para PRODUÇÃO.
# Corre com: .\seed-fake-models.ps1

$baseUrl = "http://localhost:8080/api"

$loginBody = @{ email = "admin@drivolution.pt"; password = "12345678" } | ConvertTo-Json
$loginRes = Invoke-RestMethod -Uri "$baseUrl/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$headers = @{ Authorization = "Bearer $($loginRes.token)" }

function New-CarModel($name, $version, $type) {
    $body = @{ name = $name; version = $version; type = $type } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/CarModel" -Method Post -Body $body -ContentType "application/json" -Headers $headers
    Write-Host "Modelo criado: $($res.name) (id=$($res.id))" -ForegroundColor Green
    return $res.id
}

function New-Config($modelId, $item, $allowMultiple) {
    $body = @{ modelId = $modelId; item = $item; allowMultiple = $allowMultiple } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/Config" -Method Post -Body $body -ContentType "application/json" -Headers $headers
    Write-Host "  Config criada: $($res.item) (id=$($res.id), allowMultiple=$allowMultiple)" -ForegroundColor Cyan
    return $res.id
}

function New-ConfigOption($configId, $value, $isDefault) {
    $body = @{ configId = $configId; value = $value; isDefault = $isDefault } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/ConfigOption" -Method Post -Body $body -ContentType "application/json" -Headers $headers
    Write-Host "    Opção criada: $($res.value)$(if ($isDefault) {' [default]'})" -ForegroundColor DarkGray
    return $res.id
}

# ── Definição dos modelos fictícios ─────────────────────────────────────────
# Marcas e modelos inventados, sem qualquer relação com fabricantes reais.
# Sem preços, versões comerciais ou packs — só o que decide o que é montado na linha.

$models = @(

    # Modelo 1: básico, poucas configs — bom para testar o caso "mínimo"
    @{
        name = "Bravon Halo"; version = "2026"; type = "Hatchback"
        configs = @(
            @{ item = "Cor"; allowMultiple = $false; options = @(
                @{ value = "Amarelo Solar"; default = $false },
                @{ value = "Branco Pérola"; default = $true }
            )},
            @{ item = "Motorização"; allowMultiple = $false; options = @(
                @{ value = "Elétrico 110kW"; default = $true },
                @{ value = "Gasolina 1.2"; default = $false }
            )},
            @{ item = "Acessórios"; allowMultiple = $true; options = @(
                @{ value = "Carregador wireless"; default = $false },
                @{ value = "Frisos laterais"; default = $false }
            )}
        )
    },

    # Modelo 2: intermédio — sedan com motorização, cor, jantes, estofos
    @{
        name = "Velora Astra"; version = "2026"; type = "Sedan"
        configs = @(
            @{ item = "Motorização"; allowMultiple = $false; options = @(
                @{ value = "1.6 Turbo Gasolina"; default = $true },
                @{ value = "2.0 Híbrido"; default = $false },
                @{ value = "1.6 Diesel"; default = $false }
            )},
            @{ item = "Cor"; allowMultiple = $false; options = @(
                @{ value = "Branco Glaciar"; default = $true },
                @{ value = "Azul Cobalto"; default = $false },
                @{ value = "Cinza Grafite"; default = $false }
            )},
            @{ item = "Jantes"; allowMultiple = $false; options = @(
                @{ value = "16 polegadas Standard"; default = $true },
                @{ value = "18 polegadas Desportivas"; default = $false }
            )},
            @{ item = "Estofos"; allowMultiple = $false; options = @(
                @{ value = "Tecido Cinza"; default = $true },
                @{ value = "Couro Preto"; default = $false }
            )},
            @{ item = "Acessórios"; allowMultiple = $true; options = @(
                @{ value = "Tow bar"; default = $false },
                @{ value = "Teto de abrir"; default = $false },
                @{ value = "Sensores de estacionamento"; default = $false }
            )}
        )
    },

    # Modelo 3: completo — SUV com tração, mais opções de motorização e acessórios
    @{
        name = "Quintex Fuso"; version = "2025"; type = "SUV"
        configs = @(
            @{ item = "Motorização"; allowMultiple = $false; options = @(
                @{ value = "2.2 Diesel"; default = $true },
                @{ value = "3.0 V6 Gasolina"; default = $false },
                @{ value = "Híbrido Plug-in 2.0"; default = $false }
            )},
            @{ item = "Cor"; allowMultiple = $false; options = @(
                @{ value = "Verde Floresta"; default = $false },
                @{ value = "Preto Onyx"; default = $true },
                @{ value = "Vermelho Vulcão"; default = $false },
                @{ value = "Cinza Tempestade"; default = $false }
            )},
            @{ item = "Jantes"; allowMultiple = $false; options = @(
                @{ value = "17 polegadas Standard"; default = $true },
                @{ value = "19 polegadas Off-Road"; default = $false },
                @{ value = "20 polegadas Desportivas"; default = $false }
            )},
            @{ item = "Tração"; allowMultiple = $false; options = @(
                @{ value = "Dianteira"; default = $true },
                @{ value = "Integral 4x4"; default = $false }
            )},
            @{ item = "Estofos"; allowMultiple = $false; options = @(
                @{ value = "Tecido Standard"; default = $true },
                @{ value = "Couro Castanho"; default = $false },
                @{ value = "Couro Preto Premium"; default = $false }
            )},
            @{ item = "Acessórios"; allowMultiple = $true; options = @(
                @{ value = "Barras de tejadilho"; default = $false },
                @{ value = "Câmara 360º"; default = $false },
                @{ value = "Engate de reboque"; default = $false },
                @{ value = "Proteção de cárter"; default = $false }
            )}
        )
    },

    # Modelo 4: elétrico — testar caso sem motorização variável, mas com carregamento
    @{
        name = "Nordia Volt"; version = "2026"; type = "Citadino Elétrico"
        configs = @(
            @{ item = "Cor"; allowMultiple = $false; options = @(
                @{ value = "Azul Elétrico"; default = $true },
                @{ value = "Cinza Tecnológico"; default = $false }
            )},
            @{ item = "Bateria"; allowMultiple = $false; options = @(
                @{ value = "Standard 45 kWh"; default = $true },
                @{ value = "Longo Alcance 65 kWh"; default = $false }
            )},
            @{ item = "Jantes"; allowMultiple = $false; options = @(
                @{ value = "16 polegadas Aero"; default = $true },
                @{ value = "18 polegadas Desportivas"; default = $false }
            )},
            @{ item = "Acessórios"; allowMultiple = $true; options = @(
                @{ value = "Carregador rápido DC"; default = $false },
                @{ value = "Bomba de calor"; default = $false },
                @{ value = "Câmara de marcha-atrás"; default = $false }
            )}
        )
    }
)

# ── Execução ─────────────────────────────────────────────────────────────────
foreach ($model in $models) {
    Write-Host "`n=== Criando modelo: $($model.name) ===" -ForegroundColor Yellow
    $modelId = New-CarModel $model.name $model.version $model.type

    foreach ($config in $model.configs) {
        $configId = New-Config $modelId $config.item $config.allowMultiple
        foreach ($option in $config.options) {
            New-ConfigOption $configId $option.value $option.default
        }
    }
}

Write-Host "`nConcluído. $($models.Count) modelos criados com configs e opções." -ForegroundColor Green