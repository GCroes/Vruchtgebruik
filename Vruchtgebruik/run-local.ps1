# Vruchtgebruik - Local Dev Script (clone, build, run API and Angular)
# Place this script in D:\test and run from there

function Test-PortAvailable {
    param (
        [Parameter(Mandatory = $true)][int]$Port
    )
    # Returns $true if port is NOT in use
    $used = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Where-Object { $_.State -eq "Listen" }
    return -not $used
}

# ---- Preflight: Check API port ----
$ApiPort = 5005   # Change if you use another port
$AngularPort = 4200

Write-Host ""
Write-Host "==== PREFLIGHT: Checking required ports... ====" -ForegroundColor Yellow

$apiPortOk = Test-PortAvailable $ApiPort
if (-not $apiPortOk) {
    Write-Host "ERROR: API port $ApiPort is already in use! Please free this port before running the script." -ForegroundColor Red
    exit 1
}
else {
    Write-Host "API port $ApiPort is available." -ForegroundColor Green
}

$ngPortOk = Test-PortAvailable $AngularPort
if (-not $ngPortOk) {
    Write-Host "ERROR: Angular port $AngularPort is already in use! Please free this port before running the script." -ForegroundColor Red
    exit 1
}
else {
    Write-Host "Angular port $AngularPort is available." -ForegroundColor Green
}

$RepoUrl = "https://github.com/gcroes/vruchtgebruik.git"
$RepoDir = "vruchtgebruik"
$ApiProjectDir = "$RepoDir\vruchtgebruik\Vruchtgebruik.Api"
#$AngularDir = "$RepoDir\vruchtgebruik\vruchtgebruik.frontend\vruchtgebruik.frontend.client"
$AngularDir = "vruchtgebruik.frontend\vruchtgebruik.frontend.client"

# ---- STEP 0: Clone if needed ----
Write-Host ""
Write-Host "==== STEP 0: Cloning the repository if not present ====" -ForegroundColor Yellow
if (-not (Test-Path $RepoDir)) {
    git clone $RepoUrl
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error cloning repository!"
        exit 1
    }
} else {
    Write-Host "Repo folder already exists, skipping clone."
}

# Move into the repo for all remaining steps
Set-Location $RepoDir\vruchtgebruik

# ---- API ----
Write-Host ""
Write-Host "==== STEP 1: Restoring dependencies for API ====" -ForegroundColor Cyan
dotnet restore "Vruchtgebruik.Api"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error restoring API dependencies!"
    exit 1
}

Write-Host ""
Write-Host "==== STEP 2: Building the API ====" -ForegroundColor Cyan
dotnet build "Vruchtgebruik.Api"
if ($LASTEXITCODE -ne 0) {
    Write-Error "API build failed!"
    exit 1
}

Write-Host ""
Write-Host "==== STEP 3: Running the API in a new window ====" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PWD`"; dotnet run --project `"Vruchtgebruik.Api`""

# ---- ANGULAR ----
Write-Host ""
Write-Host "==== STEP 4: Running Angular Frontend in a new window ====" -ForegroundColor Green

if (Test-Path $AngularDir) {
    Push-Location $AngularDir

    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing Angular dependencies..."
        npm install
    }

    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $(Get-Location); npx ng serve"
    Pop-Location
} else {
    Write-Error "Angular directory '$AngularDir' not found! Please check the folder path."
}

Write-Host ""
Write-Host "==== API and Angular frontend are running! ===="
Write-Host "API:      http://localhost:5005 (or check launchSettings.json for your API port)"
Write-Host "Frontend: http://localhost:4200"
Write-Host "Stop API:   Close the API window"
Write-Host "Stop Angular: Close the Angular window"