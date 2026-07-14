"""
train_phase_time_model.py

Treina modelos Ridge por fase de fabrico e guarda tudo num ficheiro pickle
para o microserviço ml-service usar no endpoint POST /predict.
"""

import os
import pickle
import psycopg2
from datetime import datetime
from pathlib import Path

import numpy as np
from sklearn.linear_model import RidgeCV
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler
from sklearn.model_selection import cross_val_score, KFold


# Configuração da ligação à base de dados.
# Os valores são obtidos através de variáveis de ambiente.
# Caso não existam, são usados os valores definidos por defeito.
DB_CONFIG = {
    "host": os.environ.get("DB_HOST", "localhost"),
    "port": int(os.environ.get("DB_PORT", "5433")),
    "dbname": os.environ.get("DB_NAME", "drivolution"),
    "user": os.environ.get("DB_USER", "drivolution"),
    "password": os.environ.get("DB_PASSWORD", "drivolution"),
}

# Caminho onde será guardado o modelo treinado.
# Pode ser alterado através da variável de ambiente MODEL_OUTPUT_PATH.
MODEL_OUTPUT_PATH = os.environ.get(
    "MODEL_OUTPUT_PATH",
    "ml-service/phase_time_model.pkl"
)

# Valores de alpha que o RidgeCV irá experimentar.
# O alpha controla a intensidade da regularização do modelo Ridge.
RIDGE_ALPHAS = np.logspace(-2, 3, 50)


# Função principal do processo de treino
def main():
    # Abre uma ligação à base de dados PostgreSQL
    conn = psycopg2.connect(**DB_CONFIG)

    # Cria um cursor para executar comandos SQL
    cur = conn.cursor()

    try:
        # Carrega todas as fases de fabrico
        phases = load_phases(cur)

        # Obtém as opções de configuração usadas como features
        # e as opções consideradas como referência/baseline
        feature_options, baseline_options = load_option_features(cur)

        # Obtém as linhas de produção usadas como features
        # e a linha considerada como baseline
        feature_lines, baseline_line = load_production_lines(cur)

        # Obtém os modelos de carro usados como features
        # e o modelo considerado como baseline
        feature_models, baseline_model = load_models_feature(cur)

        # Guarda a data e hora em que o treino foi realizado
        trained_at = datetime.now()

        # Dicionário onde serão guardados os modelos treinados por fase
        phase_models = {}

        # Percorre todas as fases de fabrico
        for phase_name, phase in phases.items():
            # Constrói o conjunto de dados da fase:
            # X contém as características
            # y contém a duração real
            # n contém o número de amostras
            X, y, n = build_dataset(
                cur,
                phase["id"],
                feature_options,
                feature_lines,
                feature_models
            )

            # Se existirem menos de 5 exemplos concluídos,
            # não existem dados suficientes para treinar esta fase
            if n < 5:
                print(
                    f"[{phase_name}] dados insuficientes "
                    f"({n} amostras), a saltar."
                )
                continue

            # Cria um pipeline com duas etapas:
            # 1. StandardScaler: normaliza as características
            # 2. RidgeCV: treina a regressão Ridge e escolhe o melhor alpha
            pipeline = Pipeline([
                ("scaler", StandardScaler()),
                ("ridge", RidgeCV(alphas=RIDGE_ALPHAS)),
            ])

            # Avalia o modelo através de validação cruzada
            cv_r2 = cross_validated_r2(pipeline, X, y)

            # Treina finalmente o modelo com todos os dados disponíveis
            pipeline.fit(X, y)

            # Guarda o modelo treinado desta fase
            phase_models[phase["id"]] = {
                "phase_name": phase_name,
                "pipeline": pipeline,
                "samples": n,
                "cv_r2": cv_r2,
            }

            # Mostra no terminal o resultado do treino
            print(
                f"[{phase_name}] {n} amostras | "
                f"R² validação cruzada = {cv_r2:.3f}"
            )

        # Cria um único objeto com todos os dados necessários
        # para o microserviço fazer previsões mais tarde
        model_package = {
            "trained_at": trained_at.isoformat(),
            "feature_options": feature_options,
            "feature_lines": feature_lines,
            "feature_models": feature_models,
            "baseline_options": baseline_options,
            "baseline_line": baseline_line,
            "baseline_model": baseline_model,
            "phase_models": phase_models,
        }

        # Converte o caminho do ficheiro num objeto Path
        output_path = Path(MODEL_OUTPUT_PATH)

        # Cria a pasta de destino, caso ainda não exista
        output_path.parent.mkdir(
            parents=True,
            exist_ok=True
        )

        # Abre o ficheiro em modo binário de escrita
        with open(output_path, "wb") as f:
            # Guarda todo o pacote do modelo num ficheiro pickle
            pickle.dump(model_package, f)

        # Mostra informação final sobre o treino
        print(f"\nModelo pickle guardado em: {output_path}")
        print(f"Fases treinadas: {len(phase_models)}")
        print(f"trained_at = {trained_at}")

    finally:
        # Estes comandos são sempre executados,
        # mesmo que ocorra um erro durante o treino

        # Fecha o cursor
        cur.close()

        # Fecha a ligação à base de dados
        conn.close()


