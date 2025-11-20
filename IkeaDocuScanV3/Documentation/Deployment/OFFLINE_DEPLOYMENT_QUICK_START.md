# Offline Deployment Quick Start Card

**⚠️ IMPORTANT: Server has NO INTERNET ACCESS - Read this first!**

---

## Before You Start

This is an **AIR-GAPPED** deployment. The server cannot download anything from the internet.

**YOU MUST:**
1. Download required software on a machine WITH internet access
2. Transfer all files to the server manually (USB/RDP/network share)
3. Follow offline installation procedures

---

## What to Download (On Internet-Connected Machine)

| # | File | Size | URL | Notes |
|---|------|------|-----|-------|
| 1 | ASP.NET Core 10.0 Hosting Bundle | 200MB | https://dotnet.microsoft.com/download/dotnet/10.0 | Click "Download Hosting Bundle" |
| 2 | Application Package (built) | 100-300MB | Build from source | Includes app + scripts + tools |

**Optional:**
- SQL Server Management Studio (600MB) - only if not on server already

---

## Transfer Methods

### Option 1: USB Flash Drive (RECOMMENDED)
```
1. Copy files to USB on dev machine
2. Safely eject USB
3. Connect USB to server
4. Copy files from USB to D:\IkeaDocuScan\Deployment\
```

### Option 2: RDP Clipboard
```
1. Open RDP to server
2. Copy file on local machine (Ctrl+C)
3. Paste in server RDP session (Ctrl+V)
4. Wait for transfer (may take several minutes)
```

### Option 3: Network Share
```
Copy files to: \\SERVER-NAME\D$\IkeaDocuScan\Deployment\
(Only if server accessible via network share)
```

---

## Files Go Here on Server

```
D:\IkeaDocuScan\Deployment\
├── Installers\
│   └── dotnet-hosting-10.0.x-win.exe          ← .NET installer
└── Archives\
    ├── IkeaDocuScan_v3.1.0_20250118.zip       ← Application
    └── IkeaDocuScan_v3.1.0_20250118.zip.md5   ← Checksum
```

---

## Installation Order

### Phase 1: Prerequisites
1. ✅ Create directory structure (PowerShell scripts in guide)
2. ✅ Install .NET from: `D:\IkeaDocuScan\Deployment\Installers\dotnet-hosting-*.exe`
3. ✅ Verify with: `dotnet --list-runtimes`

### Phase 2: IIS
1. ✅ Install IIS features (PowerShell scripts in guide)
2. ✅ Create SSL certificate (self-signed for testing)
3. ✅ Configure hosts file for testdocuscan.ikeadt.com

### Phase 3: Database
1. ✅ Connect to SQL Server: wtseelm-nx20541.ikeadt.com
2. ✅ Create database and `docuscanch` user
3. ✅ Run migration scripts (in order, one at a time)

### Phase 4: Application
1. ✅ Extract ZIP to: `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\`
2. ✅ Create `appsettings.Local.json`
3. ✅ Run ConfigEncryptionTool to create `secrets.encrypted.json`
4. ✅ Set file permissions

### Phase 5: IIS Configuration
1. ✅ Create AppPool "IkeaDocuScan"
2. ✅ Create Website "IkeaDocuScan"
3. ✅ Enable Windows Authentication
4. ✅ Bind SSL certificate
5. ✅ Start AppPool and Website

### Phase 6: Test
1. ✅ Open: https://testdocuscan.ikeadt.com
2. ✅ Accept certificate warning
3. ✅ Verify Windows Authentication works
4. ✅ Test document search
5. ✅ Check logs for errors

---

## Quick Verification Commands

```powershell
# Verify .NET installed
dotnet --list-runtimes

# Verify IIS running
Get-Service W3SVC

# Verify AppPool running
Get-WebAppPoolState -Name "IkeaDocuScan"

# Verify website running
Get-WebsiteState -Name "IkeaDocuScan"

# View recent logs
Get-Content "D:\Logs\IkeaDocuScan\log-$(Get-Date -Format 'yyyyMMdd').json" -Tail 20

# Test SQL connectivity (internal network)
Test-NetConnection -ComputerName "wtseelm-nx20541.ikeadt.com" -Port 1433

# Verify NO internet (should FAIL - expected)
Test-NetConnection -ComputerName "www.microsoft.com" -Port 443
```

---

## Common Offline Issues

### "Cannot download .NET runtime"
**Fix:** .NET is already installed offline. Run: `dotnet --list-runtimes`

### "Cannot connect to nuget.org"
**Fix:** Don't build on server. Use transferred pre-built package.

### "Application Pool stops immediately"
**Fix:** Check stdout logs in `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs\`

### "Database connection fails"
**Fix:** Verify SQL Server accessible: `Test-NetConnection -ComputerName wtseelm-nx20541.ikeadt.com -Port 1433`

---

## Network Connectivity

### ✅ Should Work (Internal Network)
- SQL Server: wtseelm-nx20541.ikeadt.com:1433
- Active Directory: Domain controllers
- SMTP: smtp-gw.ikea.com:25

### ❌ Should NOT Work (By Design)
- Internet: Any public website
- Windows Update: update.microsoft.com
- NuGet: nuget.org

---

## File Locations Quick Reference

```
Application:      D:\IkeaDocuScan\wwwroot\IkeaDocuScan\
Configuration:    D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json
Logs:             D:\Logs\IkeaDocuScan\
Tools:            D:\IkeaDocuScan\Tools\
Scanned Files:    D:\IkeaDocuScan\ScannedFiles\checkin\
Backups:          D:\Backups\IkeaDocuScan\
```

---

## Emergency Contacts

**During Deployment Issues:**
1. Check Event Viewer: Application log
2. Check stdout logs: `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs\`
3. Check Serilog: `D:\Logs\IkeaDocuScan\`
4. Review full guide: `WINDOWS_SERVER_2022_DEPLOYMENT_GUIDE.md`

---

## Post-Deployment

### Smoke Test Checklist
- [ ] https://testdocuscan.ikeadt.com loads
- [ ] Windows Authentication works (no login prompt)
- [ ] Username shows correctly (IKEADT\yourname)
- [ ] Document search page loads
- [ ] Can perform search
- [ ] Logs show no errors

### Next Steps
1. Deploy Action Reminder Service (separate guide)
2. Add users to AD groups:
   - UG-DocScanningReaders-CG@WAL-FIN-CH-GEL (read-only)
   - UG-DocScanningPublishers-CG@WAL-FIN-CH-GEL (read/write)
   - UG-DocScanningSuperUsers-CG@WAL-FIN-CH-GEL (admin)
3. Replace self-signed certificate with proper certificate
4. Update DNS (remove hosts file entry)
5. User acceptance testing

---

## Remember

✅ **DO:**
- Download everything on internet-connected machine first
- Transfer all files before starting installation
- Verify file integrity (MD5 checksums)
- Follow guide step-by-step

❌ **DON'T:**
- Try to download anything on the server
- Skip steps in the deployment guide
- Modify transferred installer files
- Build application on the server

---

**Full Documentation:**
- Main Guide: `WINDOWS_SERVER_2022_DEPLOYMENT_GUIDE.md`
- Action Reminder: `ACTION_REMINDER_WINDOWS_SERVICE_DEPLOYMENT.md`
- Changes Summary: `OFFLINE_DEPLOYMENT_CHANGES.md`

**Estimated Total Time:** 3-4 hours

**Server:** Windows Server 2022 Standard (No Internet Access)
**URL:** https://testdocuscan.ikeadt.com
**Database:** wtseelm-nx20541.ikeadt.com

---

**Keep this card handy during deployment!**
