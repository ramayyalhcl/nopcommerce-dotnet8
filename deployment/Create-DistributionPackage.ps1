<#
.SYNOPSIS
    Creates a distribution package with pre-built binaries for deployment

.DESCRIPTION
    1. Publishes the application
    2. Creates a "dist" folder with:
       - All binaries
       - Simple deployment script
       - Prerequisites scripts
    3. Package can be shared with others (no source code needed)

.EXAMPLE
    .\Create-DistributionPackage.ps1
    
    Creates: ..\dist\nopcommerce-iis-package\
#>

param(
    [string]$OutputFolder = "$PSScriptRoot\..\dist\nopcommerce-iis-package"
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
$RootDir = Split-Path -Parent $ScriptDir

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Creating Distribution Package" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Publish application
Write-Host "[1/3] Publishing application..." -ForegroundColor Green
$webProjectPath = Join-Path $RootDir "src\Presentation\Nop.Web"
$publishPath = Join-Path $RootDir "publish\nopcommerce-iis"

Push-Location $webProjectPath
try {
    if (Test-Path $publishPath) {
        Remove-Item -Path $publishPath -Recurse -Force
    }
    
    & dotnet publish -c Release -r win-x64 --self-contained false -o $publishPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Publish failed"
        exit 1
    }
    Write-Host "  Publish completed" -ForegroundColor Green
}
finally {
    Pop-Location
}
Write-Host ""

# Step 2: Create distribution folder
Write-Host "[2/3] Creating distribution package..." -ForegroundColor Green

if (Test-Path $OutputFolder) {
    Remove-Item -Path $OutputFolder -Recurse -Force
}
New-Item -Path $OutputFolder -ItemType Directory -Force | Out-Null

# Copy published files
Write-Host "  Copying application files..."
Copy-Item -Path "$publishPath\*" -Destination "$OutputFolder\app" -Recurse -Force

# Copy deployment scripts (the simple ones for pre-built package)
Write-Host "  Copying deployment scripts..."
Copy-Item -Path "$ScriptDir\Install-Prerequisites.ps1" -Destination $OutputFolder
Copy-Item -Path "$ScriptDir\Verify-Prerequisites.ps1" -Destination $OutputFolder

# Create the simple deployment script for distribution
$deployScript = @'
<#
.SYNOPSIS
    Deploys NopCommerce to IIS from pre-built package (no compilation needed)

.DESCRIPTION
    1. Verifies/installs prerequisites (IIS, .NET 8.0 Hosting Bundle)
    2. Creates IIS App Pool and Website
    3. Copies files to C:\inetpub\nopcommerce
    4. Sets permissions and starts the site

.PARAMETER SiteName
    Name of the IIS website (default: NopCommerce)

.PARAMETER AppPoolName
    Name of the application pool (default: NopCommerceAppPool)

.PARAMETER PhysicalPath
    Physical path where the site will be deployed (default: C:\inetpub\nopcommerce)

.PARAMETER Port
    Port number for the website (default: 80)

.EXAMPLE
    .\Deploy.ps1

.EXAMPLE
    .\Deploy.ps1 -SiteName "MyShop" -Port 8080
#>

[CmdletBinding()]
param(
    [string]$SiteName = "NopCommerce",
    [string]$AppPoolName = "NopCommerceAppPool",
    [string]$PhysicalPath = "C:\inetpub\nopcommerce",
    [int]$Port = 80,
    [switch]$SkipPrerequisites
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

# Check admin
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script requires Administrator privileges. Please run as Administrator."
    exit 1
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  NopCommerce Deployment (Pre-built Package)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Site Name:      $SiteName"
Write-Host "  App Pool:       $AppPoolName"
Write-Host "  Physical Path:  $PhysicalPath"
Write-Host "  Port:           $Port"
Write-Host ""

# Verify app folder exists
$appFolder = Join-Path $ScriptDir "app"
if (-not (Test-Path $appFolder)) {
    Write-Error "Application folder not found: $appFolder"
    exit 1
}

# Step 1: Prerequisites
if (-not $SkipPrerequisites) {
    Write-Host "[1/5] Verifying prerequisites..." -ForegroundColor Green
    & "$ScriptDir\Verify-Prerequisites.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Prerequisites missing." -ForegroundColor Red
        $install = Read-Host "Install prerequisites now? (Y/n)"
        if ($install -ne "n" -and $install -ne "N") {
            & "$ScriptDir\Install-Prerequisites.ps1"
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Prerequisite installation failed"
                exit 1
            }
        } else {
            Write-Error "Prerequisites required. Install manually or run with -SkipPrerequisites"
            exit 1
        }
    }
    Write-Host ""
}

# Step 2: Load IIS module
Write-Host "[2/5] Loading IIS module..." -ForegroundColor Green
Import-Module WebAdministration -ErrorAction Stop

# Step 3: Create App Pool
Write-Host "[3/5] Configuring Application Pool..." -ForegroundColor Green
$appPoolPath = "IIS:\AppPools\$AppPoolName"

if (-not (Test-Path $appPoolPath)) {
    Write-Host "  Creating app pool: $AppPoolName"
    New-WebAppPool -Name $AppPoolName | Out-Null
}

