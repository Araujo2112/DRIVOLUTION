from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Optional
import os
import pickle
import numpy as np

# Cria a aplicação FastAPI
app = FastAPI(title="Drivolution ML ETA Service")

# Caminho onde está guardado o modelo treinado (.pkl)
# Pode ser definido por variável de ambiente
MODEL_PATH = os.getenv("MODEL_PATH", "phase_time_model.pkl")

# Variável global onde ficará o modelo carregado
MODEL = None


# DTO recebido pela API

# Dados enviados pelo backend ASP.NET
class PredictRequest(BaseModel):

    # ID da fase atual
    phase_id: int

    # ID do modelo do carro
    model_id: int

    # Lista das opções de configuração escolhidas
    option_ids: List[int]

    # Linha de produção (opcional)
    line_id: Optional[int] = None

    # Tempo usado caso o modelo não consiga prever
    fallback_seconds: int = 1800


# DTO devolvido pela API

class PredictResponse(BaseModel):

    # Tempo previsto em segundos
    predicted_seconds: int

    # Indica se a previsão veio realmente do modelo ML
    is_ml_prediction: bool


# Carrega o modelo treinado

def load_model():

    # Indica que vamos usar a variável global MODEL
    global MODEL

    # Se o ficheiro não existir...
    if not os.path.exists(MODEL_PATH):

        # Não existe modelo carregado
        MODEL = None

        print(
            f"[ML] Modelo não encontrado em {MODEL_PATH}. "
            "A usar fallback."
        )

        return

    # Abre o ficheiro .pkl
    with open(MODEL_PATH, "rb") as f:

        # Carrega o modelo para memória
        MODEL = pickle.load(f)

    print(f"[ML] Modelo carregado de {MODEL_PATH}.")


# Executado quando o serviço arranca

@app.on_event("startup")
def startup():

    # Carrega automaticamente o modelo
    load_model()


# Endpoint para verificar o estado

@app.get("/health")
def health():

    return {

        # O serviço está a funcionar
        "status": "ok",

        # Diz se existe um modelo carregado
        "model_loaded": MODEL is not None
    }


# Endpoint principal

@app.post(
    "/predict",
    response_model=PredictResponse
)
def predict(req: PredictRequest):

    # Se não existir modelo carregado
    if MODEL is None:

        # Devolve o tempo de fallback
        return PredictResponse(
            predicted_seconds=req.fallback_seconds,

            # Não foi usada Machine Learning
            is_ml_prediction=False
        )

    # Obtém os modelos treinados para cada fase
    phase_models = MODEL.get("phase_models", {})

    # Procura o modelo correspondente à fase pedida
    model_data = phase_models.get(req.phase_id)

    # Se essa fase nunca foi treinada
    if model_data is None:

        return PredictResponse(
            predicted_seconds=req.fallback_seconds,
            is_ml_prediction=False
        )

    # Obtém todas as features usadas pelo modelo
    feature_options = MODEL["feature_options"]
    feature_lines = MODEL["feature_lines"]
    feature_models = MODEL["feature_models"]

    # Calcula o número total de features
    n_features = (
        len(feature_options)
        + len(feature_lines)
        + len(feature_models)
    )

    # Cria o vetor de entrada cheio de zeros
    x = np.zeros((1, n_features))

    # Associa cada opção ao respetivo índice
    option_index = {
        opt_id: i
        for i, opt_id in enumerate(feature_options)
    }

    # Calcula onde começam as features das linhas
    line_offset = len(feature_options)

    line_index = {
        line_id: line_offset + i
        for i, line_id in enumerate(feature_lines)
    }

    # Calcula onde começam as features dos modelos
    model_offset = line_offset + len(feature_lines)

    model_index = {
        model_id: model_offset + i
        for i, model_id in enumerate(feature_models)
    }

    # Marca com 1 todas as opções escolhidas
    for option_id in req.option_ids:

        if option_id in option_index:

            x[0, option_index[option_id]] = 1

    # Marca a linha de produção
    if req.line_id in line_index:

        x[0, line_index[req.line_id]] = 1

    # Marca o modelo do carro
    if req.model_id in model_index:

        x[0, model_index[req.model_id]] = 1

    # Obtém o pipeline treinado
    pipeline = model_data["pipeline"]

    # Calcula a previsão
    predicted = float(
        pipeline.predict(x)[0]
    )

    # Se o resultado for inválido
    if predicted <= 0:

        # Usa o valor de fallback
        predicted = req.fallback_seconds

    # Devolve a previsão
    return PredictResponse(
        predicted_seconds=int(round(predicted)),

        # Foi usada Machine Learning
        is_ml_prediction=True
    )