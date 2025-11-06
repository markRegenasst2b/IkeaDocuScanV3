# IkeaDocuScan V3 - Deployment Checklist

**Quick Reference Guide for IKEA Deployment**

**Framework:** .NET 10.0
**Organization:** IKEA (ikea.com)

Print this checklist and check off items as you complete them during deployment.

---

## Pre-Deployment (Day Before)

- [ ] Backup production database
- [ ] Backup current appsettings.Local.json
- [ ] Backup current secrets.encrypted.json
- [ ] Document current version: _______________
- [ ] Verify .NET 10.0 Runtime installed on server
- [ ] Verify ASP.NET Core Hosting Bundle 10.0 installed
- [ ] Schedule maintenance window
- [ ] Notify users of deployment

---

## Version Management

- [ ] Open `IkeaDocuScan-Web.csproj`
- [ ] Update `<VersionPrefix>` to: _______________
- [ ] Update `<VersionSuffix>` to: _______________
- [ ] Save file
- [ ] Rebuild solution successfully

---

## Publishing

- [ ] Clean solution
- [ ] Rebuild solution (no errors)
- [ ] Right-click IkeaDocuScan-Web → Publish
- [ ] Select/Create publish profile
- [ ] Verify settings: Release, net10.0, Framework-dependent
- [ ] Click Publish
- [ ] Verify publish succeeded
- [ ] Note publish path: _______________
- [ ] Copy DbMigration\db-scripts\ to publish folder
- [ ] Copy ConfigEncryptionTool to Tools\ folder
- [ ] Create deployment ZIP file
- [ ] Transfer ZIP to server

---

## Database

- [ ] Production database backup restored
- [ ] OR new database created: `IkeaDocuScan`
- [ ] Migration scripts executed in order (ALL 10 scripts):
  - [ ] 00_Create_Database_And_User.sql
  - [ ] 00A_Restore_And_Migrate_Schema.sql
  - [ ] 02_Migrate_FK_Data.sql
  - [ ] 03_Finalize_FK_Constraints.sql
  - [ ] 04_Create_DocuScanUser_Table.sql
  - [ ] 05_Migrate_Users_To_DocuScanUser.sql
  - [ ] 06_Add_FK_Constraint_UserPermissions.sql
  - [ ] 07_Remove_AccountName_From_UserPermissions.sql
  [ ] Verify docuscanch login exists
- [ ] Verify docuscanch user has database access
- [ ] Connection tested with docuscanch user
- [ ] Database configuration verified (TBD)

---

## File Deployment

- [ ] Stop IIS Application Pool
- [ ] Extract ZIP to: `C:\inetpub\wwwroot\IkeaDocuScan`
- [ ] Verify key files present:
  - [ ] IkeaDocuScan-Web.dll
  - [ ] web.config
  - [ ] wwwroot\_framework\
  - [ ] DbMigration\db-scripts\*.sql
  - [ ] Tools\ConfigEncryptionTool\
- [ ] Create `logs` directory
- [ ] Set file permissions

---

## IIS Configuration

### Application Pool
- [ ] Create Application Pool: `IkeaDocuScan`
- [ ] .NET CLR version: No Managed Code
- [ ] Managed pipeline mode: Integrated
- [ ] Advanced Settings:
  - [ ] Identity: ApplicationPoolIdentity (or service account)
  - [ ] Idle Time-out: 0
  - [ ] Load User Profile: True
  - [ ] Start Mode: AlwaysRunning

### Website/Application
- [ ] Create website OR application
- [ ] Bind to Application Pool: `IkeaDocuScan`
- [ ] Physical path: `C:\inetpub\wwwroot\IkeaDocuScan`
- [ ] HTTPS binding configured
- [ ] SSL certificate selected

### Authentication
- [ ] Anonymous Authentication: **Disabled**
- [ ] Windows Authentication: **Enabled**
- [ ] Extended Protection: Accept
- [ ] Kernel-mode authentication: Enabled

### WebSocket
- [ ] WebSocket Protocol feature installed
- [ ] Enabled for application

---

## Application Configuration

- [ ] Run ConfigEncryptionTool as App Pool identity
  - [ ] Enter SQL Server name
  - [ ] Use SQL auth: docuscanch user
  - [ ] Enter docuscanch password
  - [ ] Enter ScannedFilesPath
  - [ ] Verify encryption test succeeds
  - [ ] Copy secrets.encrypted.json to app root