Set-ItemProperty -Path $appPoolPath -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path $appPoolPath -Name "managedPipelineMode" -Value "Integrated"

# Step 4: Deploy files
Write-Host "[4/5] Deploying files..." -ForegroundColor Green

# Stop existing site/pool
$sitePath = "IIS:\Sites\$SiteName"
if (Test-Path $sitePath) {
    Write-Host "  Stopping existing site..."
    Stop-WebSite -Name $SiteName -ErrorAction SilentlyContinue
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Remove-Website -Name $SiteName
}

# Create directory
if (-not (Test-Path $PhysicalPath)) {
    New-Item -Path $PhysicalPath -ItemType Directory -Force | Out-Null
}

# Copy files
Write-Host "  Copying files to $PhysicalPath..."
Copy-Item -Path "$appFolder\*" -Destination $PhysicalPath -Recurse -Force

# Create logs directory
$logsPath = Join-Path $PhysicalPath "logs"
if (-not (Test-Path $logsPath)) {
    New-Item -Path $logsPath -ItemType Directory -Force | Out-Null
}

# Step 5: Create Website & Set Permissions
Write-Host "[5/5] Configuring IIS Website..." -ForegroundColor Green
New-Website -Name $SiteName `
            -PhysicalPath $PhysicalPath `
            -ApplicationPool $AppPoolName `
            -Port $Port | Out-Null

# Set permissions
$appPoolIdentity = "IIS AppPool\$AppPoolName"
$writableDirs = @(
    $PhysicalPath,
    (Join-Path $PhysicalPath "App_Data"),
    (Join-Path $PhysicalPath "logs"),
    (Join-Path $PhysicalPath "wwwroot\images"),
    (Join-Path $PhysicalPath "wwwroot\files")
)

foreach ($dir in $writableDirs) {
    if (Test-Path $dir) {
        $acl = Get-Acl $dir
        $permission = New-Object System.Security.AccessControl.FileSystemAccessRule($appPoolIdentity, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($permission)
        Set-Acl -Path $dir -AclObject $acl
    }
}

# Start
Start-WebAppPool -Name $AppPoolName
Start-WebSite -Name $SiteName

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Site URL: http://localhost:$Port" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Browse to the URL above"
Write-Host "  2. Complete the installation wizard"
Write-Host "  3. Configure database connection"
Write-Host ""
'@

Set-Content -Path (Join-Path $OutputFolder "Deploy.ps1") -Value $deployScript -Encoding UTF8

# Create README
$readme = @"
# NopCommerce IIS Deployment Package

This package contains pre-built binaries and deployment scripts.
**No source code or compilation needed.**

## Contents

- **app/** - Pre-built NopCommerce application files
- **Deploy.ps1** - Main deployment script
- **Install-Prerequisites.ps1** - Installs IIS, .NET 8.0 Hosting Bundle
- **Verify-Prerequisites.ps1** - Checks prerequisites

## Requirements

- Windows Server 2016+ or Windows 10/11
- Administrator rights
- SQL Server (configured during installation wizard)

## Quick Start

### 1. Copy this folder to the target server

### 2. Run PowerShell as Administrator

### 3. Deploy:

``````powershell
cd <path-to-this-folder>
.\Deploy.ps1
``````

That's it! The script will:
- ✅ Check/install prerequisites
- ✅ Create IIS app pool and website
- ✅ Copy files to C:\inetpub\nopcommerce
- ✅ Set permissions
- ✅ Start the site

### 4. Open browser to http://localhost

Complete the installation wizard.

## Custom Configuration

### Change port:
``````powershell
.\Deploy.ps1 -Port 8080
``````

### Change site name:
``````powershell
.\Deploy.ps1 -SiteName "MyShop" -Port 8080
``````

### Skip prerequisites check:
``````powershell
.\Deploy.ps1 -SkipPrerequisites
``````

## Uninstall

1. Open IIS Manager
2. Delete the "NopCommerce" website
3. Delete the "NopCommerceAppPool" app pool
4. Delete C:\inetpub\nopcommerce folder

## Support

For issues, check:
- IIS logs: C:\inetpub\nopcommerce\logs
- Windows Event Viewer

## Package Info

Created: $(Get-Date -Format "yyyy-MM-dd HH:mm")
Version: 8.0
"@

Set-Content -Path (Join-Path $OutputFolder "README.md") -Value $readme -Encoding UTF8

Write-Host ""

# Step 3: Summary
Write-Host "[3/3] Package created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Distribution Package Ready" -ForegroundColor Green  
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Location: $OutputFolder" -ForegroundColor Yellow
Write-Host ""
$packageSize = (Get-ChildItem -Path $OutputFolder -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Package size: $([math]::Round($packageSize, 2)) MB" -ForegroundColor Yellow
Write-Host ""
Write-Host "To distribute:" -ForegroundColor Yellow
Write-Host "  1. Zip the folder: $OutputFolder"
Write-Host "  2. Share the zip with others"
Write-Host "  3. They unzip and run: Deploy.ps1"
Write-Host ""
Write-Host "To check into Git:" -ForegroundColor Yellow
Write-Host "  - DO NOT check in the 'dist' folder (too large)"
Write-Host "  - Check in this script: Create-DistributionPackage.ps1"
Write-Host "  - Others can run this script to create their own package"
Write-Host ""
