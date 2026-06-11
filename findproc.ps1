$procs = Get-Process | Where-Object { $_.ProcessName -match 'devenv|iisexpress|msbuild|GeekTour' }
foreach ($p in $procs) {
    Write-Output "$($p.Id) $($p.ProcessName)"
}
