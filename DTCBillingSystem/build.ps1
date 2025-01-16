# Build and Run Script for DTC Billing System
Write-Host "Building DTC Billing System..." -ForegroundColor Green

# Navigate to solution directory
$solutionPath = $PSScriptRoot

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean "$solutionPath\DTCBillingSystem.sln"
if (-not $?) {
    Write-Host "Failed to clean solution" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore "$solutionPath\DTCBillingSystem.sln"
if (-not $?) {
    Write-Host "Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build "$solutionPath\DTCBillingSystem.sln" --configuration Debug
if (-not $?) {
    Write-Host "Build failed! Please check the error messages above." -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green

# Run the application
Write-Host "Starting the application..." -ForegroundColor Green
$uiPath = "$solutionPath\DTCBillingSystem.UI\bin\Debug\net6.0-windows\DTCBillingSystem.UI.exe"
if (Test-Path $uiPath) {
    Start-Process $uiPath -Wait
} else {
    Write-Host "Could not find executable at: $uiPath" -ForegroundColor Red
    exit 1
} 