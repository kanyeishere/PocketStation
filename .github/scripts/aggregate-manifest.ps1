<#
.SYNOPSIS
    Aggregates plugin manifest entries from multiple repositories into a single pluginmaster.json.

.DESCRIPTION
    Downloads the latest single-plugin entry JSON from each repository's GitHub release,
    merges them into one array, and uploads the result as pluginmaster.json to the
    PocketStation repo's latest release.

.PARAMETER PocketStationRepo
    The full "owner/repo" string for the PocketStation repository (where pluginmaster.json lives).

.PARAMETER PocketRecorderRepo
    The full "owner/repo" string for the PocketRecorder repository.

.PARAMETER GithubToken
    A GitHub token with contents:write permission on the PocketStation repo.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PocketStationRepo,

    [Parameter(Mandatory)]
    [string]$PocketRecorderRepo,

    [Parameter(Mandatory)]
    [string]$GithubToken
)

$ErrorActionPreference = "Stop"

function Get-LatestReleaseAsset {
    param([string]$Repo, [string]$AssetName, [string]$Token)

    $url = "https://github.com/$Repo/releases/latest/download/$AssetName"
    Write-Host "  Downloading $AssetName from $Repo ..."
    Write-Host "  URL: $url"

    $headers = @{ "User-Agent" = "aggregate-manifest-action" }
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }

    try {
        Invoke-WebRequest -Uri $url -OutFile $AssetName -Headers $headers -ErrorAction Stop
        Write-Host "  Downloaded $AssetName successfully."
        return $true
    }
    catch {
        Write-Host "  WARNING: Could not download $AssetName from $Repo`: $($_.Exception.Message)"
        return $false
    }
}

Write-Host "=== Aggregating plugin manifests ==="

$entries = @()

# --- PocketStation entry ---
$pocketStationOk = Get-LatestReleaseAsset -Repo $PocketStationRepo -AssetName "pocketstation-entry.json" -Token $GithubToken
if ($pocketStationOk) {
    $psEntry = Get-Content -Raw "pocketstation-entry.json" | ConvertFrom-Json
    $entries += $psEntry
    Write-Host "  Added PocketStation entry (v$($psEntry.AssemblyVersion))"
} else {
    Write-Host "  WARNING: PocketStation entry not found. The aggregated manifest will only contain other plugins."
}

# --- PocketRecorder entry ---
$pocketRecorderOk = Get-LatestReleaseAsset -Repo $PocketRecorderRepo -AssetName "pocketrecorder-entry.json" -Token $GithubToken
if ($pocketRecorderOk) {
    $prEntry = Get-Content -Raw "pocketrecorder-entry.json" | ConvertFrom-Json
    $entries += $prEntry
    Write-Host "  Added PocketRecorder entry (v$($prEntry.AssemblyVersion))"
} else {
    Write-Host "  WARNING: PocketRecorder entry not found. The aggregated manifest will only contain other plugins."
}

# --- Merge and output ---
if ($entries.Count -eq 0) {
    throw "No plugin entries could be retrieved. Aborting."
}

$entries | ConvertTo-Json -Depth 10 -AsArray | Set-Content "pluginmaster.json"
Write-Host "=== pluginmaster.json ==="
Get-Content "pluginmaster.json"

Write-Host "=== Aggregation complete: $($entries.Count) plugin(s) ==="
