$uri = "http://localhost:5005/api/orders/update"

Write-Host "Sending two concurrent PUT /api/orders/update requests to $uri`n" -ForegroundColor Cyan

$bodyA = @{ orderId = 1; productId = 101; quantity = 4; status = "Updated-From-Request-A" } | ConvertTo-Json -Compress
$bodyB = @{ orderId = 1; productId = 101; quantity = 5; status = "Updated-From-Request-B" } | ConvertTo-Json -Compress

$job1 = Start-Job -ScriptBlock {
    param($u, $b)
    Invoke-RestMethod -Method Put -Uri $u -ContentType "application/json" -Body $b
} -ArgumentList $uri, $bodyA

$job2 = Start-Job -ScriptBlock {
    param($u, $b)
    Invoke-RestMethod -Method Put -Uri $u -ContentType "application/json" -Body $b
} -ArgumentList $uri, $bodyB

Wait-Job $job1, $job2 | Out-Null

Write-Host "`nResponse from Request A:" -ForegroundColor Yellow
Receive-Job $job1

Write-Host "`nResponse from Request B:" -ForegroundColor Yellow
Receive-Job $job2

Remove-Job $job1, $job2

Write-Host "`nFinal state of order 1:" -ForegroundColor Green
Invoke-RestMethod -Method Get -Uri "http://localhost:5005/api/orders/1"


