<#
.SYNOPSIS
    Publishes NopCommerce .NET 8.0 for manual IIS deployment.

.DESCRIPTION
    Creates a publish folder ready for manual deployment to IIS.
    Use this when you need to deploy to a remote server.

.PARAMETER SelfContained
    If specified, publishes as self-contained (includes .NET runtime)

.PARAMETER OutputPath
    Custom output path for published files

.EXAMPLE
    .\Publish-Only.ps1

.EXAMPLE
    .\Publish-Only.ps1 -SelfContained -OutputPath "C:\Deployments\nopcommerce"
#>

[CmdletBinding()]
param(
    [switch]$SelfContained,
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent $ScriptDir
$WebProjectPath = Join-Path $SolutionRoot "src\Presentation\Nop.Web"

# Prerequisite: .NET SDK
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
try {
    $dotnetVersion = & dotnet --version 2>&1
    if (-not $dotnetVersion) { throw "dotnet not found" }
    Write-Host "  [PASS] .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error ".NET SDK is required to publish. Install from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

if (-not (Test-Path $WebProjectPath)) {
    Write-Error "Web project not found at: $WebProjectPath"
    exit 1
}
Write-Host ""

if (-not $OutputPath) {
    $OutputPath = if ($SelfContained) { 
        Join-Path $SolutionRoot "publish\nopcommerce-iis-selfcontained" 
    } else { 
        Join-Path $SolutionRoot "publish\nopcommerce-iis" 
    }
}

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  NopCommerce .NET 8.0 Publish Script" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Self-Contained: $SelfContained"
Write-Host "  Output Path:    $OutputPath"
Write-Host ""

# Clean previous publish
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous publish folder..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Build and Publish
Write-Host "Publishing application (this may take a few minutes)..." -ForegroundColor Green

Push-Location $WebProjectPath
try {
    if ($SelfContained) {
        & dotnet publish -c Release -r win-x64 --self-contained true -o $OutputPath
    } else {
        & dotnet publish -c Release -r win-x64 --self-contained false -o $OutputPath
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Publish failed!"
        exit 1
    }
}
finally {
    Pop-Location
}

# Create required directories
$appDataPath = Join-Path $OutputPath "App_Data"
$logsPath = Join-Path $OutputPath "logs"

if (-not (Test-Path $appDataPath)) { New-Item -Path $appDataPath -ItemType Directory -Force | Out-Null }
if (-not (Test-Path $logsPath)) { New-Item -Path $logsPath -ItemType Directory -Force | Out-Null }

# Calculate size
$size = (Get-ChildItem -Path $OutputPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Publish Complete!" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Published to: $OutputPath" -ForegroundColor Green
Write-Host "Total size:   $([math]::Round($size, 2)) MB"
Write-Host ""
Write-Host "Deployment Steps:" -ForegroundColor Yellow
Write-Host "  1. Copy the contents of '$OutputPath' to your IIS server"
Write-Host "  2. Create an IIS website pointing to that folder"
Write-Host "  3. Set App Pool to 'No Managed Code'"
Write-Host "  4. Grant IIS App Pool identity Modify permissions on:"
Write-Host "     - App_Data folder"
Write-Host "     - logs folder"
Write-Host "     - wwwroot\images folder"
Write-Host ""
if (-not $SelfContained) {
    Write-Host "IMPORTANT: Target server needs .NET 8.0 Hosting Bundle installed!" -ForegroundColor Red
    Write-Host "Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Red
}
Write-Host ""
