# Script to create a new branch, commit changes, and push to GitHub
param(
    [Parameter(Mandatory=$false)]
    [switch]$ShowCurrentBranch = $false # Show current branch info
)

# Function to handle errors
function Handle-Error {
    param($ErrorMessage)
    Write-Host "Error: $ErrorMessage" -ForegroundColor Red
    exit 1
}

# Function to sanitize branch name
function Get-SanitizedName {
    param($Name)
    # Convert to lowercase and replace spaces/special chars with hyphens
    $sanitized = $Name.ToLower() -replace '[^a-zA-Z0-9-]', '-'
    # Remove multiple consecutive hyphens
    $sanitized = $sanitized -replace '-+', '-'
    # Remove leading/trailing hyphens
    $sanitized = $sanitized.Trim('-')
    return $sanitized
}

# Function to get current date in YYYYMMDD format
function Get-FormattedDate {
    return Get-Date -Format "yyyyMMdd"
}

# Function to prompt user with choices
function Get-UserChoice {
    param(
        [string]$Title,
        [array]$Choices
    )
    Write-Host "`n$Title" -ForegroundColor Cyan
    for ($i = 0; $i -lt $Choices.Length; $i++) {
        Write-Host "$($i + 1)) $($Choices[$i])" -ForegroundColor Yellow
    }
    
    do {
        $choice = Read-Host "`nEnter number (1-$($Choices.Length))"
        $valid = $choice -match '^\d+$' -and [int]$choice -ge 1 -and [int]$choice -le $Choices.Length
        if (-not $valid) {
            Write-Host "Invalid choice. Please try again." -ForegroundColor Red
        }
    } while (-not $valid)
    
    return $Choices[[int]$choice - 1]
}

try {
    # Check if git is installed
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Handle-Error "Git is not installed or not in PATH"
    }

    # Get current branch info if requested
    if ($ShowCurrentBranch) {
        Write-Host "`nCurrent branch information:" -ForegroundColor Cyan
        $currentBranch = git rev-parse --abbrev-ref HEAD
        $lastCommit = git log -1 --format="%h - %s (%cr)"
        Write-Host "Branch: $currentBranch" -ForegroundColor Yellow
        Write-Host "Last commit: $lastCommit" -ForegroundColor Yellow
        exit 0
    }

    Clear-Host
    Write-Host "=== GitHub Branch Creation Wizard ===" -ForegroundColor Cyan

    # Step 1: Choose branch type
    $branchTypes = @(
        "feature - New feature or enhancement",
        "bugfix - Bug fix",
        "hotfix - Critical fix for production",
        "release - Release preparation",
        "docs - Documentation updates",
        "refactor - Code refactoring"
    )
    $branchType = Get-UserChoice "What type of branch are you creating?" $branchTypes
    $BranchPrefix = $branchType.Split(' ')[0]

    # Step 2: Get branch description
    Write-Host "`nEnter a brief description of your changes" -ForegroundColor Cyan
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "- add-user-authentication" -ForegroundColor Yellow
    Write-Host "- fix-login-issue" -ForegroundColor Yellow
    Write-Host "- update-billing-module" -ForegroundColor Yellow
    
    do {
        $BranchDescription = Read-Host "`nDescription"
        if ([string]::IsNullOrWhiteSpace($BranchDescription)) {
            Write-Host "Description cannot be empty. Please try again." -ForegroundColor Red
        }
    } while ([string]::IsNullOrWhiteSpace($BranchDescription))

    # Create branch name with proper convention
    $date = Get-FormattedDate
    $sanitizedDesc = Get-SanitizedName $BranchDescription
    $newBranchName = "${BranchPrefix}/${date}-${sanitizedDesc}"

    Write-Host "`nCreating branch: $newBranchName" -ForegroundColor Cyan

    # Check if branch already exists
    $existingBranches = git branch --list $newBranchName
    if ($existingBranches) {
        Handle-Error "Branch '$newBranchName' already exists"
    }

    # Create and checkout new branch
    git checkout -b $newBranchName
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to create branch" }

    # Check if there are any changes to commit
    $status = git status --porcelain
    if ([string]::IsNullOrWhiteSpace($status)) {
        Write-Host "`nNo changes to commit" -ForegroundColor Yellow
        Write-Host "Branch '$newBranchName' has been created. Make your changes and run this script again to commit." -ForegroundColor Green
        exit 0
    }

    # Show changes to be committed
    Write-Host "`nChanges to be committed:" -ForegroundColor Cyan
    git status --short

    # Get commit message
    Write-Host "`nEnter a brief description of your changes for the commit message" -ForegroundColor Cyan
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "- Added user authentication system" -ForegroundColor Yellow
    Write-Host "- Fixed login validation bug" -ForegroundColor Yellow
    Write-Host "- Updated billing calculation logic" -ForegroundColor Yellow
    
    do {
        $CommitMessage = Read-Host "`nCommit message"
        if ([string]::IsNullOrWhiteSpace($CommitMessage)) {
            Write-Host "Commit message cannot be empty. Please try again." -ForegroundColor Red
        }
    } while ([string]::IsNullOrWhiteSpace($CommitMessage))

    # Format commit message with conventional commit style
    $prefix = $BranchPrefix
    $message = $CommitMessage
    $formattedMessage = "$prefix`: $message"
    if ($formattedMessage.Length -gt 50) {
        $truncated = $formattedMessage.Substring(0, 47)
        $formattedMessage = "$truncated..."
    }

    # Confirm actions
    Write-Host "`nReview your changes:" -ForegroundColor Cyan
    Write-Host "Branch name: $newBranchName" -ForegroundColor Yellow
    Write-Host "Commit message: $formattedMessage" -ForegroundColor Yellow
    
    $confirm = Read-Host "`nProceed with commit and push? (Y/N)"
    if ($confirm -notmatch '^[Yy]') {
        Write-Host "`nOperation cancelled. Branch has been created but changes were not committed." -ForegroundColor Yellow
        exit 0
    }

    # Stage and commit changes
    Write-Host "`nStaging changes..." -ForegroundColor Yellow
    git add .
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to stage changes" }

    Write-Host "Committing changes..." -ForegroundColor Yellow
    git commit -m $formattedMessage
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to commit changes" }

    Write-Host "Pushing to GitHub..." -ForegroundColor Yellow
    git push -u origin $newBranchName
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to push to GitHub" }

    # Show success message
    Write-Host "`nSuccess! Your changes have been pushed to GitHub:" -ForegroundColor Green
    Write-Host "Branch: $newBranchName" -ForegroundColor Green
    Write-Host "Commit: $formattedMessage" -ForegroundColor Green
    Write-Host "Remote: origin/$newBranchName" -ForegroundColor Green

} catch {
    Handle-Error $_.Exception.Message
} 