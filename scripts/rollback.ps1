param(
    [string]$Tag
)

(Get-Content C:\PrintFlow3D\.env) |

ForEach-Object {

    if($_ -match "^IMAGE_TAG=")
    {
        "IMAGE_TAG=$Tag"
    }
    else
    {
        $_
    }

} |

Set-Content C:\PrintFlow3D\.env

Set-Location C:\PrintFlow3D

docker compose `
    -f docker\docker-compose.yml `
    -f docker\docker-compose.prod.yml `
    up -d