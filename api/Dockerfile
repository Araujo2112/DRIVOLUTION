FROM mcr.microsoft.com/dotnet/aspnet:8.0 as base
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends python3 python3-pip && \
    rm -rf /var/lib/apt/lists/* && \
    pip3 install --no-cache-dir --break-system-packages psycopg2-binary scikit-learn numpy

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY . /src
WORKDIR /src
RUN dotnet build "Drivolution.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Drivolution.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY MachineLearning/train_phase_time_model.py /app/ml/train_phase_time_model.py
ENTRYPOINT ["dotnet", "Drivolution.dll"]