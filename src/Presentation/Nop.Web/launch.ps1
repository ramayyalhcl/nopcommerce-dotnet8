# nopCommerce Web Application Launcher
# Usage: .\launch.ps1 [port]

param(
    [int]$Port = 5000
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "nopCommerce .NET 8.0 Launcher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to the project directory
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectPath

Write-Host "Project Directory: $projectPath" -ForegroundColor Yellow
Write-Host ""

# Check if .NET SDK is available
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 8.0 SDK." -ForegroundColor Red
    exit 1
}
Write-Host ".NET SDK Version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Build the project (skip restore if packages are already available)
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --no-restore 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Attempting full build with restore..." -ForegroundColor Yellow
    dotnet build 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed. Please check the errors above." -ForegroundColor Red
        exit 1
    }
}
Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:$Port"

Write-Host "Starting nopCommerce on http://localhost:$Port" -ForegroundColor Cyan
Write-Host "Environment: Development" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

# Run the application
dotnet run --no-build
