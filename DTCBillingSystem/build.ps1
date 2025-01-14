# Build and Run Script for DTC Billing System
Write-Host "Building DTC Billing System..." -ForegroundColor Green

# Navigate to solution directory
$solutionPath = $PSScriptRoot

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean "$solutionPath\DTCBillingSystem.sln"

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore "$solutionPath\DTCBillingSystem.sln"

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build "$solutionPath\DTCBillingSystem.sln" --configuration Release

# Check if build was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully!" -ForegroundColor Green
    
    # Run the application (assuming UI project is the startup project)
    Write-Host "Starting the application..." -ForegroundColor Green
    dotnet run --project "$solutionPath\DTCBillingSystem.UI\DTCBillingSystem.UI.csproj"
} else {
    Write-Host "Build failed! Please check the error messages above." -ForegroundColor Red
} 