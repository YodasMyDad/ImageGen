#!/usr/bin/env pwsh

# Build ImageGenApp for ARM64 architecture as framework-dependent deployment
# This requires .NET 9 Desktop Runtime to be installed on target machines but is much more reliable

Write-Host "Building ImageGenApp for ARM64 (framework-dependent)..." -ForegroundColor Green
Write-Host "This version requires .NET 9 Desktop Runtime on target machines" -ForegroundColor Yellow

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean --configuration Release

# Build and publish
Write-Host "Publishing framework-dependent deployment..." -ForegroundColor Yellow
dotnet publish --configuration Release -p:PublishProfile=win-arm64-framework

$outputPath = "bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish-framework\"
$exePath = Join-Path $outputPath "ImageGenApp.exe"

if (Test-Path $exePath) {
    $size = (Get-Item $exePath).Length / 1MB
    $folderSize = (Get-ChildItem $outputPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host "Output: $outputPath" -ForegroundColor Cyan
    Write-Host "Exe size: $([math]::Round($size, 2)) MB" -ForegroundColor Cyan
    Write-Host "Total folder size: $([math]::Round($folderSize, 2)) MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "This version should work reliably!" -ForegroundColor Green
    Write-Host "This ARM64 build is for Windows on ARM devices (Surface Pro X, etc.)" -ForegroundColor Green
    Write-Host "Staff need .NET 9 Desktop Runtime installed:" -ForegroundColor Yellow
    Write-Host "   https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can distribute the entire folder or create an installer" -ForegroundColor Green
} else {
    Write-Host "Build failed! Check the output above for errors." -ForegroundColor Red
    exit 1
}
