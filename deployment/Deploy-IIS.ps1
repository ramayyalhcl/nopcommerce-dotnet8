<#
.SYNOPSIS
    Deploys NopCommerce .NET 8.0 application to IIS.

.DESCRIPTION
    This script:
    1. Publishes the application in Release mode
    2. Creates IIS Application Pool (if not exists)
    3. Creates IIS Website (if not exists)
    4. Deploys published files to target path
    5. Sets appropriate folder permissions
    6. Starts the website

.PARAMETER SiteName
    Name of the IIS website (default: NopCommerce)

.PARAMETER AppPoolName
    Name of the application pool (default: NopCommerceAppPool)

.PARAMETER PhysicalPath
    Physical path where the site will be deployed (default: C:\inetpub\nopcommerce)

.PARAMETER Port
    Port number for the website (default: 80)

.PARAMETER HostName
    Optional hostname/domain for the site binding

.PARAMETER SelfContained
    If specified, publishes as self-contained (no runtime required on server)

.PARAMETER SkipPublish
    Skip the publish step (use existing publish folder)

.EXAMPLE
    .\Deploy-IIS.ps1 -SiteName "NopCommerce" -Port 8080

.EXAMPLE
    .\Deploy-IIS.ps1 -SiteName "NopCommerce" -HostName "shop.example.com" -SelfContained

.NOTES
    Requires:
    - Administrator privileges
    - IIS with ASP.NET Core Module installed
    - .NET 8.0 Hosting Bundle (unless using -SelfContained)
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
    [switch]$SkipPrerequisiteCheck,
    [switch]$PrerequisitesOnly
)

# Ensure running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script requires Administrator privileges. Please run as Administrator."
    exit 1
}

# Script configuration
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent $ScriptDir
$WebProjectPath = Join-Path $SolutionRoot "src\Presentation\Nop.Web"
$PublishPath = if ($SelfContained) { 
    Join-Path $SolutionRoot "publish\nopcommerce-iis-selfcontained" 
} else { 
    Join-Path $SolutionRoot "publish\nopcommerce-iis" 
}

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  NopCommerce .NET 8.0 IIS Deployment Script" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Site Name:      $SiteName"
Write-Host "  App Pool:       $AppPoolName"
Write-Host "  Physical Path:  $PhysicalPath"
Write-Host "  Port:           $Port"
Write-Host "  Hostname:       $(if ($HostName) { $HostName } else { '(any)' })"
Write-Host "  Self-Contained: $SelfContained"
Write-Host ""

# Step 0: Prerequisite Verification (unless using SelfContained or skipped)
if ($PrerequisitesOnly) {
    Write-Host "[0/1] Running prerequisite check only..." -ForegroundColor Green
    & "$ScriptDir\Verify-Prerequisites.ps1"
    exit $LASTEXITCODE
}

if (-not $SkipPrerequisiteCheck -and -not $SelfContained) {
    Write-Host "[0/7] Verifying prerequisites..." -ForegroundColor Green
    & "$ScriptDir\Verify-Prerequisites.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Prerequisite check failed." -ForegroundColor Red
        $install = Read-Host "Run Install-Prerequisites.ps1 to install missing components automatically? (Y/n)"
        if ($install -ne "n" -and $install -ne "N") {
            Write-Host ""
            & "$ScriptDir\Install-Prerequisites.ps1"
            if ($LASTEXITCODE -eq 0) {
                Write-Host ""
                Write-Host "Prerequisites installed. Continuing deployment..." -ForegroundColor Green
                Write-Host ""
            } else {
                Write-Error "Prerequisite installation failed or incomplete. Resolve the issues above, or use -SkipPrerequisiteCheck to bypass."
                exit 1
            }
        } else {
            Write-Error "Deployment aborted. Run Install-Prerequisites.ps1 manually, or use -SkipPrerequisiteCheck to bypass."
            exit 1
        }
    }
    Write-Host ""
}

if ($SkipPrerequisiteCheck) {
    Write-Host "[0/7] Skipping prerequisite check (-SkipPrerequisiteCheck specified)" -ForegroundColor Yellow
}

if ($SelfContained) {
    Write-Host "[0/7] Skipping prerequisite check (self-contained deployment - no Hosting Bundle required)" -ForegroundColor Yellow
}

# Step 1: Publish Application
if (-not $SkipPublish) {
    Write-Host "[1/6] Publishing application..." -ForegroundColor Green
    
    Push-Location $WebProjectPath
    try {
        # Clean previous publish
        if (Test-Path $PublishPath) {
            Write-Host "  Cleaning previous publish folder..."
            Remove-Item -Path $PublishPath -Recurse -Force
        }
        
        # Publish with explicit output path (avoids profile path resolution issues)
        Write-Host "  Running dotnet publish (this may take a few minutes)..."
        $selfContainedArg = if ($SelfContained) { "true" } else { "false" }
        $publishResult = & dotnet publish -c Release -r win-x64 --self-contained $selfContainedArg -o $PublishPath 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Publish failed: $publishResult"
            exit 1
        }
        Write-Host "  Publish completed successfully." -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host "[1/6] Skipping publish (using existing files)..." -ForegroundColor Yellow
}