# Avalia o modelo através de validação cruzada
def cross_validated_r2(model, X, y):
    # Usa no máximo 5 divisões.
    # Se existirem menos amostras, usa um número inferior.
    n_splits = min(5, len(y))

    # Não é possível fazer validação cruzada
    # com menos de duas divisões
    if n_splits < 2:
        return float("nan")

    # Divide os dados em vários grupos
    kfold = KFold(
        n_splits=n_splits,

        # Baralha as amostras antes da divisão
        shuffle=True,

        # Garante que o resultado é repetível
        random_state=42
    )

    # Treina e avalia o modelo várias vezes,
    # usando grupos diferentes para treino e validação
    scores = cross_val_score(
        model,
        X,
        y,
        cv=kfold,
        scoring="r2"
    )

    # Devolve a média dos valores de R²
    return float(np.mean(scores))


# Carrega as fases de fabrico existentes
def load_phases(cur):
    # Procura o ID e o nome de todas as fases
    cur.execute(
        """
        SELECT id, trim(name)
        FROM manufacturing_phase
        ORDER BY id;
        """
    )

    # Cria um dicionário semelhante a:
    # {
    #     "Painting": {"id": 1},
    #     "Assembly": {"id": 2}
    # }
    return {
        name: {"id": pid}
        for pid, name in cur.fetchall()
    }


# Obtém as opções de configuração que serão usadas como features
def load_option_features(cur):
    # Procura todas as opções e a configuração a que pertencem
    cur.execute("""
        SELECT
            co.id,
            co.config_id,
            co.is_default,
            c.allow_multiple
        FROM config_option co
        JOIN config c
            ON c.id = co.config_id;
    """)

    # Obtém todas as linhas devolvidas pela consulta
    rows = cur.fetchall()

    # Guarda a opção base de cada configuração
    baseline_options = {}

    # Identifica as opções predefinidas de configurações
    # onde apenas uma opção pode ser escolhida
    for option_id, config_id, is_default, allow_multiple in rows:
        if not allow_multiple and is_default:
            baseline_options[config_id] = option_id

    # Lista das opções que terão uma coluna no vetor X
    feature_options = []

    for option_id, config_id, is_default, allow_multiple in rows:
        # Em configurações de escolha múltipla,
        # todas as opções são usadas como features
        if allow_multiple:
            feature_options.append(option_id)

        # Em configurações de escolha única,
        # a opção baseline não é incluída como feature
        elif baseline_options.get(config_id) != option_id:
            feature_options.append(option_id)

    return feature_options, baseline_options


# Obtém as linhas de produção que serão usadas como features
def load_production_lines(cur):
    # Procura todas as linhas de produção
    cur.execute(
        "SELECT id FROM production_line ORDER BY id;"
    )

    # Cria uma lista apenas com os IDs
    lines = [row[0] for row in cur.fetchall()]

    # A primeira linha é usada como referência/baseline
    baseline_line = lines[0] if lines else None

    # As restantes linhas terão posições próprias no vetor de features
    feature_lines = lines[1:]

    return feature_lines, baseline_line


