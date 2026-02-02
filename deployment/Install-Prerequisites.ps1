<#
.SYNOPSIS
    Installs ALL prerequisites for NopCommerce .NET 8.0 on IIS.

.DESCRIPTION
    This script installs everything required to host NopCommerce .NET 8.0 on IIS:
    
    1. IIS - Full web server with all required features
    2. .NET 8.0 Hosting Bundle - Includes:
       - .NET 8.0 Runtime
       - ASP.NET Core 8.0 Runtime
       - ASP.NET Core Module (ANCM) for IIS
       (No separate .NET 8 install needed)
    3. IIS URL Rewrite Module - For HTTPS redirects and clean URLs
    4. iisreset
    
    Works on: Windows Server 2012 R2+, Windows 10/11

.PARAMETER SkipIIS
    Skip IIS installation

.PARAMETER SkipHostingBundle
    Skip .NET Hosting Bundle installation

.PARAMETER SkipUrlRewrite
    Skip URL Rewrite module installation

.PARAMETER WhatIf
    Show what would be installed without making changes

.PARAMETER Force
    Reinstall/repair Hosting Bundle even if ANCM appears installed

.EXAMPLE
    .\Install-Prerequisites.ps1

.EXAMPLE
    .\Install-Prerequisites.ps1 -SkipIIS -Force
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch]$SkipIIS,
    [switch]$SkipHostingBundle,
    [switch]$SkipUrlRewrite,
    [switch]$Force
)

$HostingBundleUrl = "https://dotnet.microsoft.com/download/dotnet/8.0"
$ErrorActionPreference = "Stop"

# Download URLs
$HostingBundleUrls = @(
    "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.11/dotnet-hosting-8.0.11-win.exe",
    "https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/8.0.11/dotnet-hosting-8.0.11-win.exe"
)
$UrlRewriteUrl = "https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi"

function Test-ANCMInstalled {
    try {
        Import-Module WebAdministration -ErrorAction Stop
        $ancm = Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
        return ($null -ne $ancm)
    } catch { return $false }
}

function Test-UrlRewriteInstalled {
    try {
        Import-Module WebAdministration -ErrorAction Stop
        $rewrite = Get-WebGlobalModule -Name "RewriteModule" -ErrorAction SilentlyContinue
        return ($null -ne $rewrite)
    } catch { return $false }
}

function Install-IISFeatures {
    param([switch]$WhatIf)
    
    $isServer = $null -ne (Get-Command Get-WindowsFeature -ErrorAction SilentlyContinue)
    
    if ($isServer) {
        # Windows Server - full feature set
        $requiredFeatures = @(
            "Web-Server", "Web-WebServer", "Web-Common-Http", "Web-Default-Doc", "Web-Dir-Browsing",
            "Web-Http-Errors", "Web-Static-Content", "Web-Http-Redirect", "Web-Health", "Web-Http-Logging",
            "Web-Performance", "Web-Stat-Compression", "Web-Dyn-Compression", "Web-Security", "Web-Filtering",
            "Web-App-Dev", "Web-Net-Ext45", "Web-Asp-Net45", "Web-ISAPI-Ext", "Web-ISAPI-Filter",
            "Web-WebSockets", "Web-Mgmt-Tools", "Web-Mgmt-Console"
        )
        $toInstall = @()
        foreach ($f in $requiredFeatures) {
            $feature = Get-WindowsFeature -Name $f -ErrorAction SilentlyContinue
            if ($feature -and -not $feature.Installed) { $toInstall += $f }
        }
        if ($toInstall.Count -gt 0) {
            if ($WhatIf) {
                Write-Host "  Would install: $($toInstall.Count) IIS features" -ForegroundColor Yellow
                return
            }
            Write-Host "  Installing $($toInstall.Count) IIS features (Web Server, ASP.NET 4.x, WebSockets, Compression, etc.)..."
            $result = Install-WindowsFeature -Name $toInstall -IncludeManagementTools
            if ($result.RestartNeeded) { Write-Host "  [WARN] Server restart may be required." -ForegroundColor Yellow }
        }
        Write-Host "  [PASS] IIS installed." -ForegroundColor Green
    } else {
        # Windows 10/11 - comprehensive IIS features
        $features = @(
            "IIS-WebServerRole", "IIS-WebServer", "IIS-CommonHttpFeatures", "IIS-HttpErrors",
            "IIS-StaticContent", "IIS-DefaultDocument", "IIS-DirectoryBrowsing", "IIS-HttpLogging",
            "IIS-RequestFiltering", "IIS-ApplicationDevelopment", "IIS-NetFxExtensibility45",
            "IIS-ASPNET45", "IIS-HealthAndDiagnostics", "IIS-HttpTracing", "IIS-Security",
            "IIS-Performance", "IIS-WebServerManagementTools", "IIS-ManagementConsole",
            "IIS-WebSockets", "IIS-DynCompression", "IIS-StaticCompression"
        )
        if ($WhatIf) {
            Write-Host "  Would enable: IIS with ASP.NET 4.x, WebSockets, Compression" -ForegroundColor Yellow
            return
        }
        Write-Host "  Enabling IIS features (Web Server, ASP.NET 4.x, WebSockets, Compression)..."
        foreach ($f in $features) {
            $state = Get-WindowsOptionalFeature -Online -FeatureName $f -ErrorAction SilentlyContinue
            if ($state -and $state.State -ne "Enabled") {
                Enable-WindowsOptionalFeature -Online -FeatureName $f -All -NoRestart | Out-Null
            }
        }
        Write-Host "  [PASS] IIS installed." -ForegroundColor Green
    }
}

