# Script to restore repository to latest GitHub remote branch and commit
param(
    [Parameter(Mandatory=$false)]
    [switch]$ShowBranches = $false  # Option to show available branches
)

Write-Host "Starting repository restoration process..." -ForegroundColor Cyan

# Function to handle errors
function Handle-Error {
    param($ErrorMessage)
    Write-Host "Error: $ErrorMessage" -ForegroundColor Red
    exit 1
}

try {
    # Step 1: Check remote URL
    Write-Host "Checking GitHub remote..." -ForegroundColor Yellow
    $remoteUrl = git config --get remote.origin.url
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to get remote URL" }
    Write-Host "Remote URL: $remoteUrl" -ForegroundColor Cyan

    # Step 2: Fetch latest changes from GitHub
    Write-Host "Fetching latest changes from GitHub..." -ForegroundColor Yellow
    git fetch --all --prune
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to fetch from GitHub" }

    # Step 3: Get all remote branches and find latest
    Write-Host "Getting remote branches from GitHub..." -ForegroundColor Yellow
    $branches = git for-each-ref --sort=-committerdate refs/remotes/origin --format='%(refname:short)' | Where-Object { $_ -notmatch 'HEAD$' }
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to get GitHub remote branches" }

    # Convert branches to array and get latest
    $branchArray = @($branches)
    if ($branchArray.Count -eq 0) { Handle-Error "No remote branches found" }
    $latestBranch = $branchArray[0]

    if ($ShowBranches) {
        Write-Host "`nAvailable GitHub branches (sorted by latest commit):" -ForegroundColor Cyan
        $branchArray | ForEach-Object { 
            $commitInfo = git log -1 --format="%cr" $_
            Write-Host "  $_ (last updated: $commitInfo)" 
        }
        Write-Host "`nLatest branch is: $latestBranch`n" -ForegroundColor Green
        exit 0
    }

    Write-Host "Latest GitHub branch detected: $latestBranch" -ForegroundColor Cyan

    # Step 4: Clean untracked files
    Write-Host "Cleaning untracked files..." -ForegroundColor Yellow
    git clean -fd
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to clean untracked files" }

    # Step 5: Hard reset to latest GitHub branch
    Write-Host "Resetting to latest GitHub version ($latestBranch)..." -ForegroundColor Yellow
    git reset --hard $latestBranch
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to reset to $latestBranch" }

    # Step 6: Clean again after reset
    Write-Host "Final cleanup..." -ForegroundColor Yellow
    git clean -fd
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed during final cleanup" }

    # Step 7: Verify status
    Write-Host "Verifying repository status..." -ForegroundColor Yellow
    $status = git status
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to get repository status" }

    # Get latest commit info
    $commitInfo = git log -1 --format="%h - %s (%cr)"
    if ($LASTEXITCODE -ne 0) { Handle-Error "Failed to get commit information" }

    # Get remote commit info
    $remoteCommit = git rev-parse $latestBranch
    $localCommit = git rev-parse HEAD
    
    # Check if working tree is clean and commits match
    if ($status -match "working tree clean" -and $remoteCommit -eq $localCommit) {
        Write-Host "`nRepository successfully restored to latest GitHub state!" -ForegroundColor Green
        Write-Host "Remote URL: $remoteUrl" -ForegroundColor Green
        Write-Host "Branch: $latestBranch" -ForegroundColor Green
        Write-Host "Commit: $commitInfo" -ForegroundColor Green
        Write-Host "Working tree is clean and matches GitHub remote." -ForegroundColor Green
    } else {
        Handle-Error "Working tree is not clean or doesn't match GitHub remote"
    }

} catch {
    Handle-Error $_.Exception.Message
} 