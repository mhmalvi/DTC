Write-Host "Building and running DTC Billing System..." -ForegroundColor Green

# Build the solution
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Starting application..." -ForegroundColor Green
    
    # Run the UI project
    dotnet run --project DTCBillingSystem.UI/DTCBillingSystem.UI.csproj
} else {
    Write-Host "Build failed. Please check the errors above." -ForegroundColor Red
} 