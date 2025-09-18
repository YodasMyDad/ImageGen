# Single-File Deployment Guide

This guide explains how to build ImageGenApp as a single executable file that you can distribute to your staff without them needing to install .NET.

## 🚀 Quick Build

### Option 1: Build for Maximum Compatibility (Recommended)
```powershell
.\build-x86.ps1
```
This creates a 32-bit executable that works on **all Windows machines** (32-bit and 64-bit).

### Option 2: Build for Specific Architecture
```powershell
.\build-x64.ps1     # For 64-bit Windows (most modern machines)
.\build-arm64.ps1   # For Windows on ARM (Surface Pro X, etc.)
```

### Option 3: Build All Architectures
```powershell
.\build-all.ps1
```
This builds all three versions at once and shows you the file sizes.

## 📁 Output Locations

After building, you'll find the executable here:
- **x86**: `bin\Release\net9.0-windows10.0.19041.0\win-x86\publish\ImageGenApp.exe`
- **x64**: `bin\Release\net9.0-windows10.0.19041.0\win-x64\publish\ImageGenApp.exe`
- **ARM64**: `bin\Release\net9.0-windows10.0.19041.0\win-arm64\publish\ImageGenApp.exe`

## 📦 What You Get

- **Single .exe file** - No installation required
- **Self-contained** - Includes .NET runtime and all dependencies
- **No prerequisites** - Staff don't need .NET installed
- **Compressed** - Optimized file size with built-in compression
- **Ready to distribute** - Just copy the .exe file

## 💡 Distribution Tips

1. **For maximum compatibility**: Use the x86 build - it works everywhere
2. **File size**: Expect around 80-120MB per executable (varies by architecture)
3. **Testing**: Test the .exe on a machine without .NET installed to verify it works
4. **Antivirus**: Some antivirus software may flag self-contained .exe files - this is normal

## 🛠️ Manual Build (Alternative)

If you prefer using dotnet CLI directly:

```powershell
# x86 (recommended for compatibility)
dotnet publish --configuration Release --publish-profile win-x86

# x64 (for modern 64-bit machines)
dotnet publish --configuration Release --publish-profile win-x64

# ARM64 (for Windows on ARM)
dotnet publish --configuration Release --publish-profile win-arm64
```

## ✅ What's Optimized

The project is configured for optimal single-file deployment with:
- **Single-file publishing** enabled
- **Compression** enabled to reduce file size
- **Native libraries** included for self-extraction
- **Trimming** enabled to remove unused code (Release builds only)
- **ReadyToRun** enabled for faster startup (Release builds only)

## 🔍 Troubleshooting

**Build fails?**
- Make sure you're in the ImageGenApp directory
- Ensure you have .NET 9 SDK installed
- Try running `dotnet clean` first

**Executable won't run on target machine?**
- Verify Windows version compatibility (minimum Windows 10 version 1809)
- Check if antivirus is blocking the file
- Try running as administrator

**File too large?**
- The executable includes the entire .NET runtime for portability
- This is normal for self-contained applications
- Consider using framework-dependent deployment if all target machines have .NET installed
