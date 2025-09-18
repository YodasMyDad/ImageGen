# Framework-Dependent Deployment Guide

This guide explains how to build ImageGenApp as a reliable framework-dependent deployment that requires .NET 9 Desktop Runtime to be installed on target machines.

## 🚀 Quick Build

### Option 1: Build for Maximum Compatibility (Recommended)
```powershell
.\build-x86.ps1
```
This creates a framework-dependent deployment that works on **all Windows machines** (32-bit and 64-bit).

### Option 2: Build for Specific Architecture
```powershell
.\build-x64.ps1     # For 64-bit Windows (most modern machines)
.\build-arm64.ps1   # For Windows on ARM (Surface Pro X, etc.)
```

### Option 3: Build All Architectures
```powershell
.\build-all.ps1
```
This builds all three versions at once and shows you the folder sizes.

## 📁 Output Locations

After building, you'll find the application folder here:
- **x86**: `bin\Release\net9.0-windows10.0.19041.0\win-x86\publish-framework\`
- **x64**: `bin\Release\net9.0-windows10.0.19041.0\win-x64\publish-framework\`
- **ARM64**: `bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish-framework\`

## 📦 What You Get

- **Small application folder** - ~10-20MB total
- **Framework-dependent** - Uses system .NET runtime for reliability
- **Requires .NET 9 Desktop Runtime** - Staff need this installed once
- **Perfect WinUI compatibility** - No missing DLL or UI issues
- **Ready to distribute** - Copy the entire folder

## 💡 Distribution Tips

1. **For maximum compatibility**: Use the x86 build - it works everywhere
2. **Folder size**: Expect around 10-20MB per deployment folder
3. **Prerequisites**: Staff need .NET 9 Desktop Runtime installed first
4. **Testing**: Test on a machine with .NET 9 Desktop Runtime to verify it works
5. **One-time setup**: Runtime installation is one-time per machine

## 🛠️ Manual Build (Alternative)

If you prefer using dotnet CLI directly:

```powershell
# x86 (recommended for compatibility)
dotnet publish --configuration Release -p:PublishProfile=win-x86-framework

# x64 (for modern 64-bit machines)
dotnet publish --configuration Release -p:PublishProfile=win-x64-framework

# ARM64 (for Windows on ARM)
dotnet publish --configuration Release -p:PublishProfile=win-arm64-framework
```

## 📋 .NET 9 Desktop Runtime Installation

Your staff need to install this once per machine:
- **Download**: https://dotnet.microsoft.com/download/dotnet/9.0
- **Choose**: ".NET Desktop Runtime 9.0.9"
- **Architecture**: Match your app (x86, x64, or ARM64)
- **Size**: ~50MB download

## ✅ What's Optimized

The project is configured for optimal framework-dependent deployment with:
- **Framework-dependent** deployment for reliability
- **No trimming** to ensure EF Core compatibility
- **No ReadyToRun** to avoid build issues
- **WinUI compatibility** ensured with proper configuration
- **Small deployment size** by using system .NET runtime

## 🔍 Troubleshooting

**Build fails?**
- Make sure you're in the ImageGenApp directory
- Ensure you have .NET 9 SDK installed
- Try running `dotnet clean` first

**App won't run on target machine?**
- Verify .NET 9 Desktop Runtime is installed
- Check Windows version compatibility (minimum Windows 10 version 1809)
- Try running from command line to see error messages
- Ensure correct architecture (x86 vs x64 vs ARM64)

**Missing .NET 9 Desktop Runtime?**
- Download from: https://dotnet.microsoft.com/download/dotnet/9.0
- Choose ".NET Desktop Runtime" (not SDK)
- Install once per machine, works for all .NET 9 desktop apps