- [ ] Create `appsettings.Local.json`:
  - [ ] IkeaDocuScan → DomainName (ikea.com)
  - [ ] IkeaDocuScan → UserEmail LDAP settings
  - [ ] IkeaDocuScan → EmailGroups LDAP settings
  - [ ] IkeaDocuScan → IKEA AD Groups (3):
    - [ ] ADGroupReader
    - [ ] ADGroupPublisher
    - [ ] ADGroupSuperUser
  - [ ] Email → SmtpHost (smtp-gw.ikea.com)
  - [ ] Email → ApplicationUrl
  - [ ] ExcelExport → ApplicationUrl

- [ ] Verify configuration file permissions

---

## File Permissions

- [ ] Application directory: Read & Execute for App Pool
- [ ] Logs directory: Modify for App Pool
- [ ] Scanned files path: Read & Execute for App Pool
- [ ] Configuration files: Read for App Pool only
- [ ] Test file access as App Pool identity

---

## Windows Authentication

- [ ] Verify IKEA AD groups exist:
  - [ ] UG-DocScanningReaders-CG@WAL-FIN-CH-GEL
  - [ ] UG-DocScanningPublishers-CG@WAL-FIN-CH-GEL
  - [ ] UG-DocScanningSuperUsers-CG@WAL-FIN-CH-GEL
- [ ] Verify LDAP connectivity (LDAP://DC=ikea,DC=com)
- [ ] Add initial admin user(s) to SuperUser AD group
- [ ] Database seeding for initial admin (TBD)

---

## Post-Deployment Verification

### Application Status
- [ ] Application Pool started
- [ ] Check stdout logs for startup success
- [ ] Check Event Viewer for errors
- [ ] Browse to application URL successfully

### Functionality Tests
- [ ] Home page loads
- [ ] Windows Authentication works automatically
- [ ] User sees correct access based on AD group
- [ ] Documents → Search works
- [ ] View document details
- [ ] Access scanned files
- [ ] Generate Excel export
- [ ] Email notifications work (if enabled)

### Health Checks
- [ ] /health returns "Healthy"
- [ ] /health/ready returns JSON
- [ ] /health/live returns JSON

### Version Verification
- [ ] Verify version number: _______________
- [ ] Check file version of IkeaDocuScan-Web.dll

---

## Common Issues Quick Reference

**Pool Stops Immediately:**
- Check stdout logs
- Verify .NET 10.0 Runtime: `dotnet --list-runtimes`

**Database Connection Fails:**
- Verify secrets.encrypted.json exists
- Test: `sqlcmd -S SERVER -d IkeaDocuScan -U docuscanch -P [password]`
- Verify docuscanch user exists and has access
- Re-run ConfigEncryptionTool if password incorrect

**Windows Auth Not Working:**
- Disable Anonymous Authentication
- Enable Windows Authentication
- Check browser Intranet zone settings

**Cannot Access Files:**
- Test as App Pool identity: `runas /user:"IIS APPPOOL\IkeaDocuScan" "cmd.exe"`
- Verify network share permissions

**SignalR Not Working:**
- Install WebSocket Protocol feature
- Check ARR proxy settings

---

## Rollback (If Needed)

- [ ] Stop Application Pool
- [ ] Restore previous files from backup
- [ ] Restore appsettings.Local.json
- [ ] Rollback database (if migrations applied)
- [ ] Start Application Pool
- [ ] Verify functionality

---

## Sign-Off

| Role | Name | Signature | Date/Time |
|------|------|-----------|-----------|
| Deployer | | | |
| Technical Lead | | | |
| QA/Tester | | | |

**Deployment Notes:**
```
[Space for notes about any issues, deviations, or special configurations]







```

---

## Next Steps

- [ ] Monitor application logs for 24 hours
- [ ] Review Performance Monitor metrics
- [ ] Schedule smoke testing (see SMOKE_TEST.md when available)
- [ ] Update documentation with any environment-specific details
- [ ] Notify users deployment is complete

---

**Deployment Completed:** _______________
**Environment:** ☐ Production  ☐ Staging  ☐ UAT
**Version Deployed:** _______________
