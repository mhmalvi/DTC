# Check and Set PowerShell Execution Policy
Write-Host "Checking PowerShell Execution Policy..." -ForegroundColor Yellow

# Get current execution policy
$currentPolicy = Get-ExecutionPolicy
Write-Host "Current Execution Policy: $currentPolicy" -ForegroundColor Cyan

# Function to test if running as administrator
function Test-Administrator {
    $user = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($user)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Administrator)) {
    Write-Host "`nThis script needs to be run as Administrator to change execution policy." -ForegroundColor Red
    Write-Host "Please try one of the following:" -ForegroundColor Yellow
    Write-Host "1. Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Write-Host "2. Or run this command in an admin PowerShell:" -ForegroundColor Yellow
    Write-Host "   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force" -ForegroundColor Green
    exit 1
}

# If we're running as admin, set the policy
try {
    Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
    $newPolicy = Get-ExecutionPolicy
    Write-Host "`nExecution Policy has been updated:" -ForegroundColor Green
    Write-Host "New Execution Policy: $newPolicy" -ForegroundColor Green
    
    Write-Host "`nYou can now run PowerShell scripts on your system." -ForegroundColor Green
    Write-Host "Try running your build script again: ./build.ps1" -ForegroundColor Yellow
} catch {
    Write-Host "`nFailed to set execution policy: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
} 