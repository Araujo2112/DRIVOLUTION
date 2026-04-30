# Usando imagem oficial do Python
FROM python:3.12-slim

# Definir diretório de trabalho no contêiner
WORKDIR /app

# Copiar o arquivo requirements.txt para o contêiner
COPY requirements.txt /app/

# Instalar as dependências a partir do requirements.txt
RUN pip install --no-cache-dir -r /app/requirements.txt

# Copiar os scripts Python para dentro do contêiner
COPY ./scripts /app/scripts

# Definir o comando de entrada padrão para executar o script Python
ENTRYPOINT ["python", "/app/scripts/validator.py"]
