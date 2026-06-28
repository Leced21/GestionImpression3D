param(
    [string]$Environment = "dev"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "Déploiement : $Environment"
Write-Host ""

switch ($Environment)
{
    "dev"
    {
        $compose = "docker/docker-compose.dev.yml"
        $envFile = "docker/.env.dev"
    }

    "staging"
    {
        $compose = "docker/docker-compose.staging.yml"
        $envFile = "docker/.env.staging"
    }

    "prod"
    {
        $compose = "docker/docker-compose.prod.yml"
        $envFile = "docker/.env.prod"
    }

    default
    {
        throw "Environnement inconnu."
    }
}

docker compose `
    --env-file $envFile `
    -f docker/docker-compose.yml `
    -f $compose `
    pull

docker compose `
    --env-file $envFile `
    -f docker/docker-compose.yml `
    -f $compose `
    up -d

docker image prune -f