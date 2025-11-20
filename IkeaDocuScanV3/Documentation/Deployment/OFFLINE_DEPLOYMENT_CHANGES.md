# Offline Deployment Updates - Summary

**Date:** 2025-01-18
**Updated For:** Air-Gapped Environment (No Internet Access)

---

## Overview

Both deployment guides have been updated to support deployment to a Windows Server 2022 environment **with NO internet access** (air-gapped environment).

---

## Files Updated

1. **WINDOWS_SERVER_2022_DEPLOYMENT_GUIDE.md** - Main application deployment
2. **ACTION_REMINDER_WINDOWS_SERVICE_DEPLOYMENT.md** - Action Reminder service deployment

---

## Key Changes Made

### 1. Document Headers

**Added:**
- `Server Network: No Internet Access (Air-Gapped Environment)` to document metadata
- Clear warnings about air-gapped environment throughout

### 2. New Prerequisites Section

**Added "Required Downloads" section with:**
- ASP.NET Core 10.0 Hosting Bundle (~200MB)
  - URL: https://dotnet.microsoft.com/download/dotnet/10.0
  - Must be downloaded on machine with internet access
- SQL Server Management Studio (optional, ~600MB)
- Clear instructions on what to download before deployment

**Added "Transfer Method Options":**
- USB flash drive (recommended)
- RDP clipboard copy/paste
- Internal network share
- Jump server (if available)

### 3. Pre-Deployment Checklist

**New comprehensive checklist including:**
- Files to download on internet-connected machine
- Files to transfer (with sizes)
- Transfer media preparation
- Documentation to have available
- Server access verification
- **⚠️ STOP warning** before proceeding

### 4. Updated Directory Structure

**Added:**
- `D:\IkeaDocuScan\Deployment\Installers\` - for offline installers

### 5. Offline .NET Runtime Installation

**Completely rewritten Step 1.3:**

**Before:** Instructions assumed internet access to download installer

**After:**
- Download on internet-connected machine
- Transfer to server via USB/RDP/network share
- Verify installer is present before installation
- Install from local file
- Alternative verification methods if `dotnet` command not found

### 6. Updated File Transfer Instructions

**Step 5.6 (Main App) and Step 5 (Action Reminder) completely rewritten:**

**Added 4 transfer methods:**
1. **USB Flash Drive** (recommended for air-gapped)
   - PowerShell commands for copy to/from USB
   - Drive letter detection
2. **RDP Clipboard Copy/Paste**
   - Step-by-step instructions
   - Notes about large file limitations
3. **Internal Network Share**
   - Accessibility test before copy
   - Fallback to other methods if not accessible
4. **Jump Server/Bastion Host**
   - Note to consult security team

**Each method includes:**
- Verification steps
- File integrity checks
- Troubleshooting tips

### 7. Server Verification Updates

**Updated Step 1.1:**

**Added internet connectivity test:**
```powershell
# Verify NO internet access (expected to fail)
try {
    Invoke-WebRequest -Uri "https://www.microsoft.com" -TimeoutSec 5
    Write-Host "⚠️ WARNING: Server has internet access!"
} catch {
    Write-Host "✅ Confirmed: No internet access (expected)"
}
```

### 8. Updated Checklists

**All phase checklists updated to include:**
- "ASP.NET Core 10.0 Hosting Bundle installer transferred to server"
- ".NET 10.0 Hosting Bundle installed (offline installer)"
- "Server confirmed to have NO internet access (expected)"

### 9. New Appendices (Main Guide)

**Appendix A: Offline Deployment Summary**
- Why offline deployment?
- Software that must be transferred (table with sizes)
- Transfer methods comparison table
- Post-deployment internet isolation notes
- Future update procedures

**Appendix B: Quick Reference Commands**
- Essential PowerShell commands
- File locations quick reference

**Appendix C: Offline Troubleshooting**
- Cannot download .NET Runtime error
- NuGet package restore fails
- Windows Update or feature installation fails

---

## What Deployers Need to Do Differently

### Before Starting Deployment

1. **On machine WITH internet access:**
   - Download ASP.NET Core 10.0 Hosting Bundle installer
   - Build and package application
   - Verify all files are ready
   - Prepare USB drive or RDP session

2. **Transfer to server:**
   - Use USB drive, RDP clipboard, or network share
   - Verify file integrity (MD5 checksums)

3. **On server (NO internet):**
   - Install .NET from transferred installer file
   - Extract and deploy application from transferred package
   - Everything else proceeds as normal

### During Deployment

- **No attempts to download anything** - all installers are local
- **All network connectivity** is to internal resources only:
  - SQL Server (wtseelm-nx20541.ikeadt.com) - internal
  - Active Directory - internal
  - SMTP gateway (smtp-gw.ikea.com) - internal

### After Deployment

- Server remains isolated (no internet access)
- All future updates require manual transfer of packages
- Application functions normally using internal network resources

---

## Files to Download (Complete List)

Before starting deployment, download these on a machine with internet access:

| File | Size | Source | Purpose |
|------|------|--------|---------|
| `dotnet-hosting-10.0.x-win.exe` | ~200MB | https://dotnet.microsoft.com/download/dotnet/10.0 | ASP.NET Core Runtime + IIS integration |
| `SSMS-Setup-ENU.exe` | ~600MB | https://aka.ms/ssmsfullsetup | SQL Server Management Studio (if not on server) |
| `IkeaDocuScan_vX.X.X_YYYYMMDD.zip` | ~100-300MB | Built from source | Application package |
| `IkeaDocuScan_vX.X.X_YYYYMMDD.zip.md5` | 1KB | Generated during build | File integrity verification |
| `ActionReminderService_vX.X.X_YYYYMMDD.zip` | ~50MB | Built from source | Action Reminder service package |

**Total:** Approximately 500MB-1GB (depending on whether SSMS is needed)

**Transfer media:** USB flash drive (minimum 2GB recommended)

---

## Testing Offline Deployment

### Verify Air-Gapped Status

```powershell
# This should FAIL (expected):
Invoke-WebRequest -Uri "https://www.microsoft.com" -TimeoutSec 5

