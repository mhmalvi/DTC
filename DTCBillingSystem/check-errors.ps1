# Get recent application errors
Write-Host "Checking for recent application errors..." -ForegroundColor Yellow
$errors = Get-WinEvent -FilterHashtable @{
    LogName = 'Application'
    Level = 2  # Error level
    StartTime = (Get-Date).AddMinutes(-5)
} -ErrorAction SilentlyContinue

if ($errors) {
    Write-Host "Found recent application errors:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "Time: $($error.TimeCreated)" -ForegroundColor Red
        Write-Host "Source: $($error.ProviderName)" -ForegroundColor Red
        Write-Host "Message: $($error.Message)" -ForegroundColor Red
        Write-Host "---" -ForegroundColor Red
    }
} else {
    Write-Host "No recent application errors found." -ForegroundColor Green
}

# Check if the process is running
$process = Get-Process "DTCBillingSystem.UI" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Application is running with Process ID: $($process.Id)" -ForegroundColor Green
} else {
    Write-Host "Application is not running" -ForegroundColor Red
}

# Check .NET Runtime version
Write-Host "`nChecking .NET Runtime version..." -ForegroundColor Yellow
dotnet --info 