# Install Unity Hub (Personal, free). Does not install Pro.
$ErrorActionPreference = "Stop"
winget install --id Unity.UnityHub -e --accept-package-agreements --accept-source-agreements
Write-Host "Open Unity Hub, sign in with Personal (free), install Unity 6 LTS + Android Build Support, then Open this project folder (Pool Pocket Shot)."
