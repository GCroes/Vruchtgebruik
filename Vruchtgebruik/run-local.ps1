# Vruchtgebruik - Local Run Script
# ================================
# This script will:
# 1. Clone the GitHub repo (if not already present)
# 2. Restore and build the ASP.NET Core API
# 3. Run the API
# 4. Restore and run the Angular app in a new window

# ---- CONFIG ----
$RepoUrl = "https://github.com/gcroes/vruchtgebruik.git"
$RootDir = "Vruchtgebruik"
$ApiRepoDir = "Vruchtgebruik\Vruchtgebruik"
$ApiProjectDir = "Vruchtgebruik.Api"
$AngularDir = "Vruchtgebruik\Vruchtgebruik\Vruchtgebruik.FrontEnd\vruchtgebruik.frontend.client"

Write-Host ""
Write-Host "==== STEP 1: Cloning the repository ====" -ForegroundColor Cyan

if (-not (Test-Path $RootDir)) {
    git clone $RepoUrl
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error cloning repository!"
        exit 1
    }
} else {
    Write-Host "Repository folder already exists. Skipping clone."
}

# ---- API ----
Write-Host ""
Write-Host "==== STEP 2: Restoring dependencies for API ====" -ForegroundColor Cyan
Set-Location $ApiRepoDir
dotnet restore $ApiProjectDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error restoring API dependencies!"
    exit 1
}

Write-Host ""
Write-Host "==== STEP 3: Building the API ====" -ForegroundColor Cyan
dotnet build $ApiProjectDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "API build failed!"
    exit 1
}

#Write-Host ""
#Write-Host "`n==== STEP 4: Running the API ====" -ForegroundColor Cyan
#Write-Host "Browse to http://localhost:5005/swagger after startup.`n"
#dotnet run --project "$ProjectDir\Vruchtgebruik.Api.csproj" --urls "http://localhost:5005"
#if ($LASTEXITCODE -ne 0) {
#    Write-Error "API failed to start!"
#    exit 1
#}
#Start-Sleep -Seconds 5

Write-Host ""
Write-Host "==== STEP 4: Running the API in a new window ====" -ForegroundColor Cyan
# Start API in a new terminal window so script can continue
$apiPath = Resolve-Path $ApiRepoDir
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$apiPath`"; dotnet run --project `"$ApiProjectDir`""

# ---- ANGULAR ----
Write-Host ""
Write-Host "==== STEP 5: Running Angular Frontend ====" -ForegroundColor Green

if (Test-Path $AngularDir) {
    Push-Location $AngularDir

    # Install node modules if needed
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing Angular dependencies..."
        npm install
    }

    # Start Angular in new window
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $(Get-Location); npx ng serve"
    Pop-Location
} else {
    Write-Error "Angular directory '$AngularDir' not found! Please check the folder path."
}

Write-Host ""
Write-Host "==== API and Angular frontend are running! ===="
Write-Host "API:     http://localhost:5005 (or check launchSettings.json)"
Write-Host "Frontend: http://localhost:4200"
Write-Host "Stop API:  Run 'Get-Job | Stop-Job' in this window or Ctrl+C in Angular window."
