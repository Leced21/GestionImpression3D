#!/bin/sh

echo "Attente de SQL Server..."

until nc -z sqlserver 1433
do
    sleep 2
done

echo "SQL Server disponible."

dotnet Backend.dll