FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends busybox-static wget \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Hotel/HBP.Hotel.API/HBP.Hotel.API.csproj", "src/Services/Hotel/HBP.Hotel.API/"]
COPY ["src/Services/Hotel/HBP.Hotel.Application/HBP.Hotel.Application.csproj", "src/Services/Hotel/HBP.Hotel.Application/"]
COPY ["src/Services/Hotel/HBP.Hotel.Domain/HBP.Hotel.Domain.csproj", "src/Services/Hotel/HBP.Hotel.Domain/"]
COPY ["src/Services/Hotel/HBP.Hotel.Infrastructure/HBP.Hotel.Infrastructure.csproj", "src/Services/Hotel/HBP.Hotel.Infrastructure/"]
COPY ["src/BuildingBlocks/HBP.Common/HBP.Common.csproj", "src/BuildingBlocks/HBP.Common/"]
COPY ["src/BuildingBlocks/HBP.Data.Abstractions/HBP.Data.Abstractions.csproj", "src/BuildingBlocks/HBP.Data.Abstractions/"]
COPY ["src/BuildingBlocks/HBP.Data.Postgres/HBP.Data.Postgres.csproj", "src/BuildingBlocks/HBP.Data.Postgres/"]
COPY ["src/BuildingBlocks/HBP.Messaging.Abstractions/HBP.Messaging.Abstractions.csproj", "src/BuildingBlocks/HBP.Messaging.Abstractions/"]
COPY ["src/BuildingBlocks/HBP.Messaging.Kafka/HBP.Messaging.Kafka.csproj", "src/BuildingBlocks/HBP.Messaging.Kafka/"]
COPY ["src/BuildingBlocks/HBP.Observability/HBP.Observability.csproj", "src/BuildingBlocks/HBP.Observability/"]
RUN dotnet restore "src/Services/Hotel/HBP.Hotel.API/HBP.Hotel.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Hotel/HBP.Hotel.API"
RUN dotnet build "./HBP.Hotel.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HBP.Hotel.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HBP.Hotel.API.dll"]