# These should SUCCEED (internal network):
Test-NetConnection -ComputerName "wtseelm-nx20541.ikeadt.com" -Port 1433  # SQL Server
Test-NetConnection -ComputerName "smtp-gw.ikea.com" -Port 25              # SMTP
```

### Verify .NET Installation (Offline)

```powershell
# Should show .NET 10.0 runtimes:
dotnet --list-runtimes

# Should show ASP.NET Core Hosting Bundle:
Get-ItemProperty HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\* |
    Where-Object {$_.DisplayName -like "*ASP.NET Core*"} |
    Select-Object DisplayName, DisplayVersion
```

---

## Benefits of Updated Documentation

1. **Clear expectations:** Deployers know upfront what to download and prepare
2. **No surprises:** No deployment failures due to missing internet connectivity
3. **Multiple transfer options:** Flexibility for different security policies
4. **Comprehensive troubleshooting:** Covers offline-specific issues
5. **Future-proof:** Update procedures documented for air-gapped maintenance

---

## Migration from Online to Offline Guide

If you previously used deployment documentation that assumed internet access:

**Changes you need to be aware of:**
1. Download installers BEFORE going to server
2. Use manual file transfer instead of direct download
3. Verify file integrity with MD5 checksums
4. Expect internet connectivity tests to fail (this is correct)
5. Use offline installer for .NET runtime

**Everything else remains the same:**
- IIS configuration
- Database migration
- Application configuration
- Service installation
- Testing procedures

---

## Support

**If deployment fails due to offline requirements:**

1. Check if required installer files are present:
   ```powershell
   Get-ChildItem "D:\IkeaDocuScan\Deployment\Installers"
   Get-ChildItem "D:\IkeaDocuScan\Deployment\Archives"
   ```

2. Verify file integrity (MD5 checksums match)

3. Ensure .NET runtime is installed:
   ```powershell
   dotnet --list-runtimes
   ```

4. Check internal network connectivity (SQL, AD, SMTP)

5. Review Appendix C: Offline Troubleshooting in main guide

---

**Document Version:** 1.0
**Last Updated:** 2025-01-18
**Prepared By:** Deployment Documentation Update
