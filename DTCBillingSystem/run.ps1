# Stop any existing instances of the application
Get-Process "DTCBillingSystem.UI" -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean the solution
Write-Host "Cleaning solution..." -ForegroundColor Yellow
dotnet clean
if (-not $?) {
    Write-Host "Failed to clean solution" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore
if (-not $?) {
    Write-Host "Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build -c Release
if (-not $?) {
    Write-Host "Failed to build solution" -ForegroundColor Red
    exit 1
}

# Publish the UI project
Write-Host "Publishing UI project..." -ForegroundColor Yellow
dotnet publish DTCBillingSystem.UI -c Release
if (-not $?) {
    Write-Host "Failed to publish UI project" -ForegroundColor Red
    exit 1
}

# Run the application
Write-Host "Starting application..." -ForegroundColor Green
$exePath = ".\DTCBillingSystem.UI\bin\Release\net8.0-windows\publish\DTCBillingSystem.UI.exe"
if (Test-Path $exePath) {
    Start-Process $exePath -Wait
} else {
    Write-Host "Could not find executable at: $exePath" -ForegroundColor Red
    exit 1
} 