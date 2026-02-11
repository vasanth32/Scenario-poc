$uri = "http://localhost:5005/api/orders/update"

Write-Host "Sending 10 concurrent PUT /api/orders/update requests to $uri`n" -ForegroundColor Cyan

$jobs = @()

1..10 | ForEach-Object {
    $i = $_
    $body = @{
        orderId   = 1
        productId = 101
        quantity  = $i
        status    = "Batch-Request-$i"
    } | ConvertTo-Json -Compress

    $jobs += Start-Job -ScriptBlock {
        param($u, $b)
        Invoke-RestMethod -Method Put -Uri $u -ContentType "application/json" -Body $b
    } -ArgumentList $uri, $body
}

Wait-Job $jobs | Out-Null

Write-Host "`nResponses from batch requests:" -ForegroundColor Yellow
$results = $jobs | Receive-Job
$jobs | Remove-Job

$results

Write-Host "`nFinal state of order 1:" -ForegroundColor Green
Invoke-RestMethod -Method Get -Uri "http://localhost:5005/api/orders/1"


