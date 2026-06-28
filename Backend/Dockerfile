# ===============================
# BUILD
# ===============================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copie des fichiers de solution
COPY Backend.slnx .
COPY Backend.csproj .

# Si tu ajoutes d'autres projets plus tard,
# ajoute simplement les COPY ici.

RUN dotnet restore Backend.slnx

# Copie du reste
COPY . .

RUN dotnet publish Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===============================
# RUNTIME
# ===============================

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

ENV ASPNETCORE_ENVIRONMENT=Production

ENV DOTNET_gcServer=1

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

COPY entrypoint.sh .

RUN chmod +x entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["./entrypoint.sh"]