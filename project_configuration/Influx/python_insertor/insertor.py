import time
import random
from datetime import datetime
from influxdb_client import InfluxDBClient, Point

# Configurações do InfluxDB
url = "http://localhost:8086"
token = "vo9HA3cEWOyC808UU9tYpNdUxX1KISGVBJOM5RUkCvEXw33QTHCC1mBDjInTYMx7WVneHaBVnvfC0wZb_i1rAA=="  # Substitua pelo seu token
org = "TextPack"      # Substitua pela sua organização
bucket = "TextPack"

# Criação do cliente InfluxDB
client = InfluxDBClient(url=url, token=token, org=org)
write_api = client.write_api()

# Função para gerar dados aleatórios
def generate_random_data():
    locations = ["Room1", "Room2", "Room3", "Room4", "Room5"]
    location = random.choice(locations)
    temperature = round(random.uniform(20, 25), 1)
    humidity = round(random.uniform(40, 50), 1)
    pressure = random.randint(1000, 1020)
    return location, temperature, humidity, pressure

# Inserir dados a cada meio segundo
try:
    while True:
        # Gerar dados aleatórios
        location, temperature, humidity, pressure = generate_random_data()
        timestamp = datetime.utcnow()  # Timestamp atual em UTC

        # Criar ponto de dados
        point = (
            Point("sensor_data")
            .tag("location", location)
            .field("temperature", temperature)
            .field("humidity", humidity)
            .field("pressure", pressure)
            .time(timestamp)
        )

        # Inserir ponto de dados no InfluxDB
        write_api.write(bucket=bucket, record=point)
        print(f"Inserido: {location}, {temperature}, {humidity}, {pressure} em {timestamp}")

        # Esperar meio segundo
        time.sleep(0.01)

except KeyboardInterrupt:
    print("Inserção de dados interrompida.")

# Fechar o cliente
client.close()