# Verify publish output exists
if (-not (Test-Path (Join-Path $PublishPath "Nop.Web.dll"))) {
    Write-Error "Published files not found at: $PublishPath"
    exit 1
}

# Step 2: Import IIS Module
Write-Host "[2/6] Loading IIS PowerShell module..." -ForegroundColor Green
Import-Module WebAdministration -ErrorAction Stop

# Step 3: Create Application Pool
Write-Host "[3/6] Configuring Application Pool..." -ForegroundColor Green
$appPoolPath = "IIS:\AppPools\$AppPoolName"

if (-not (Test-Path $appPoolPath)) {
    Write-Host "  Creating application pool: $AppPoolName"
    New-WebAppPool -Name $AppPoolName | Out-Null
} else {
    Write-Host "  Application pool already exists: $AppPoolName"
}

# Configure App Pool for .NET 8.0 / ASP.NET Core (No Managed Code - required for .NET 8)
Write-Host "  Configuring app pool for .NET 8.0 (No Managed Code)..."
Set-ItemProperty -Path $appPoolPath -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path $appPoolPath -Name "managedPipelineMode" -Value "Integrated"
Set-ItemProperty -Path $appPoolPath -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path $appPoolPath -Name "processModel.idleTimeout" -Value "00:00:00"
Set-ItemProperty -Path $appPoolPath -Name "recycling.periodicRestart.time" -Value "00:00:00"

# Step 4: Create Physical Directory
Write-Host "[4/6] Setting up deployment directory..." -ForegroundColor Green
if (-not (Test-Path $PhysicalPath)) {
    Write-Host "  Creating directory: $PhysicalPath"
    New-Item -Path $PhysicalPath -ItemType Directory -Force | Out-Null
}

# Stop, remove existing site (and free binding) so we redeploy with latest code
$sitePath = "IIS:\Sites\$SiteName"
if (Test-Path $sitePath) {
    Write-Host "  Stopping and removing existing site for clean redeploy..."
    Stop-WebSite -Name $SiteName -ErrorAction SilentlyContinue
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Remove-Website -Name $SiteName
}

# Copy published files
Write-Host "  Copying published files..."
Copy-Item -Path "$PublishPath\*" -Destination $PhysicalPath -Recurse -Force

# Create logs directory
$logsPath = Join-Path $PhysicalPath "logs"
if (-not (Test-Path $logsPath)) {
    New-Item -Path $logsPath -ItemType Directory -Force | Out-Null
}

# Step 5: Create Website (always create fresh after remove)
Write-Host "[5/6] Configuring IIS Website..." -ForegroundColor Green
Write-Host "  Creating website: $SiteName"
New-Website -Name $SiteName `
            -PhysicalPath $PhysicalPath `
            -ApplicationPool $AppPoolName `
            -Port $Port `
            -HostHeader $HostName | Out-Null

# Step 6: Set Permissions
Write-Host "[6/6] Setting folder permissions..." -ForegroundColor Green

# Get the App Pool identity
$appPoolIdentity = "IIS AppPool\$AppPoolName"

# Directories that need write access
$writableDirs = @(
    $PhysicalPath,
    (Join-Path $PhysicalPath "App_Data"),
    (Join-Path $PhysicalPath "logs"),
    (Join-Path $PhysicalPath "wwwroot\images"),
    (Join-Path $PhysicalPath "wwwroot\files")
)

foreach ($dir in $writableDirs) {
    if (Test-Path $dir) {
        Write-Host "  Setting permissions on: $dir"
        $acl = Get-Acl $dir
        $permission = New-Object System.Security.AccessControl.FileSystemAccessRule(
            $appPoolIdentity,
            "Modify",
            "ContainerInherit,ObjectInherit",
            "None",
            "Allow"
        )
        $acl.SetAccessRule($permission)
        Set-Acl -Path $dir -AclObject $acl
    }
}

# Start the site
Write-Host ""
Write-Host "Starting website..." -ForegroundColor Green
Start-WebAppPool -Name $AppPoolName
try {
    Start-WebSite -Name $SiteName
} catch {
    Write-Host ""
    Write-Host "Could not start website (port $Port may be in use by another site)." -ForegroundColor Yellow
    Write-Host "Please stop the other site in IIS Manager, then run this script again." -ForegroundColor Yellow
    Write-Host ""
    throw
}

# Final status
Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Site URL: http://$(if ($HostName) { $HostName } else { 'localhost' })$(if ($Port -ne 80) { ':' + $Port })"
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Navigate to the URL above"
Write-Host "  2. Complete the NopCommerce installation wizard"
Write-Host "  3. Configure your database connection"
Write-Host ""
Write-Host "Troubleshooting:" -ForegroundColor Yellow
Write-Host "  - Check logs at: $logsPath"
Write-Host "  - Enable stdout logging in web.config for detailed errors"
Write-Host "  - Verify .NET 8.0 Hosting Bundle is installed: dotnet --list-runtimes"
Write-Host ""
