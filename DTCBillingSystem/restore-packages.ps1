# Restore NuGet packages script
Write-Host "Starting package restoration process..." -ForegroundColor Green

# Define package versions
$materialDesignVersion = "4.9.0"
$microsoftExtensionsVersion = "6.0.1"

# Function to add package with error handling
function Add-PackageWithRetry {
    param (
        [string]$project,
        [string]$package,
        [string]$version
    )
    
    Write-Host "Installing $package version $version..." -ForegroundColor Yellow
    $maxRetries = 3
    $retryCount = 0
    
    while ($retryCount -lt $maxRetries) {
        try {
            dotnet add $project package $package -v $version
            if ($?) {
                Write-Host "Successfully installed $package" -ForegroundColor Green
                return $true
            }
        }
        catch {
            $retryCount++
            if ($retryCount -lt $maxRetries) {
                Write-Host "Retry $retryCount of $maxRetries for $package" -ForegroundColor Yellow
                Start-Sleep -Seconds 2
            }
        }
    }
    
    Write-Host "Failed to install $package after $maxRetries attempts" -ForegroundColor Red
    return $false
}

# Restore all packages first
Write-Host "Restoring existing packages..." -ForegroundColor Yellow
dotnet restore
if (-not $?) {
    Write-Host "Failed to restore existing packages" -ForegroundColor Red
    exit 1
}

# UI Project packages
$uiProject = "DTCBillingSystem.UI/DTCBillingSystem.UI.csproj"

# Install Material Design packages
if (-not (Add-PackageWithRetry $uiProject "MaterialDesignThemes" $materialDesignVersion)) { exit 1 }
if (-not (Add-PackageWithRetry $uiProject "MaterialDesignColors" $materialDesignVersion)) { exit 1 }

# Install Microsoft Extensions packages
if (-not (Add-PackageWithRetry $uiProject "Microsoft.Extensions.DependencyInjection" $microsoftExtensionsVersion)) { exit 1 }
if (-not (Add-PackageWithRetry $uiProject "Microsoft.Extensions.Configuration" $microsoftExtensionsVersion)) { exit 1 }
if (-not (Add-PackageWithRetry $uiProject "Microsoft.Extensions.Configuration.Json" $microsoftExtensionsVersion)) { exit 1 }

# Final restore to ensure everything is in sync
Write-Host "Performing final package restore..." -ForegroundColor Yellow
dotnet restore
if (-not $?) {
    Write-Host "Final restore failed" -ForegroundColor Red
    exit 1
}

Write-Host "Package restoration completed successfully!" -ForegroundColor Green 