import base64
import pickle
from typing import List, Dict

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from river import tree

app = FastAPI()
_MODEL_STORE: bytes | None = None


class TrainRequest(BaseModel):
    model_base64: str
    version: int
    last_date_iso: str
    model_type: str
    dataset: List[Dict[str, float]]


class TrainResponse(BaseModel):
    model_base64: str


class PredictRequest(BaseModel):
    features: List[Dict[str, float]]


class PredictResponse(BaseModel):
    predictions: List[float]


@app.post("/train", response_model=TrainResponse)
async def train(req: TrainRequest):
    global _MODEL_STORE

    if req.model_base64:
        try:
            model = pickle.loads(base64.b64decode(req.model_base64))
        except Exception:
            model = tree.HoeffdingAdaptiveTreeClassifier(
                grace_period=50,

            )
    else:
        model = tree.HoeffdingAdaptiveTreeClassifier(
            grace_period=50,
        )

    for row in req.dataset:
        y = row.pop("label")
        model.learn_one(row, y)

    model_bytes = pickle.dumps(model)
    _MODEL_STORE = model_bytes
    return TrainResponse(model_base64=base64.b64encode(model_bytes).decode())


@app.post("/predict", response_model=PredictResponse)
async def predict(req: PredictRequest):
    if _MODEL_STORE is None:
        raise HTTPException(400, "modelo ainda não treinado")

    model = pickle.loads(_MODEL_STORE)
    try:
        preds = [model.predict_one(feat) for feat in req.features]
    except Exception as e:
        raise HTTPException(500, f"erro na predição: {e}")

    return PredictResponse(predictions=preds)


if __name__ == "__main__":
    # hypercorn server:app --bind 0.0.0.0:8815 --certfile cert.pem --keyfile key.pem --quic-bind 0.0.0.0:4433
    import uvicorn

    uvicorn.run("server:app", host="127.0.0.1", port=4433, ssl_keyfile="key.pem", ssl_certfile="cert.pem")
