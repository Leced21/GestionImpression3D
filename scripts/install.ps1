$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "==============================="
Write-Host " PrintFlow3D Installation"
Write-Host "==============================="
Write-Host ""

if (!(Get-Command docker -ErrorAction SilentlyContinue))
{
    throw "Docker Desktop n'est pas installé."
}

if (!(Get-Command git -ErrorAction SilentlyContinue))
{
    throw "Git n'est pas installé."
}

docker version

Write-Host ""
Write-Host "Connexion au GitLab Registry..."

docker login

Write-Host ""
Write-Host "Création des volumes..."

docker volume create printflow_sqlserver | Out-Null
docker volume create printflow_uploads | Out-Null

Write-Host ""
Write-Host "Installation terminée."