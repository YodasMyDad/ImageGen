#!/usr/bin/env pwsh

# Build ImageGenApp for x64 architecture as single-file executable
# This creates a self-contained .exe that doesn't require .NET runtime to be installed

Write-Host "Building ImageGenApp for x64..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean --configuration Release

# Build and publish
Write-Host "Publishing single-file executable..." -ForegroundColor Yellow
dotnet publish --configuration Release --publish-profile win-x64

$outputPath = "bin\Release\net9.0-windows10.0.19041.0\win-x64\publish\"
$exePath = Join-Path $outputPath "ImageGenApp.exe"

if (Test-Path $exePath) {
    $size = (Get-Item $exePath).Length / 1MB
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host "📁 Output: $outputPath" -ForegroundColor Cyan
    Write-Host "📦 File size: $([math]::Round($size, 2)) MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now distribute ImageGenApp.exe to your staff!" -ForegroundColor Green
    Write-Host "They don't need .NET installed - it's all included in the .exe" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed! Check the output above for errors." -ForegroundColor Red
    exit 1
}
