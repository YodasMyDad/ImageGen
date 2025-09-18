#!/usr/bin/env pwsh

# Build ImageGenApp for all architectures as framework-dependent deployments
# This creates reliable deployments that require .NET 9 Desktop Runtime to be installed

Write-Host "Building ImageGenApp for all architectures (framework-dependent)..." -ForegroundColor Green
Write-Host "This will create framework-dependent deployments for x86, x64, and ARM64" -ForegroundColor Cyan
Write-Host "Target machines need .NET 9 Desktop Runtime installed" -ForegroundColor Yellow
Write-Host ""

$ErrorActionPreference = "Stop"
$buildResults = @()

# Clean all previous builds first
Write-Host "🧹 Cleaning all previous builds..." -ForegroundColor Yellow
dotnet clean --configuration Release

# Build x64
Write-Host ""
Write-Host "Building x64..." -ForegroundColor Green
try {
    dotnet publish --configuration Release -p:PublishProfile=win-x64-framework --verbosity quiet
    $x64Path = "bin\Release\net9.0-windows10.0.19041.0\win-x64\publish-framework\ImageGenApp.exe"
    if (Test-Path $x64Path) {
        $x64FolderPath = Split-Path $x64Path
        $x64Size = (Get-ChildItem $x64FolderPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        $buildResults += "x64: $([math]::Round($x64Size, 2)) MB"
    } else {
        $buildResults += "x64: Build failed"
    }
} catch {
    $buildResults += "x64: Build failed - $($_.Exception.Message)"
}

# Build x86
Write-Host "Building x86..." -ForegroundColor Green
try {
    dotnet publish --configuration Release -p:PublishProfile=win-x86-framework --verbosity quiet
    $x86Path = "bin\Release\net9.0-windows10.0.19041.0\win-x86\publish-framework\ImageGenApp.exe"
    if (Test-Path $x86Path) {
        $x86FolderPath = Split-Path $x86Path
        $x86Size = (Get-ChildItem $x86FolderPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        $buildResults += "x86: $([math]::Round($x86Size, 2)) MB"
    } else {
        $buildResults += "x86: Build failed"
    }
} catch {
    $buildResults += "x86: Build failed - $($_.Exception.Message)"
}

# Build ARM64
Write-Host "Building ARM64..." -ForegroundColor Green
try {
    dotnet publish --configuration Release -p:PublishProfile=win-arm64-framework --verbosity quiet
    $arm64Path = "bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish-framework\ImageGenApp.exe"
    if (Test-Path $arm64Path) {
        $arm64FolderPath = Split-Path $arm64Path
        $arm64Size = (Get-ChildItem $arm64FolderPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        $buildResults += "ARM64: $([math]::Round($arm64Size, 2)) MB"
    } else {
        $buildResults += "ARM64: Build failed"
    }
} catch {
    $buildResults += "ARM64: Build failed - $($_.Exception.Message)"
}

# Show results
Write-Host ""
Write-Host "📊 Build Results:" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
foreach ($result in $buildResults) {
    if ($result.StartsWith("✅")) {
        Write-Host $result -ForegroundColor Green
    } else {
        Write-Host $result -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Output locations:" -ForegroundColor Cyan
Write-Host "x64:   bin\Release\net9.0-windows10.0.19041.0\win-x64\publish-framework\" -ForegroundColor Gray
Write-Host "x86:   bin\Release\net9.0-windows10.0.19041.0\win-x86\publish-framework\" -ForegroundColor Gray
Write-Host "ARM64: bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish-framework\" -ForegroundColor Gray
Write-Host ""
Write-Host "Recommendation: Use x86 for maximum compatibility (works on all Windows machines)" -ForegroundColor Yellow
Write-Host "Staff need .NET 9 Desktop Runtime: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
