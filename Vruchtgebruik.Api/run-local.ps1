# CONFIGURATION
$RepoUrl = "https://github.com/your-username/vruchtgebruik-api.git"
$RepoDir = "vruchtgebruik-api"
$ProjectDir = "Vruchtgebruik.Api"  # Update if your project folder is different

Write-Host ""
Write-Host "==== STEP 1: Cloning the repository ====" -ForegroundColor Cyan

if (-not (Test-Path $RepoDir)) {
    git clone $RepoUrl
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error cloning repository!"
        exit 1
    }
} else {
    Write-Host "Repository folder already exists. Skipping clone."
}

Set-Location $RepoDir

Write-Host ""
Write-Host "==== STEP 2: Restoring dependencies ====" -ForegroundColor Cyan

dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error restoring dependencies!"
    exit 1
}

Write-Host ""
Write-Host "==== STEP 3: Building the project ====" -ForegroundColor Cyan

dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

Write-Host ""
Write-Host "==== STEP 4: Running the API ====" -ForegroundColor Cyan

dotnet run --project $ProjectDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "API failed to start!"
    exit 1
}

Write-Host ""
Write-Host "API stopped. Press Enter to exit."
Read-Host
