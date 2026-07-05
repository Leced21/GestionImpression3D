$Date = Get-Date -Format "yyyyMMdd-HHmm"

$Folder = "backup/$Date"

New-Item `
    -ItemType Directory `
    -Force `
    -Path $Folder | Out-Null

docker exec printflow-sqlserver `
    /opt/mssql-tools/bin/sqlcmd `
    -S localhost `
    -U sa `
    -P $env:SA_PASSWORD `
    -Q "BACKUP DATABASE PrintFlow3D TO DISK='/var/opt/mssql/backup/PrintFlow3D.bak'"

docker cp `
    printflow-sqlserver:/var/opt/mssql/backup/PrintFlow3D.bak `
    "$Folder/PrintFlow3D.bak"

Compress-Archive `
    uploads `
    "$Folder/uploads.zip"

Write-Host "Sauvegarde terminée."