<#
.SYNOPSIS
    Single script: Prerequisites first, then full IIS deployment (same as Deploy-IIS.ps1).

.DESCRIPTION
    1. Runs Install-Prerequisites.ps1 (IIS, .NET 8 Hosting Bundle, URL Rewrite) if needed.
    2. Calls Deploy-IIS.ps1 with -SkipPrerequisiteCheck to publish and deploy to IIS.

    Uses the same publish/deploy flow as Deploy-IIS.ps1 (publish to repo publish folder, then IIS).

.PARAMETER SiteName, AppPoolName, PhysicalPath, Port, HostName, SelfContained, SkipPublish
    Passed through to Deploy-IIS.ps1.

.PARAMETER SkipPrerequisites
    Skip prerequisite install; only run Deploy-IIS.ps1.

.EXAMPLE
    .\Deploy-All.ps1
.EXAMPLE
    .\Deploy-All.ps1 -SiteName "NopCommerce" -Port 80
#>

[CmdletBinding()]
param(
    [string]$SiteName = "NopCommerce",
    [string]$AppPoolName = "NopCommerceAppPool",
    [string]$PhysicalPath = "C:\inetpub\nopcommerce",
    [int]$Port = 80,
    [string]$HostName = "",
    [switch]$SelfContained,
    [switch]$SkipPublish,
    [switch]$SkipPrerequisites
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
if (-not $ScriptDir) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path }

$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Run as Administrator."
    exit 1
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Deploy-All: Prerequisites + IIS + Nop.Web" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

if (-not $SkipPrerequisites) {
    Write-Host "Step 1: Prerequisites..." -ForegroundColor Green
    & "$ScriptDir\Verify-Prerequisites.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Installing prerequisites..." -ForegroundColor Yellow
        & "$ScriptDir\Install-Prerequisites.ps1"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Prerequisites failed. Use -SkipPrerequisites to skip."
            exit 1
        }
    }
    Write-Host ""
}

Write-Host "Step 2: Publish and deploy to IIS (Deploy-IIS.ps1)..." -ForegroundColor Green
$deployParams = @{
    SiteName                = $SiteName
    AppPoolName             = $AppPoolName
    PhysicalPath            = $PhysicalPath
    Port                    = $Port
    SkipPrerequisiteCheck   = $true
}
if ($HostName) { $deployParams.HostName = $HostName }
if ($SelfContained) { $deployParams.SelfContained = $true }
if ($SkipPublish) { $deployParams.SkipPublish = $true }

& "$ScriptDir\Deploy-IIS.ps1" @deployParams
exit $LASTEXITCODE
