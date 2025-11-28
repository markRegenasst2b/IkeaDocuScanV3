# IkeaDocuScan Smoke Test - Production Deployment

**Purpose:** Quick validation after deployment to production
**Duration:** 15-20 minutes
**Performed By:** System Operator / DevOps

---

## Pre-Deployment Checklist

| Item | Verified |
|------|----------|
| Backup of existing database completed | [ ] |
| Backup of existing application files | [ ] |
| Deployment package verified (hash/signature) | [ ] |
| Maintenance window communicated | [ ] |
| Rollback plan documented | [ ] |

---

## 1. Application Startup

### 1.1 Service Status
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Check IIS Application Pool | Started, No errors | [ ] |
| 2 | Check Windows Event Log | No startup errors | [ ] |
| 3 | Navigate to application URL | Login page loads | [ ] |
| 4 | Check for SSL certificate | Valid, no warnings | [ ] |

### 1.2 Health Endpoints
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | GET /health | 200 OK, "Healthy" | [ ] |
| 2 | GET /health/ready | 200 OK | [ ] |
| 3 | GET /health/live | 200 OK | [ ] |

---

## 2. SQL Server Connectivity

### 2.1 Database Connection
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Application loads data on first page | No connection errors | [ ] |
| 2 | Check application logs for DB errors | None found | [ ] |
| 3 | Run simple search query | Results return | [ ] |

### 2.2 Database Verification
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Verify connection string points to production DB | Correct server/database | [ ] |
| 2 | Check DB user permissions | Read/Write access | [ ] |
| 3 | Verify EF migrations applied | No pending migrations | [ ] |

### 2.3 Query Test
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Search for known document | Document found | [ ] |
| 2 | Create test document | Created successfully | [ ] |
| 3 | Delete test document | Deleted successfully | [ ] |

---

## 3. SMTP Configuration

### 3.1 SMTP Connection
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Login as SuperUser | Access admin area | [ ] |
| 2 | Navigate to SMTP Settings | Settings display | [ ] |
| 3 | Verify SMTP Host | Correct mail server | [ ] |
| 4 | Verify SMTP Port | Correct port (25/587/465) | [ ] |
| 5 | Click "Test Connection" | "Connection successful" | [ ] |

### 3.2 Email Delivery Test
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Send test email to operator | Email received | [ ] |
| 2 | Check email From address | Correct sender | [ ] |
| 3 | Check email formatting | HTML renders correctly | [ ] |

### 3.3 Email Template Test
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Select a document | Document selected | [ ] |
| 2 | Send document link email | Email sent successfully | [ ] |
| 3 | Verify email content | Link works, template correct | [ ] |

---

## 4. Logging Configuration

### 4.1 Application Logging
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Check log file location | Files exist in configured path | [ ] |
| 2 | Verify log level | Appropriate for production (Warning/Error) | [ ] |
| 3 | Trigger an action | New log entry appears | [ ] |
| 4 | Check log format | Structured, includes timestamp/level/source | [ ] |

### 4.2 Audit Trail
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Perform an auditable action (edit document) | Action logged | [ ] |
| 2 | Navigate to Audit Trail | Entry visible | [ ] |
| 3 | Verify entry details | User, action, timestamp correct | [ ] |

### 4.3 Error Logging
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Trigger a handled error (invalid search) | Error logged appropriately | [ ] |
| 2 | Check no sensitive data in logs | No passwords/connection strings | [ ] |

---

## 5. Configuration Completeness

### 5.1 Application Settings
| Setting | Location | Verified |
|---------|----------|----------|
| Connection String | appsettings.json / secrets | [ ] |
| SMTP Configuration | Database/Config | [ ] |
| Scanned Files Path | appsettings.json | [ ] |
| Allowed File Extensions | appsettings.json | [ ] |
| Max File Size | appsettings.json | [ ] |
| Application URL (BaseUrl) | appsettings.json | [ ] |

### 5.2 Security Settings
| Setting | Expected | Verified |
|---------|----------|----------|
| Windows Authentication | Enabled | [ ] |
| HTTPS Enforced | Yes | [ ] |
| HSTS Header | Present | [ ] |
| Sensitive config encrypted (DPAPI) | Yes (if Windows) | [ ] |

### 5.3 AD Group Configuration
| Setting | Value Configured | Verified |
|---------|------------------|----------|
| ADGroupReader | Correct AD group | [ ] |
| ADGroupPublisher | Correct AD group | [ ] |
| ADGroupADAdmin | Correct AD group | [ ] |
| ADGroupSuperUser | Correct AD group | [ ] |

---

## 6. File System Access

### 6.1 Check-in Folder
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Verify ScannedFilesPath exists | Directory exists | [ ] |
| 2 | Check IIS App Pool identity permissions | Read/Write/Delete | [ ] |
| 3 | Place test PDF in folder | File visible in Check-in page | [ ] |
| 4 | Check-in the file | File deleted after success | [ ] |

### 6.2 Log Folder
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Verify log directory exists | Directory exists | [ ] |
| 2 | Check write permissions | App can write logs | [ ] |

---

## 7. Authentication Flow

### 7.1 Windows Authentication
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Access application from domain PC | Auto-login works | [ ] |
| 2 | Check user identity displayed | Correct DOMAIN\username | [ ] |
| 3 | Verify role assignment | Correct role based on AD groups | [ ] |

### 7.2 Authorization
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Login as Reader user | Limited menu visible | [ ] |
| 2 | Login as Publisher user | Edit capabilities available | [ ] |
| 3 | Login as SuperUser | Full admin access | [ ] |
| 4 | Test access denied scenario | 403 Forbidden for unauthorized endpoint | [ ] |

---

## 8. SignalR (Real-time Updates)

### 8.1 Hub Connection
| Step | Action | Expected Result | Pass |
|------|--------|-----------------|------|
| 1 | Open application in browser | SignalR connection established | [ ] |
| 2 | Check browser console | No WebSocket errors | [ ] |
| 3 | Open second browser session | Both connected | [ ] |
| 4 | Create document in session 1 | Session 2 receives update | [ ] |

---

## 9. Performance Baseline

### 9.1 Response Times
| Action | Acceptable | Actual | Pass |
|--------|------------|--------|------|
| Home page load | < 3 sec | _____ | [ ] |
| Document search (100 results) | < 5 sec | _____ | [ ] |
| PDF download (5MB) | < 10 sec | _____ | [ ] |
| Excel export (500 rows) | < 15 sec | _____ | [ ] |

---

## 10. Rollback Verification

### 10.1 Rollback Readiness
| Item | Verified |
|------|----------|
| Previous version backup accessible | [ ] |
| Database rollback script tested (if schema changed) | [ ] |
| Rollback procedure documented | [ ] |
| Rollback can be completed within maintenance window | [ ] |

---

## Post-Deployment Actions

| Action | Completed |
|--------|-----------|
| Remove test data created during smoke test | [ ] |
| Notify stakeholders of successful deployment | [ ] |
| Update deployment documentation | [ ] |
| Close maintenance window | [ ] |
| Monitor application for 30 minutes | [ ] |

---

## Issues Found

| Issue | Severity | Resolution |
|-------|----------|------------|
| | | |
| | | |
| | | |

---

## Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Deployer | | | |
| Verifier | | | |
| Approver | | | |

---

**Smoke Test Result:** PASS / FAIL

**Notes:**
_______________________________________________
_______________________________________________
_______________________________________________