function Install-HostingBundle {
    param([switch]$WhatIf, [switch]$Force)
    
    if (-not $Force -and (Test-ANCMInstalled)) {
        Write-Host "  [PASS] .NET 8.0 Hosting Bundle already installed (includes Runtime + ANCM)." -ForegroundColor Green
        return $true
    }
    
    if ($Force -and (Test-ANCMInstalled)) { Write-Host "  Repairing Hosting Bundle (-Force)..." }
    
    # Method 1: winget
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if ($winget) {
        Write-Host "  Installing via winget..."
        & winget install --id Microsoft.DotNet.HostingBundle.8 --exact --accept-package-agreements --accept-source-agreements --silent 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  [PASS] Hosting Bundle installed (includes .NET 8 Runtime + ASP.NET Core + ANCM)." -ForegroundColor Green
            return $true
        }
        Write-Host "  winget failed, trying direct download..." -ForegroundColor Yellow
    } else {
        Write-Host "  Downloading Hosting Bundle..."
    }
    
    # Method 2: Direct download
    $installerPath = Join-Path $env:TEMP "dotnet-hosting-8-win.exe"
    foreach ($url in $HostingBundleUrls) {
        try {
            $ProgressPreference = "SilentlyContinue"
            Invoke-WebRequest -Uri $url -OutFile $installerPath -UseBasicParsing -TimeoutSec 120
            if ((Test-Path $installerPath) -and ((Get-Item $installerPath).Length -gt 1MB)) { break }
        } catch { Write-Host "  Download failed, trying next URL..." -ForegroundColor Yellow }
    }
    
    if (-not (Test-Path $installerPath) -or ((Get-Item $installerPath).Length -lt 1MB)) {
        Write-Host "  [FAIL] Could not download. Install manually from: $HostingBundleUrl" -ForegroundColor Red
        if (-not $WhatIf) {
            $r = Read-Host "  Open download page? (Y/n)"
            if ($r -ne "n" -and $r -ne "N") { Start-Process $HostingBundleUrl }
        }
        return $false
    }
    
    if ($WhatIf) {
        Write-Host "  Would run: Hosting Bundle installer" -ForegroundColor Yellow
        Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
        return $true
    }
    
    Write-Host "  Installing Hosting Bundle (includes .NET 8 Runtime + ASP.NET Core + ANCM)..."
    $args = if ($Force) { "/repair", "/quiet", "/norestart" } else { "/install", "/quiet", "/norestart" }
    $p = Start-Process -FilePath $installerPath -ArgumentList $args -Wait -PassThru
    Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
    
    if ($p.ExitCode -eq 0) {
        Write-Host "  [PASS] Hosting Bundle installed." -ForegroundColor Green
        return $true
    }
    Write-Host "  [WARN] Installer exited $($p.ExitCode). Verifying ANCM..." -ForegroundColor Yellow
    return (Test-ANCMInstalled)
}

