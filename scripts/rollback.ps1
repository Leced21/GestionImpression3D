param(
    [string]$ImageTag
)

if ([string]::IsNullOrWhiteSpace($ImageTag))
{
    throw "Préciser un tag."
}

(Get-Content docker/.env.prod) |
ForEach-Object {

    if ($_ -match "^IMAGE_TAG=")
    {
        "IMAGE_TAG=$ImageTag"
    }
    else
    {
        $_
    }

} | Set-Content docker/.env.prod

docker compose `
    --env-file docker/.env.prod `
    -f docker/docker-compose.yml `
    -f docker/docker-compose.prod.yml `
    up -d