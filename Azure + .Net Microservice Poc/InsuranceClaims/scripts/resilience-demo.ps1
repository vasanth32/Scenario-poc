<#
  Live resilience demo: starts Policy.Api and Claims.Api, injects faults through Policy.Api's /api/chaos
  endpoint and shows retries, the circuit opening, fail-fast, and recovery.

  Run from the InsuranceClaims folder:   .\scripts\resilience-demo.ps1
  Needs SQL Server reachable with the connection strings in each appsettings.json (databases already migrated).
  Demo claims created along the way are deleted at the end.
#>
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$policyUrl = 'http://localhost:5102'
$claimsUrl = 'http://localhost:5101'
$claimsLog = Join-Path $env:TEMP 'claims-demo.log'
$policyLog = Join-Path $env:TEMP 'policy-demo.log'
$created = [System.Collections.Generic.List[string]]::new()

function Start-Api($project, $log) {
    Start-Process dotnet -ArgumentList 'run', '--project', "`"$(Join-Path $root $project)`"", '--launch-profile', 'http' `
        -RedirectStandardOutput $log -RedirectStandardError "$log.err" -WindowStyle Hidden | Out-Null
}

# Any HTTP response (even 404) means the server is up.
function Wait-Ready($url) {
    for ($i = 0; $i -lt 90; $i++) {
        try { Invoke-WebRequest "$url/api/chaos" -UseBasicParsing -TimeoutSec 2 | Out-Null; return }
        catch { if ($_.Exception.Response) { return }; Start-Sleep 1 }
    }
    throw "Timed out waiting for $url"
}

function Stop-Api($port) {
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
}

function Set-Chaos($body) {
    Invoke-RestMethod "$policyUrl/api/chaos" -Method Put -ContentType 'application/json' -Body ($body | ConvertTo-Json) | Out-Null
    Write-Host "  chaos = $($body | ConvertTo-Json -Compress)" -ForegroundColor DarkYellow
}

function New-Claim($label) {
    $body = '{"policyId":"11111111-1111-1111-1111-111111111111","description":"resilience demo","amount":10}'
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $r = Invoke-WebRequest "$claimsUrl/api/claims" -Method Post -ContentType 'application/json' -Body $body -UseBasicParsing -TimeoutSec 60
        $code = [int]$r.StatusCode
        if ($code -eq 201) { $created.Add(($r.Content | ConvertFrom-Json).id) }
    }
    catch { $code = [int]$_.Exception.Response.StatusCode }
    $sw.Stop()
    Write-Host ("  {0,-24} -> HTTP {1}  in {2,5:F1}s" -f $label, $code, $sw.Elapsed.TotalSeconds)
}

# Prints new resilience-related log lines since the last call; returns the new line count.
function Show-Log($title, $since) {
    Start-Sleep -Milliseconds 500
    Write-Host "  -- Claims.Api log: $title --" -ForegroundColor Cyan
    $lines = @(Get-Content $claimsLog)
    $lines | Select-Object -Skip $since |
        Select-String '^\s*(Retry \d|Circuit |Timed out calling|Policy.Api is unreachable)' |
        ForEach-Object { Write-Host ("    " + $_.Line.Trim()) }
    return $lines.Count
}

try {
    Write-Host "Starting APIs..." -ForegroundColor Green
    Start-Api 'Policy.Api' $policyLog
    Start-Api 'Claims.Api' $claimsLog
    Wait-Ready $policyUrl
    Wait-Ready $claimsUrl
    Set-Chaos @{}
    $mark = (@(Get-Content $claimsLog)).Count

    Write-Host "`n1) Baseline: healthy Policy.Api" -ForegroundColor Green
    New-Claim 'healthy call'

    Write-Host "`n2) Latency 3000 ms (> 2 s attempt timeout): attempts time out and are retried" -ForegroundColor Green
    Set-Chaos @{ latencyMs = 3000 }
    New-Claim 'slow Policy.Api'
    $mark = Show-Log 'latency' $mark

    Write-Host "`n3) 50% random HTTP 500: retries rescue some calls" -ForegroundColor Green
    Set-Chaos @{ failureRate = 0.5 }
    1..4 | ForEach-Object { New-Claim "flaky call $_" }
    $mark = Show-Log 'random 500s' $mark

    Write-Host "`n4) Downtime: failures pile up and the circuit opens" -ForegroundColor Green
    Set-Chaos @{ down = $true }
    1..6 | ForEach-Object { New-Claim "down call $_" }
    $mark = Show-Log 'downtime' $mark

    Write-Host "`n5) Policy.Api healthy again, but the circuit is still open: calls fail fast" -ForegroundColor Green
    Set-Chaos @{}
    New-Claim 'right after recovery'
    $mark = Show-Log 'fail fast' $mark

    Write-Host "`n6) Waiting 16 s for the break duration; the next call is the half-open trial" -ForegroundColor Green
    Start-Sleep 16
    New-Claim 'trial call'
    New-Claim 'normal call'
    $null = Show-Log 'recovery' $mark
}
finally {
    foreach ($id in $created) {
        try { Invoke-WebRequest "$claimsUrl/api/claims/$id" -Method Delete -UseBasicParsing | Out-Null } catch { }
    }
    Stop-Api 5101; Stop-Api 5102
    Remove-Item $claimsLog, "$claimsLog.err", $policyLog, "$policyLog.err" -ErrorAction SilentlyContinue
    Write-Host "`nDone. Demo claims removed, APIs stopped." -ForegroundColor Green
}
