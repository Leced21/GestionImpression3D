# ==========================================
# BUILD
# ==========================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copie uniquement le projet Backend pour profiter du cache Docker
COPY Backend.csproj ./

RUN dotnet restore Backend.csproj

# Copie tout le projet Backend
COPY . ./

RUN dotnet publish Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ==========================================
# RUNTIME
# ==========================================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_gcServer=1
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet","Backend.dll"]
