# Deployment Smoke Test Plan

**Purpose:** Verify all services and integrations are correctly configured after deployment
**Duration:** ~30 minutes
**Prerequisite:** SuperUser access, test files in scan folder, SMTP accessible

---

## Pre-Test Setup

```
[ ] Place 2-3 test PDF files in D:\IkeaDocuScan\ScannedFiles\checkin
[ ] Ensure test users exist: reader, publisher, adadmin, superuser
[ ] Have a recipient email address ready for email tests
[ ] Note current document count for comparison
```

---

## 1. Infrastructure Checks (5 min)

### SMOKE-01: Application Starts
| Check | How | Expected |
|-------|-----|----------|
| Site responds | Browse to https://testdocuscan.ikeadt.com | No 500/503 error |
| No startup errors | Check `D:\Logs\IkeaDocuScan\log-*.json` | No FATAL/ERROR on startup |
| AppPool running | IIS Manager > Application Pools | IkeaDocuScan = Started |

### SMOKE-02: Windows Authentication
| Check | How | Expected |
|-------|-----|----------|
| Auto-login works | Access site from domain PC | No login prompt, user identified |
| Username shown | Check header/profile area | DOMAIN\username displayed |
| Correct role assigned | Check user menu/permissions | Role matches AD group |

### SMOKE-03: Database Connectivity
| Check | How | Expected |
|-------|-----|----------|
| Documents load | Navigate to Documents page | Document list appears |
| Search works | Enter barcode in search | Results return |
| No timeout | Page loads in <5 seconds | Fast response |

**If fails:** Check ConnectionStrings in appsettings.Local.json, verify SQL login