function Install-UrlRewrite {
    param([switch]$WhatIf)
    
    if (Test-UrlRewriteInstalled) {
        Write-Host "  [PASS] URL Rewrite module already installed." -ForegroundColor Green
        return $true
    }
    
    $msiPath = Join-Path $env:TEMP "rewrite_amd64_en-US.msi"
    try {
        if ($WhatIf) {
            Write-Host "  Would download and install URL Rewrite module" -ForegroundColor Yellow
            return $true
        }
        Write-Host "  Downloading URL Rewrite module..."
        $ProgressPreference = "SilentlyContinue"
        Invoke-WebRequest -Uri $UrlRewriteUrl -OutFile $msiPath -UseBasicParsing -TimeoutSec 60
        
        if (-not (Test-Path $msiPath) -or ((Get-Item $msiPath).Length -lt 100KB)) {
            Write-Host "  [WARN] Could not download URL Rewrite. Optional - install from: https://www.iis.net/downloads/microsoft/url-rewrite" -ForegroundColor Yellow
            return $false
        }
        
        Write-Host "  Installing URL Rewrite module..."
        $p = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", $msiPath, "/quiet", "/norestart" -Wait -PassThru
        Remove-Item $msiPath -Force -ErrorAction SilentlyContinue
        
        if ($p.ExitCode -eq 0) {
            Write-Host "  [PASS] URL Rewrite module installed." -ForegroundColor Green
            return $true
        }
        Write-Host "  [WARN] URL Rewrite install exited $($p.ExitCode). Optional for basic operation." -ForegroundColor Yellow
        return $false
    } catch {
        Write-Host "  [WARN] URL Rewrite install failed: $($_.Message). Optional." -ForegroundColor Yellow
        Remove-Item $msiPath -Force -ErrorAction SilentlyContinue
        return $false
    }
}

# ========== MAIN ==========
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  NopCommerce .NET 8.0 - Complete IIS Prerequisites Installer" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script installs EVERYTHING needed for NopCommerce on IIS:" -ForegroundColor White
Write-Host "  - IIS (full web server with ASP.NET 4.x, WebSockets, Compression)" -ForegroundColor Gray
Write-Host "  - .NET 8.0 Hosting Bundle (.NET 8 Runtime + ASP.NET Core + ANCM for IIS)" -ForegroundColor Gray
Write-Host "  - IIS URL Rewrite (for HTTPS redirects, clean URLs)" -ForegroundColor Gray
Write-Host ""

$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Requires Administrator. Right-click PowerShell > Run as Administrator."
    exit 1
}

# Step 1: IIS
if (-not $SkipIIS) {
    Write-Host "[1/5] IIS Web Server..." -ForegroundColor Green
    Install-IISFeatures -WhatIf:$WhatIfPreference
} else {
    Write-Host "[1/5] Skipping IIS (-SkipIIS)" -ForegroundColor Yellow
}
Write-Host ""

# Step 2: .NET 8.0 Hosting Bundle
if (-not $SkipHostingBundle) {
    Write-Host "[2/5] .NET 8.0 Hosting Bundle (Runtime + ASP.NET Core + ANCM)..." -ForegroundColor Green
    Install-HostingBundle -WhatIf:$WhatIf -Force:$Force
} else {
    Write-Host "[2/5] Skipping Hosting Bundle (-SkipHostingBundle)" -ForegroundColor Yellow
}
Write-Host ""

# Step 3: URL Rewrite
if (-not $SkipUrlRewrite) {
    Write-Host "[3/5] IIS URL Rewrite Module..." -ForegroundColor Green
    Install-UrlRewrite -WhatIf:$WhatIfPreference
} else {
    Write-Host "[3/5] Skipping URL Rewrite (-SkipUrlRewrite)" -ForegroundColor Yellow
}
Write-Host ""

# Step 4: iisreset
if (-not $WhatIfPreference) {
    Write-Host "[4/5] Restarting IIS..." -ForegroundColor Green
    try {
        iisreset /noforce | Out-Null
        Write-Host "  [PASS] IIS restarted." -ForegroundColor Green
    } catch {
        Write-Host "  [WARN] Run 'iisreset' manually." -ForegroundColor Yellow
    }
} else {
    Write-Host "[4/5] Would restart IIS" -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Verify
Write-Host "[5/5] Verifying prerequisites..." -ForegroundColor Green
& "$PSScriptRoot\Verify-Prerequisites.ps1"
$exitCode = $LASTEXITCODE

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
if ($exitCode -eq 0) {
    Write-Host "  All prerequisites installed. Ready for deployment!" -ForegroundColor Green
} else {
    Write-Host "  Some checks failed. Try -Force to repair Hosting Bundle." -ForegroundColor Yellow
}
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: .\Deploy-IIS.ps1 -SiteName NopCommerce -Port 80" -ForegroundColor Cyan
Write-Host ""

exit $exitCode
