# CONFIGURATION
$RepoUrl = "https://github.com/gcroes/vruchtgebruik.git"
$RepoDir = "vruchtgebruik\Vruchtgebruik"
$ProjectDir = "Vruchtgebruik.Api"

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

dotnet restore "$ProjectDir"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error restoring dependencies!"
    exit 1
}

Write-Host ""
Write-Host "==== STEP 3: Building the project ====" -ForegroundColor Cyan

dotnet build "$ProjectDir"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

Write-Host ""
Write-Host "`n==== STEP 4: Running the API ====" -ForegroundColor Cyan
Write-Host "Browse to http://localhost:5005/swagger after startup.`n"
dotnet run --project "$ProjectDir\Vruchtgebruik.Api.csproj" --urls "http://localhost:5005"
if ($LASTEXITCODE -ne 0) {
    Write-Error "API failed to start!"
    exit 1
}

Write-Host "API stopped. Press Enter to exit."
Read-Host
