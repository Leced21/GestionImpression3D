param(
    [string]$BackupFolder
)

if (!(Test-Path $BackupFolder))
{
    throw "Sauvegarde introuvable."
}

docker cp `
"$BackupFolder/PrintFlow3D.bak" `
printflow-sqlserver:/var/opt/mssql/backup/

docker exec printflow-sqlserver `
    /opt/mssql-tools/bin/sqlcmd `
    -S localhost `
    -U sa `
    -P $env:SA_PASSWORD `
    -Q "RESTORE DATABASE PrintFlow3D FROM DISK='/var/opt/mssql/backup/PrintFlow3D.bak' WITH REPLACE"

Expand-Archive `
"$BackupFolder/uploads.zip" `
uploads `
-Force

Write-Host "Restauration terminée."