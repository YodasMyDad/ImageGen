#!/usr/bin/env pwsh

# Build ImageGenApp for all architectures as single-file executables
# This creates self-contained .exe files that don't require .NET runtime to be installed

Write-Host "Building ImageGenApp for all architectures..." -ForegroundColor Green
Write-Host "This will create single-file executables for x86, x64, and ARM64" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$buildResults = @()

# Clean all previous builds first
Write-Host "🧹 Cleaning all previous builds..." -ForegroundColor Yellow
dotnet clean --configuration Release

# Build x64
Write-Host ""
Write-Host "🔨 Building x64..." -ForegroundColor Green
try {
    dotnet publish --configuration Release --publish-profile win-x64 --verbosity quiet
    $x64Path = "bin\Release\net9.0-windows10.0.19041.0\win-x64\publish\ImageGenApp.exe"
    if (Test-Path $x64Path) {
        $x64Size = (Get-Item $x64Path).Length / 1MB
        $buildResults += "✅ x64: $([math]::Round($x64Size, 2)) MB"
    } else {
        $buildResults += "❌ x64: Build failed"
    }
} catch {
    $buildResults += "❌ x64: Build failed - $($_.Exception.Message)"
}

# Build x86
Write-Host "🔨 Building x86..." -ForegroundColor Green
try {
    dotnet publish --configuration Release --publish-profile win-x86 --verbosity quiet
    $x86Path = "bin\Release\net9.0-windows10.0.19041.0\win-x86\publish\ImageGenApp.exe"
    if (Test-Path $x86Path) {
        $x86Size = (Get-Item $x86Path).Length / 1MB
        $buildResults += "✅ x86: $([math]::Round($x86Size, 2)) MB"
    } else {
        $buildResults += "❌ x86: Build failed"
    }
} catch {
    $buildResults += "❌ x86: Build failed - $($_.Exception.Message)"
}

# Build ARM64
Write-Host "🔨 Building ARM64..." -ForegroundColor Green
try {
    dotnet publish --configuration Release --publish-profile win-arm64 --verbosity quiet
    $arm64Path = "bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish\ImageGenApp.exe"
    if (Test-Path $arm64Path) {
        $arm64Size = (Get-Item $arm64Path).Length / 1MB
        $buildResults += "✅ ARM64: $([math]::Round($arm64Size, 2)) MB"
    } else {
        $buildResults += "❌ ARM64: Build failed"
    }
} catch {
    $buildResults += "❌ ARM64: Build failed - $($_.Exception.Message)"
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
Write-Host "📁 Output locations:" -ForegroundColor Cyan
Write-Host "x64:   bin\Release\net9.0-windows10.0.19041.0\win-x64\publish\" -ForegroundColor Gray
Write-Host "x86:   bin\Release\net9.0-windows10.0.19041.0\win-x86\publish\" -ForegroundColor Gray
Write-Host "ARM64: bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish\" -ForegroundColor Gray
Write-Host ""
Write-Host "💡 Recommendation: Use x86 for maximum compatibility (works on all Windows machines)" -ForegroundColor Yellow
