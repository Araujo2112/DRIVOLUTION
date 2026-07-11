FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY ml_service.py .
COPY phase_time_model.pkl .

CMD ["uvicorn", "ml_service:app", "--host", "0.0.0.0", "--port", "8000"]