# Obtém os modelos de carro usados como features
def load_models_feature(cur):
    # Procura os IDs de todos os modelos
    cur.execute(
        "SELECT id FROM model ORDER BY id;"
    )

    # Cria uma lista com os IDs
    models = [row[0] for row in cur.fetchall()]

    # O primeiro modelo é usado como baseline
    baseline_model = models[0] if models else None

    # Os restantes modelos terão colunas próprias no vetor X
    feature_models = models[1:]

    return feature_models, baseline_model


# Constrói o conjunto de dados para uma fase específica
def build_dataset(
    cur,
    phase_id,
    feature_options,
    feature_lines,
    feature_models
):
    # Procura todas as execuções concluídas da fase indicada
    cur.execute("""
        SELECT
            pp.id,
            pp.datetime_ini,
            pp.datetime_end,
            pp.product_id,
            w.production_line_id,
            p.model_id
        FROM product_phase pp
        JOIN workstation w
            ON w.id = pp.workstation_id
        JOIN product p
            ON p.id = pp.product_id
        WHERE pp.manufacturing_phase_id = %s
          AND pp.datetime_end IS NOT NULL;
    """, (phase_id,))

    # Cada linha representa uma execução concluída da fase
    phase_rows = cur.fetchall()

    # Número de amostras disponíveis
    n = len(phase_rows)

    # Número total de características do modelo
    n_features = (
        len(feature_options)
        + len(feature_lines)
        + len(feature_models)
    )

    # Cria a matriz X, inicialmente preenchida com zeros.
    # Cada linha representa uma execução da fase.
    # Cada coluna representa uma característica.
    X = np.zeros((n, n_features))

    # Cria o vetor y onde será guardada
    # a duração real de cada fase em segundos
    y = np.zeros(n)

    # Cria um mapa entre o ID da opção e a sua coluna em X
    option_index = {
        opt_id: i
        for i, opt_id in enumerate(feature_options)
    }

    # As colunas das linhas começam depois das opções
    line_offset = len(feature_options)

    # Cria um mapa entre o ID da linha e a sua coluna
    line_index = {
        line_id: line_offset + i
        for i, line_id in enumerate(feature_lines)
    }

    # As colunas dos modelos começam depois das opções e linhas
    model_offset = line_offset + len(feature_lines)

    # Cria um mapa entre o ID do modelo e a sua coluna
    model_index = {
        model_id: model_offset + i
        for i, model_id in enumerate(feature_models)
    }

    # Percorre todas as execuções concluídas da fase
    for row_i, (
        _,
        dt_ini,
        dt_end,
        product_id,
        line_id,
        model_id
    ) in enumerate(phase_rows):

        # Calcula a duração real da fase em segundos
        # e guarda-a no vetor y
        y[row_i] = (
            dt_end - dt_ini
        ).total_seconds()

        # Se a linha tiver uma feature própria,
        # marca essa coluna com 1
        if line_id in line_index:
            X[row_i, line_index[line_id]] = 1

        # Se o modelo tiver uma feature própria,
        # marca essa coluna com 1
        if model_id in model_index:
            X[row_i, model_index[model_id]] = 1

        # Procura as opções escolhidas para este produto
        cur.execute(
            """
            SELECT config_option_id
            FROM product_config
            WHERE product_id = %s;
            """,
            (product_id,)
        )

        # Percorre as opções selecionadas
        for (option_id,) in cur.fetchall():
            # Se a opção for usada como feature,
            # marca a respetiva coluna com 1
            if option_id in option_index:
                X[row_i, option_index[option_id]] = 1

    # Devolve:
    # X -> características
    # y -> duração real
    # n -> número de amostras
    return X, y, n


# Só executa main() quando este ficheiro é iniciado diretamente
if __name__ == "__main__":
    main()