### SMOKE-04: File System Access
| Check | How | Expected |
|-------|-----|----------|
| Scanned files visible | Navigate to Check-in Scanned | Test files listed |
| File preview works | Click a PDF file | Preview renders |
| Logs being written | Check `D:\Logs\IkeaDocuScan\` | Today's log file exists and growing |

**If fails:** Check AppPool identity permissions on folders

---

## 2. Core Functionality (10 min)

### SMOKE-05: Document CRUD (as publisher)
| Step | Action | Verify |
|------|--------|--------|
| Create | Add document with unique barcode | Success message, appears in list |
| Read | Search by barcode | Document found |
| Update | Edit document name | Changes saved |
| Download | Download document file | File downloads correctly |

### SMOKE-06: Check-In Flow (as publisher)
| Step | Action | Verify |
|------|--------|--------|
| 1 | Select file from scan folder | Preview loads |
| 2 | Click Check-in | Form opens |
| 3 | Enter barcode, select type/party | Fields work |
| 4 | Complete check-in | Document created |
| 5 | Verify file moved | No longer in scan folder |
| 6 | Search new document | Found with file attached |

### SMOKE-07: Search & Filter
| Test | Action | Verify |
|------|--------|--------|
| Barcode search | Enter known barcode | Exact match |
| Type filter | Select DocumentType | Filtered results |
| Party filter | Select CounterParty | Filtered results |
| Date range | Set From/To dates | Filtered results |
| Clear filters | Clear all | Full list restored |

### SMOKE-08: Excel Export
| Step | Action | Verify |
|------|--------|--------|
| 1 | Select 3+ documents | Checkboxes work |
| 2 | Click Export | Excel downloads |
| 3 | Open file | Contains correct data, formatting OK |

---

## 3. Integration Services (10 min)

### SMOKE-09: Email Service
| Step | Action | Verify |
|------|--------|--------|
| 1 | Select document | Document selected |
| 2 | Click Send Email | Compose form opens |
| 3 | Enter test recipient | Address accepted |
| 4 | Send as Link | Email sent message |
| 5 | Check recipient inbox | Email received with link |
| 6 | Click link in email | Opens document in app |

**If fails:** Check SMTP settings in appsettings.Local.json, test with `Test-NetConnection smtp-gw.ikea.com -Port 25`

### SMOKE-10: Email with Attachment
| Step | Action | Verify |
|------|--------|--------|
| 1 | Select document with PDF | Document selected |
| 2 | Send as Attachment | Email sent |
| 3 | Check inbox | PDF attached to email |

### SMOKE-11: Active Directory Groups
| Test | User | Expected Role |
|------|------|---------------|
| Reader group | reader | Reader badge, read-only access |
| Publisher group | publisher | Publisher badge, can create/edit |
| ADAdmin group | adadmin | ADAdmin badge, admin view access |
| SuperUser | superuser | SuperUser badge, full access |

**If fails:** Check AD group names in appsettings.Local.json match actual AD groups

### SMOKE-12: SignalR Real-Time Updates
| Step | Action | Verify |
|------|--------|--------|
| 1 | Open Documents in Browser A | List shown |
| 2 | Open Documents in Browser B | List shown |
| 3 | Create document in Browser A | Document appears |
| 4 | Check Browser B | New document appears WITHOUT refresh |

**If fails:** Check WebSockets enabled in IIS, firewall not blocking SignalR

---

## 4. Admin Functions (5 min, as superuser)

### SMOKE-13: User Management
| Step | Action | Verify |
|------|--------|--------|
| 1 | Navigate to User Management | Page loads |
| 2 | View existing users | User list displayed |
| 3 | View user permissions | Permission details shown |

### SMOKE-14: Endpoint Authorization
| Step | Action | Verify |
|------|--------|--------|
| 1 | Navigate to Endpoint Management | Page loads |
| 2 | View endpoints | API routes listed |
| 3 | View role assignments | Roles shown per endpoint |

### SMOKE-15: System Logs
| Step | Action | Verify |
|------|--------|--------|
| 1 | Navigate to Log Viewer | Page loads |
| 2 | View recent logs | Entries displayed |
| 3 | Filter by Error | Only errors shown (or none) |
| 4 | Search for known text | Results found |

### SMOKE-16: Configuration
| Step | Action | Verify |
|------|--------|--------|
| 1 | Navigate to Configuration | Page loads |
| 2 | View email templates | Templates listed |
| 3 | Test SMTP connection | "Connection successful" |

---

## 5. Edge Cases & Error Handling (5 min)

### SMOKE-17: Invalid Barcode
| Step | Action | Verify |
|------|--------|--------|
| 1 | Search for non-existent barcode | "No results" (not error) |
| 2 | Create with existing barcode | Validation error shown |

### SMOKE-18: Permission Denied
| Step | Action | Verify |
|------|--------|--------|
| 1 | As reader, try access admin page | Access Denied page (not 500) |
| 2 | As reader, try delete document | Button hidden or 403 |

### SMOKE-19: Large File Handling
| Step | Action | Verify |
|------|--------|--------|
| 1 | Preview large PDF (~20MB) | Loads (may take time) |
| 2 | Download large file | Download completes |

### SMOKE-20: Session Persistence
| Step | Action | Verify |
|------|--------|--------|
| 1 | Navigate through multiple pages | No random logouts |
| 2 | Wait 5 minutes idle | Session still active |
| 3 | Perform action | Works without re-auth |

### SMOKE-21: Browser Compatibility
| Browser | Test |
|---------|------|
| Edge | Documents page loads, basic CRUD works |
| Chrome | Documents page loads, basic CRUD works |
| Firefox | Documents page loads, basic CRUD works |

---

## 6. Audit Trail Verification

### SMOKE-22: Actions Logged
| Action | Check Audit Trail | Expected |
|--------|-------------------|----------|
| Create document | Search by new barcode | Create entry with user/timestamp |
| Update document | Search by barcode | Update entry logged |
| Download document | Search by barcode | Read/Download entry logged |
| Check-in file | Search by barcode | CheckIn entry logged |

---

## Quick Troubleshooting

| Symptom | Likely Cause | Check |
|---------|--------------|-------|
| 500 on startup | Config error | Logs, web.config, connection string |
| Login loop | Auth misconfigured | IIS Auth settings, Windows Auth enabled |
| No documents shown | DB connection | Connection string, SQL permissions |
| Scanned files empty | Path wrong | ScannedFilesPath in config, folder permissions |
| Email fails | SMTP blocked | Port 25 open, SMTP host reachable |
| SignalR fails | WebSockets | IIS WebSockets feature, firewall |
| Slow performance | Missing indexes | SQL execution plans, memory pressure |
| Wrong role | AD group mismatch | AD group names in config vs actual |

---

## Sign-Off

**Deployment Date:** _______________
**Tester:** _______________
**Environment:** [ ] Test  [ ] Production

| Section | Status | Notes |
|---------|--------|-------|
| 1. Infrastructure | [ ] Pass [ ] Fail | |
| 2. Core Functionality | [ ] Pass [ ] Fail | |
| 3. Integration Services | [ ] Pass [ ] Fail | |
| 4. Admin Functions | [ ] Pass [ ] Fail | |
| 5. Edge Cases | [ ] Pass [ ] Fail | |
| 6. Audit Trail | [ ] Pass [ ] Fail | |

**Overall Result:** [ ] APPROVED  [ ] BLOCKED

**Blocking Issues:**
```


```

**Notes:**
```


```
