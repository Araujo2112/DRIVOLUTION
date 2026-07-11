from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Optional
import os
import pickle
import numpy as np

app = FastAPI(title="Drivolution ML ETA Service")

MODEL_PATH = os.getenv("MODEL_PATH", "phase_time_model.pkl")
MODEL = None


class PredictRequest(BaseModel):
    phase_id: int
    model_id: int
    option_ids: List[int]
    line_id: Optional[int] = None
    fallback_seconds: int = 1800


class PredictResponse(BaseModel):
    predicted_seconds: int
    is_ml_prediction: bool


def load_model():
    global MODEL

    if not os.path.exists(MODEL_PATH):
        MODEL = None
        print(f"[ML] Modelo não encontrado em {MODEL_PATH}. A usar fallback.")
        return

    with open(MODEL_PATH, "rb") as f:
        MODEL = pickle.load(f)

    print(f"[ML] Modelo carregado de {MODEL_PATH}.")


@app.on_event("startup")
def startup():
    load_model()


@app.get("/health")
def health():
    return {
        "status": "ok",
        "model_loaded": MODEL is not None
    }


@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest):
    if MODEL is None:
        return PredictResponse(
            predicted_seconds=req.fallback_seconds,
            is_ml_prediction=False
        )

    phase_models = MODEL.get("phase_models", {})
    model_data = phase_models.get(req.phase_id)

    if model_data is None:
        return PredictResponse(
            predicted_seconds=req.fallback_seconds,
            is_ml_prediction=False
        )

    feature_options = MODEL["feature_options"]
    feature_lines = MODEL["feature_lines"]
    feature_models = MODEL["feature_models"]

    n_features = len(feature_options) + len(feature_lines) + len(feature_models)
    x = np.zeros((1, n_features))

    option_index = {opt_id: i for i, opt_id in enumerate(feature_options)}

    line_offset = len(feature_options)
    line_index = {
        line_id: line_offset + i
        for i, line_id in enumerate(feature_lines)
    }

    model_offset = line_offset + len(feature_lines)
    model_index = {
        model_id: model_offset + i
        for i, model_id in enumerate(feature_models)
    }

    for option_id in req.option_ids:
        if option_id in option_index:
            x[0, option_index[option_id]] = 1

    if req.line_id in line_index:
        x[0, line_index[req.line_id]] = 1

    if req.model_id in model_index:
        x[0, model_index[req.model_id]] = 1

    pipeline = model_data["pipeline"]
    predicted = float(pipeline.predict(x)[0])

    if predicted <= 0:
        predicted = req.fallback_seconds

    return PredictResponse(
        predicted_seconds=int(round(predicted)),
        is_ml_prediction=True
    )