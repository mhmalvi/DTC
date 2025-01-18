# Stop on any error
$ErrorActionPreference = "Stop"

Write-Host "Building and running DTCBillingSystem..." -ForegroundColor Green

try {
    # Build the solution
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet build --configuration Release

    if ($LASTEXITCODE -eq 0) {
        # Get the output directory
        $outputDir = ".\DTCBillingSystem.UI\bin\Release\net6.0-windows"
        
        # Copy appsettings.json to output directory
        Write-Host "Copying appsettings.json to output directory..." -ForegroundColor Yellow
        Copy-Item -Path ".\appsettings.json" -Destination $outputDir -Force
        
        # Run the application using dotnet run
        Write-Host "Starting application..." -ForegroundColor Cyan
        Set-Location -Path ".\DTCBillingSystem.UI"
        dotnet run --configuration Release --no-build
    } else {
        Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
} finally {
    # Restore original location
    Set-Location -Path $PSScriptRoot
} 