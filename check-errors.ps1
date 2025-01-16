# Enhanced Error Checking Script
Write-Host "Starting comprehensive system check..." -ForegroundColor Green

# Function to check directory existence
function Test-ProjectDirectory {
    param (
        [string]$path
    )
    if (-not (Test-Path $path)) {
        Write-Host "Warning: Directory not found: $path" -ForegroundColor Yellow
        return $false
    }
    return $true
}

# Check project structure
Write-Host "`nChecking project structure..." -ForegroundColor Yellow
$directories = @(
    "DTCBillingSystem.UI",
    "DTCBillingSystem.Core",
    "DTCBillingSystem.Infrastructure"
)

foreach ($dir in $directories) {
    if (Test-ProjectDirectory $dir) {
        Write-Host "✓ Found $dir" -ForegroundColor Green
    }
}

# Check .NET SDK and Runtime
Write-Host "`nChecking .NET installation..." -ForegroundColor Yellow
try {
    $dotnetInfo = dotnet --info
    Write-Host "✓ .NET SDK/Runtime information:" -ForegroundColor Green
    $dotnetInfo | Select-String "Version:"
    $dotnetInfo | Select-String "OS Version:"
} catch {
    Write-Host "× Error checking .NET installation" -ForegroundColor Red
}

# Get recent application errors
Write-Host "`nChecking for recent application errors..." -ForegroundColor Yellow
$errors = Get-WinEvent -FilterHashtable @{
    LogName = 'Application'
    Level = 2  # Error level
    StartTime = (Get-Date).AddMinutes(-30)  # Last 30 minutes
} -ErrorAction SilentlyContinue | Where-Object { $_.ProviderName -like "*DTCBillingSystem*" }

if ($errors) {
    Write-Host "Found recent application errors:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "`nError Details:" -ForegroundColor Red
        Write-Host "Time: $($error.TimeCreated)" -ForegroundColor Red
        Write-Host "Source: $($error.ProviderName)" -ForegroundColor Red
        Write-Host "Event ID: $($error.Id)" -ForegroundColor Red
        Write-Host "Message: $($error.Message)" -ForegroundColor Red
        Write-Host "---" -ForegroundColor Red
    }
} else {
    Write-Host "✓ No recent application errors found" -ForegroundColor Green
}

# Check process status
Write-Host "`nChecking application process..." -ForegroundColor Yellow
$process = Get-Process "DTCBillingSystem.UI" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "✓ Application is running" -ForegroundColor Green
    Write-Host "  Process ID: $($process.Id)" -ForegroundColor Green
    Write-Host "  Memory Usage: $([math]::Round($process.WorkingSet64 / 1MB, 2)) MB" -ForegroundColor Green
    Write-Host "  CPU Time: $($process.CPU)" -ForegroundColor Green
    Write-Host "  Start Time: $($process.StartTime)" -ForegroundColor Green
} else {
    Write-Host "× Application is not running" -ForegroundColor Yellow
}

# Check build outputs
Write-Host "`nChecking build outputs..." -ForegroundColor Yellow
$buildPaths = @(
    ".\DTCBillingSystem.UI\bin\Debug\net6.0-windows",
    ".\DTCBillingSystem.Core\bin\Debug\net6.0",
    ".\DTCBillingSystem.Infrastructure\bin\Debug\net6.0"
)

foreach ($path in $buildPaths) {
    if (Test-Path $path) {
        Write-Host "✓ Found build output: $path" -ForegroundColor Green
        $dlls = Get-ChildItem $path -Filter *.dll
        Write-Host "  Found $($dlls.Count) assemblies" -ForegroundColor Green
    } else {
        Write-Host "× Missing build output: $path" -ForegroundColor Red
    }
}

Write-Host "`nSystem check completed!" -ForegroundColor Green 