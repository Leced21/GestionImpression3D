param(
    [string]$Url
)

$MaxRetry = 30

for($i=1;$i -le $MaxRetry;$i++)
{
    try
    {
        $response = Invoke-WebRequest $Url -UseBasicParsing

        if($response.StatusCode -eq 200)
        {
            Write-Host "Application disponible."

            exit 0
        }
    }
    catch
    {
    }

    Start-Sleep 2
}

Write-Host "Healthcheck échoué."

exit 1