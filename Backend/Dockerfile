# ==========================================
# BUILD
# ==========================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copie uniquement le projet Backend pour profiter du cache Docker
COPY Backend/Backend.csproj Backend/

WORKDIR /src/Backend

RUN dotnet restore Backend.csproj

# Copie ensuite tout le projet Backend
WORKDIR /src

COPY Backend/. Backend/

WORKDIR /src/Backend

RUN dotnet publish Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ==========================================
# RUNTIME
# ==========================================

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_gcServer=1

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet","Backend.dll"]