#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

function Write-Message {
    param($msg, $color = "White")
    Write-Host $msg -ForegroundColor $color
}

function Get-GitStatus {
    Write-Message "Starting Git Status Check..." "Cyan"
    
    try {
        # Check git installation
        $null = git --version
    } catch {
        Write-Message "Git is not installed!" "Red"
        return
    }

    # Check repository
    if (-not (Test-Path .git)) {
        Write-Message "Not a git repository!" "Red"
        return
    }

    # Check remote
    try {
        $remoteUrl = git config --get remote.origin.url
        if ($remoteUrl) {
            Write-Message "Remote URL: $remoteUrl" "Cyan"
            
            try {
                $null = git ls-remote --quiet
                Write-Message "Connected to remote" "Green"
                
                try {
                    Write-Message "Fetching updates..." "Cyan"
                    git fetch --all
                } catch {
                    Write-Message "Error fetching: $_" "Yellow"
                }
            } catch {
                Write-Message "Cannot connect to remote" "Red"
            }
        } else {
            Write-Message "No remote configured" "Yellow"
        }
    } catch {
        Write-Message "Error checking remote: $_" "Yellow"
    }

    # Check branch
    try {
        $currentBranch = git rev-parse --abbrev-ref HEAD
        Write-Message "Current branch: $currentBranch" "Cyan"
        
        try {
            $trackingBranch = git rev-parse --abbrev-ref --symbolic-full-name "@{u}"
            Write-Message "Tracking: $trackingBranch" "Cyan"
            
            try {
                $unpushedCommits = git log "@{u}.." --oneline
                if ($unpushedCommits) {
                    $commitCount = ($unpushedCommits | Measure-Object -Line).Lines
                    Write-Message "You have $commitCount unpushed commit(s)" "Yellow"
                } else {
                    Write-Message "No unpushed commits" "Green"
                }

                $behindCount = git rev-list "HEAD..@{u}" --count
                if ($behindCount -gt 0) {
                    Write-Message "Branch is behind by $behindCount commit(s)" "Yellow"
                } else {
                    Write-Message "Branch is up to date" "Green"
                }
            } catch {
                Write-Message "Error checking commits: $_" "Yellow"
            }
        } catch {
            Write-Message "Branch is not tracking remote" "Yellow"
        }
    } catch {
        Write-Message "Error getting branch info: $_" "Red"
    }

    # Check stash
    try {
        $stashCount = (git stash list | Measure-Object -Line).Lines
        if ($stashCount -gt 0) {
            Write-Message "You have $stashCount stashed change(s)" "Yellow"
        }
    } catch {
        Write-Message "Error checking stash: $_" "Yellow"
    }

    # Check status
    try {
        $status = git status --porcelain
        if ($status) {
            Write-Message "You have changes:" "Yellow"
            $modified = ($status | Select-String "^.M").Count
            $added = ($status | Select-String "^.A").Count
            $deleted = ($status | Select-String "^.D").Count
            $untracked = ($status | Select-String "^\?\?").Count
            Write-Host "Modified: $modified | Added: $added | Deleted: $deleted | Untracked: $untracked"
            Write-Host $status
        } else {
            Write-Message "Working directory is clean" "Green"
        }
    } catch {
        Write-Message "Error checking status: $_" "Red"
    }

    # Check conflicts
    try {
        $conflictedFiles = git diff --name-only --diff-filter=U
        if ($conflictedFiles) {
            Write-Message "You have merge conflicts:" "Yellow"
            Write-Host $conflictedFiles
        }
    } catch {
        Write-Message "Error checking conflicts: $_" "Yellow"
    }

    # Show last commit
    try {
        $lastCommit = git log -1 --pretty=format:"Hash: %h%nAuthor: %an%nDate: %cd%nMessage: %s" --date=relative
        if ($lastCommit) {
            Write-Message "Last commit:" "Cyan"
            Write-Host $lastCommit
        }
    } catch {
        Write-Message "Error getting last commit: $_" "Yellow"
    }

    Write-Message "Status check completed" "Green"
}

try {
    Get-GitStatus
} catch {
    Write-Message "Fatal error: $_" "Red"
    exit 1
} 