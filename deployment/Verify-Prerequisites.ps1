<#
.SYNOPSIS
    Verifies IIS server prerequisites for NopCommerce .NET 8.0 deployment.

.DESCRIPTION
    Checks for:
    - IIS installation and features
    - .NET 8.0 Hosting Bundle
    - ASP.NET Core Module (ANCM)
    - Required permissions

.EXAMPLE
    .\Verify-Prerequisites.ps1
#>

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  NopCommerce .NET 8.0 Prerequisites Check" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$allPassed = $true

# Check 1: IIS Installation
Write-Host "[1] Checking IIS Installation..." -ForegroundColor Yellow
$iisInstalled = $false
try {
    # Windows Server: Get-WindowsFeature is available
    $iisFeature = Get-WindowsFeature -Name Web-Server -ErrorAction Stop
    $iisInstalled = $iisFeature.Installed
} catch {
    # Windows 10/11 Client: Get-WindowsFeature not available - check W3SVC service exists
    $w3svc = Get-Service -Name W3SVC -ErrorAction SilentlyContinue
    $iisInstalled = $null -ne $w3svc
}
if ($iisInstalled) {
    Write-Host "    [PASS] IIS is installed" -ForegroundColor Green
} else {
    Write-Host "    [FAIL] IIS is not installed" -ForegroundColor Red
    Write-Host "    Windows Server: Install-WindowsFeature -Name Web-Server -IncludeManagementTools" -ForegroundColor Gray
    Write-Host "    Windows 10/11: Turn on 'Internet Information Services' in Windows Features" -ForegroundColor Gray
    $allPassed = $false
}

# Check 2: .NET Runtimes
Write-Host "[2] Checking .NET 8.0 Runtimes..." -ForegroundColor Yellow
try {
    $runtimes = & dotnet --list-runtimes 2>&1
    $aspNetCore8 = $runtimes | Where-Object { $_ -match "Microsoft.AspNetCore.App 8\." }
    $netCore8 = $runtimes | Where-Object { $_ -match "Microsoft.NETCore.App 8\." }
    
    if ($aspNetCore8) {
        Write-Host "    [PASS] ASP.NET Core 8.0 Runtime: $aspNetCore8" -ForegroundColor Green
    } else {
        Write-Host "    [FAIL] ASP.NET Core 8.0 Runtime not found" -ForegroundColor Red
        $allPassed = $false
    }
    
    if ($netCore8) {
        Write-Host "    [PASS] .NET Core 8.0 Runtime: $netCore8" -ForegroundColor Green
    } else {
        Write-Host "    [FAIL] .NET Core 8.0 Runtime not found" -ForegroundColor Red
        $allPassed = $false
    }
} catch {
    Write-Host "    [FAIL] dotnet command not found - .NET SDK/Runtime not installed" -ForegroundColor Red
    Write-Host "    Download Hosting Bundle from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Gray
    $allPassed = $false
}

# Check 3: ASP.NET Core Module
Write-Host "[3] Checking ASP.NET Core Module (ANCM)..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    $ancm = Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
    
    if ($ancm) {
        Write-Host "    [PASS] ASP.NET Core Module is registered" -ForegroundColor Green
        foreach ($m in $ancm) {
            Write-Host "           - $($m.Name): $($m.Image)" -ForegroundColor Gray
        }
    } else {
        Write-Host "    [FAIL] ASP.NET Core Module not found in IIS" -ForegroundColor Red
        Write-Host "    Reinstall .NET 8.0 Hosting Bundle after IIS installation" -ForegroundColor Gray
        $allPassed = $false
    }
} catch {
    Write-Host "    [WARN] Could not check - IIS WebAdministration module not available" -ForegroundColor Yellow
}

# Check 4: Handler Mapping
Write-Host "[4] Checking Handler Mappings..." -ForegroundColor Yellow
try {
    $handlers = Get-WebHandler -PSPath "IIS:\" | Where-Object { $_.Name -eq "aspNetCore" }
    if ($handlers) {
        Write-Host "    [PASS] aspNetCore handler is registered" -ForegroundColor Green
    } else {
        Write-Host "    [WARN] aspNetCore handler not found at server level (may be in web.config)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "    [WARN] Could not check handler mappings" -ForegroundColor Yellow
}

# Check 5: IIS Features (Windows Server only - Get-WindowsFeature not on Client)
Write-Host "[5] Checking Recommended IIS Features..." -ForegroundColor Yellow
try {
    $getWindowsFeature = Get-Command Get-WindowsFeature -ErrorAction Stop
    $features = @(
        @{Name="Web-WebSockets"; Description="WebSockets Protocol"},
        @{Name="Web-Dyn-Compression"; Description="Dynamic Compression"},
        @{Name="Web-Stat-Compression"; Description="Static Compression"},
        @{Name="Web-Mgmt-Console"; Description="IIS Management Console"}
    )
    foreach ($feature in $features) {
        $f = Get-WindowsFeature -Name $feature.Name -ErrorAction SilentlyContinue
        if ($f -and $f.Installed) {
            Write-Host "    [PASS] $($feature.Description)" -ForegroundColor Green
        } else {
            Write-Host "    [WARN] $($feature.Description) not installed (recommended)" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "    [INFO] Skipped (Get-WindowsFeature not available on this OS - Windows 10/11)" -ForegroundColor Gray
}

# Summary
Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "  All critical prerequisites PASSED!" -ForegroundColor Green
    Write-Host "  Server is ready for NopCommerce deployment." -ForegroundColor Green
} else {
    Write-Host "  Some prerequisites FAILED!" -ForegroundColor Red
    Write-Host "  Please resolve the issues above before deployment." -ForegroundColor Red
}
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Additional info
Write-Host "Hosting Bundle Download:" -ForegroundColor Yellow
Write-Host "  https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Yellow
Write-Host "  https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/" -ForegroundColor Cyan
Write-Host ""

# Exit with appropriate code for use by deployment scripts
exit $(if ($allPassed) { 0 } else { 1 })
