# Authorization Matrix - IkeaDocuScan V3

**Date:** 2025-11-05
**Version:** 3.0
**Document Type:** Authorization Configuration Report

---

## Role Definitions

| Role | AD Group | Database Flag | Description |
|------|----------|---------------|-------------|
| **Reader** | `ADGroup.Builtin.Reader` | N/A | Read-only access to documents. Can view and search documents within their assigned permissions. Cannot modify, create, or delete. |
| **Publisher** | `ADGroup.Builtin.Publisher` | N/A | Full document management access. Can create, edit, and register documents. Can send emails and export data. Cannot delete documents or manage users. |
| **SuperUser** | `ADGroup.Builtin.SuperUser` | `IsSuperUser = true` | Full administrative access. Can perform all operations including delete, manage users, and administer reference data. Bypasses all permission filters. |

**Note:** SuperUser status can be granted via **either** AD group membership **or** database flag. If user is in SuperUser AD group, database flag is automatically set to true.

---

## Authorization Matrix

### Legend
- ✅ **Full Access** - Complete access to feature/operation
- 👁️ **View Only** - Read-only access, no modifications allowed
- 🔒 **Filtered** - Access limited by user permissions (document types, countries, counter parties)
- ❌ **No Access** - Feature/operation not available

---

## 1. Document Management

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Documents** | 👁️ 🔒 | ✅ 🔒 | ✅ | Readers/Publishers: Filtered by document type, country, counter party permissions. SuperUsers: See all documents. |
| **Search Documents** | 👁️ 🔒 | ✅ 🔒 | ✅ | All roles can search. Results filtered by permissions. |
| **View Document Properties** | 👁️ 🔒 | ✅ 🔒 | ✅ | View metadata, dates, amounts, attributes. |
| **View Document Files** | 👁️ 🔒 | ✅ 🔒 | ✅ | Preview/download PDF and other file formats. |
| **Create New Document** | ❌ | ✅ | ✅ | Publishers can create documents within their assigned document types. |
| **Edit Document Metadata** | ❌ | ✅ 🔒 | ✅ | Edit properties, dates, amounts. Publishers: Only documents they have permission for. |
| **Delete Document** | ❌ | ❌ | ✅ | **SuperUser only.** Permanent deletion from database. |
| **Register Document** | ❌ | ✅ | ✅ | Change status, assign barcodes, complete registration workflow. |
| **Upload Document Files** | ❌ | ✅ | ✅ | Attach PDF/images to documents. |
| **Download Document Files** | 👁️ 🔒 | ✅ 🔒 | ✅ | All roles can download documents they can view. |
| **Bulk Operations** | ❌ | ✅ 🔒 | ✅ | Bulk edit, bulk delete (SuperUser only). |

**API Endpoints:**
- `GET /api/documents` - All roles (filtered)
- `GET /api/documents/{id}` - All roles (permission check)
- `POST /api/documents` - Publisher, SuperUser
- `PUT /api/documents/{id}` - Publisher (filtered), SuperUser
- `DELETE /api/documents/{id}` - **SuperUser only**
- `POST /api/documents/search` - All roles (filtered)

---

## 2. Scanned File Management (Check-in)

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Scanned Files List** | 👁️ | ✅ | ✅ | View files in network scan directory. |
| **View Scanned File Details** | 👁️ | ✅ | ✅ | Preview scanned document before check-in. |
| **Check-in Scanned Files** | ❌ | ✅ | ✅ | Register scanned files as new documents. |
| **Delete Scanned Files** | ❌ | ❌ | ✅ | **SuperUser only.** Remove files from scan directory. |
| **Bulk Check-in** | ❌ | ✅ | ✅ | Process multiple scanned files at once. |

**Pages:**
- `/checkin-scanned` - Reader (view), Publisher/SuperUser (full)
- `/checkin-file-detail/{filename}` - Reader (view), Publisher/SuperUser (full)

**API Endpoints:**
- `GET /api/scanned-files` - All roles
- `GET /api/scanned-files/{filename}` - All roles
- `POST /api/scanned-files/checkin` - Publisher, SuperUser
- `DELETE /api/scanned-files/{filename}` - **SuperUser only**

---

## 3. Document Email & Communication

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Email Templates** | 👁️ | ✅ | ✅ | View configured email templates. |
| **Compose Document Email** | ❌ | ✅ | ✅ | Send document links/attachments via email. |
| **Send Email with Links** | ❌ | ✅ | ✅ | Send URLs to documents. |
| **Send Email with Attachments** | ❌ | ✅ | ✅ | Send document files as attachments. |
| **Configure Email Templates** | ❌ | ❌ | ✅ | **SuperUser only.** Manage email template configuration. |
| **Configure Email Recipients** | ❌ | ❌ | ✅ | **SuperUser only.** Manage recipient lists. |

**Pages:**
- `/documents/compose-email` - Publisher, SuperUser

**API Endpoints:**
- `POST /api/email/send` - Publisher, SuperUser
- `GET /api/configuration/email-templates` - All roles (read), SuperUser (write)

---

## 4. Excel Export & Preview

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **Excel Preview** | 👁️ 🔒 | ✅ 🔒 | ✅ | Preview search results in tabular format before export. Filtered by permissions. |
| **Export to Excel** | ✅ 🔒 | ✅ 🔒 | ✅ | Export search results to .xlsx file. Filtered by permissions. |
| **Configure Export Columns** | 👁️ | ✅ | ✅ | Choose which columns to include in export. |
| **Large Exports (>1000 records)** | ✅ 🔒 | ✅ 🔒 | ✅ | All roles can export large datasets (subject to system limits). |

**Pages:**
- `/excel-preview` - All roles (filtered)

**API Endpoints:**
- `POST /api/excel-export` - All roles (filtered results)
- `GET /api/excel-export/{filename}` - All roles (own exports only)

---

## 5. User & Permission Management

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View User List** | ❌ | ❌ | ✅ | **SuperUser only.** View all DocuScan users. |
| **View User Permissions** | ❌ | ❌ | ✅ | **SuperUser only.** View permission assignments. |
| **Add New User** | ❌ | ❌ | ✅ | **SuperUser only.** Create user records. |
| **Edit User Permissions** | ❌ | ❌ | ✅ | **SuperUser only.** Grant/revoke document type, country, counter party access. |
| **Delete User** | ❌ | ❌ | ✅ | **SuperUser only.** Remove user from system. |
| **Grant SuperUser Status** | ❌ | ❌ | ✅ | **SuperUser only.** Promote user to SuperUser. |
| **Filter Registration Requests** | ❌ | ❌ | ✅ | **SuperUser only.** View users without permissions (new registration requests). |
| **Process Access Requests** | ❌ | ❌ | ✅ | **SuperUser only.** Approve/deny access requests. |

**Pages:**
- `/edit-userpermissions` - **SuperUser only**

**API Endpoints:**
- `GET /api/users` - **SuperUser only**
- `GET /api/users/{id}` - **SuperUser only**
- `POST /api/users` - **SuperUser only**
- `PUT /api/users/{id}` - **SuperUser only**
- `DELETE /api/users/{id}` - **SuperUser only**
- `GET /api/userpermissions` - **SuperUser only**
- `POST /api/userpermissions` - **SuperUser only**
- `DELETE /api/userpermissions/{id}` - **SuperUser only**

---

## 6. Reference Data Administration

### 6.1 Document Types

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Document Types** | 👁️ | 👁️ | ✅ | All roles can view available document types. |
| **Add Document Type** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Edit Document Type** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Delete Document Type** | ❌ | ❌ | ✅ | **SuperUser only.** (if no documents reference it) |
| **Configure Document Names** | ❌ | ❌ | ✅ | **SuperUser only.** Manage document name templates. |

**Pages:**
- `/document-type-administration` - Reader/Publisher (view), **SuperUser (full)**

**API Endpoints:**
- `GET /api/document-types` - All roles
- `POST /api/document-types` - **SuperUser only**
- `PUT /api/document-types/{id}` - **SuperUser only**
- `DELETE /api/document-types/{id}` - **SuperUser only**

### 6.2 Countries

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Countries** | 👁️ | 👁️ | ✅ | All roles can view country list. |
| **Add Country** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Edit Country** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Delete Country** | ❌ | ❌ | ✅ | **SuperUser only.** (if no documents reference it) |

**Pages:**
- `/country-administration` - Reader/Publisher (view), **SuperUser (full)**

**API Endpoints:**
- `GET /api/countries` - All roles
- `POST /api/countries` - **SuperUser only**
- `PUT /api/countries/{id}` - **SuperUser only**
- `DELETE /api/countries/{id}` - **SuperUser only**

### 6.3 Currencies

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Currencies** | 👁️ | 👁️ | ✅ | All roles can view currency list. |
| **Add Currency** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Edit Currency** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Delete Currency** | ❌ | ❌ | ✅ | **SuperUser only.** (if no documents reference it) |

**Pages:**
- `/currency-administration` - Reader/Publisher (view), **SuperUser (full)**

**API Endpoints:**
- `GET /api/currencies` - All roles
- `POST /api/currencies` - **SuperUser only**
- `PUT /api/currencies/{id}` - **SuperUser only**
- `DELETE /api/currencies/{id}` - **SuperUser only**

### 6.4 Counter Parties

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Counter Parties** | 👁️ | 👁️ | ✅ | All roles can view counter party list. |
| **Add Counter Party** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Edit Counter Party** | ❌ | ❌ | ✅ | **SuperUser only.** |
| **Delete Counter Party** | ❌ | ❌ | ✅ | **SuperUser only.** (if no documents reference it) |
| **Manage Counter Party Relations** | ❌ | ❌ | ✅ | **SuperUser only.** Link counter parties to documents. |

**Pages:**
- `/counterparty-administration` - Reader/Publisher (view), **SuperUser (full)**

**API Endpoints:**
- `GET /api/counterparties` - All roles
- `POST /api/counterparties` - **SuperUser only**
- `PUT /api/counterparties/{id}` - **SuperUser only**
- `DELETE /api/counterparties/{id}` - **SuperUser only**

---

## 7. System Configuration Management

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Configuration** | ❌ | ❌ | 👁️ | **SuperUser only.** View system settings. |
| **Edit Configuration** | ❌ | ❌ | ✅ | **SuperUser only.** Modify system configuration. |
| **Configure Email Templates** | ❌ | ❌ | ✅ | **SuperUser only.** Manage email template content. |
| **Configure Email Recipients** | ❌ | ❌ | ✅ | **SuperUser only.** Manage recipient distribution lists. |
| **Configure File Paths** | ❌ | ❌ | ✅ | **SuperUser only.** Set scan directories, storage locations. |
| **View Encrypted Secrets** | ❌ | ❌ | ❌ | **No access.** Encrypted values not displayed in UI. |

**Pages:**
- `/configuration-management` - **SuperUser only**

**API Endpoints:**
- `GET /api/configuration` - **SuperUser only**
- `PUT /api/configuration` - **SuperUser only**
- `GET /api/configuration/email-templates` - **SuperUser only**
- `PUT /api/configuration/email-templates/{key}` - **SuperUser only**

---

## 8. Action Reminders

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Own Reminders** | ✅ 🔒 | ✅ 🔒 | ✅ | View action reminders assigned to current user. Filtered by document permissions. |
| **View All Reminders** | ❌ | ❌ | ✅ | **SuperUser only.** View all users' reminders. |
| **Create Reminder** | ❌ | ✅ | ✅ | Create action reminder for a document. |
| **Mark Reminder Complete** | ❌ | ✅ | ✅ | Mark own reminders as completed. |
| **Delete Reminder** | ❌ | ✅ | ✅ | Delete own reminders. SuperUser: Delete any reminder. |
| **Configure Reminder Types** | ❌ | ❌ | ✅ | **SuperUser only.** Manage reminder categories. |

**Pages:**
- `/action-reminders` - Reader (own, view), Publisher (own, full), SuperUser (all, full)

**API Endpoints:**
- `GET /api/action-reminders` - All roles (filtered by user)
- `GET /api/action-reminders/all` - **SuperUser only**
- `POST /api/action-reminders` - Publisher, SuperUser
- `PUT /api/action-reminders/{id}` - Publisher (own), SuperUser (all)
- `DELETE /api/action-reminders/{id}` - Publisher (own), SuperUser (all)

---

## 9. Audit Trail

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **View Audit Logs** | ❌ | ❌ | ✅ | **SuperUser only.** View system audit trail. |
| **Search Audit Logs** | ❌ | ❌ | ✅ | **SuperUser only.** Filter by user, date, action type. |
| **Export Audit Logs** | ❌ | ❌ | ✅ | **SuperUser only.** Export audit data to Excel. |
| **View Document History** | 👁️ 🔒 | ✅ 🔒 | ✅ | All roles can view audit history for documents they can access. |
| **Delete Audit Logs** | ❌ | ❌ | ❌ | **No access.** Audit logs are immutable. |

**API Endpoints:**
- `GET /api/audit-trail` - **SuperUser only**
- `GET /api/audit-trail/document/{documentId}` - All roles (permission check)
- `POST /api/audit-trail/search` - **SuperUser only**

---

## 10. System Navigation & General Access

| Access Point | Reader | Publisher | SuperUser | Notes |
|-------------|--------|-----------|-----------|-------|
| **Home Page** | ✅ | ✅ | ✅ | All authenticated users. |
| **Login/Authentication** | ✅ | ✅ | ✅ | Windows Authentication (Active Directory). |
| **Access Denied Page** | ✅ | ✅ | ✅ | Displayed when user lacks required permissions. |
| **Request Access** | ✅ | ✅ | ✅ | Users without permissions can submit access request. |
| **Main Navigation Menu** | 👁️ | ✅ | ✅ | Readers: Limited menu. Publishers/SuperUsers: Full menu. |
| **User Profile/Settings** | ✅ | ✅ | ✅ | View own user information. |

---

## Permission Filtering Details

### Document-Level Filtering

All roles (Reader, Publisher, SuperUser) are subject to permission filtering based on their assigned permissions in the `UserPermissions` table, **except SuperUsers who bypass all filters**.

**Permission Dimensions:**
1. **Document Type** - User can only access specific document types (e.g., Contracts, Invoices)
2. **Country** - User can only access documents from specific countries (e.g., US, SE, UK)
3. **Counter Party** - User can only access documents for specific counter parties

**Null Values = All Access:**
- If `DocumentTypeId` is NULL in `UserPermissions` → User can access **all** document types
- If `CountryCode` is NULL → User can access **all** countries
- If `CounterPartyId` is NULL → User can access **all** counter parties

**Example Permission Scenarios:**

| User | DocumentType | Country | CounterParty | Result |
|------|-------------|---------|--------------|--------|
| John (Reader) | Contract (ID=1) | US | NULL | Can view **only** Contract documents from **US**, for **all** counter parties |
| Jane (Publisher) | NULL | SE, UK | IKEA (ID=5) | Can manage **all** document types from **SE or UK**, for **IKEA only** |
| Bob (SuperUser) | N/A | N/A | N/A | Can access **ALL** documents regardless of type, country, or counter party |

**SuperUser Override:**
- If `IsSuperUser = true` (database) **OR** user is in `ADGroup.Builtin.SuperUser`
- **All permission filters are bypassed**
- User sees and can access **ALL** documents
- No need to grant specific permissions

---

## Authorization Implementation Patterns

### 1. Page-Level Authorization (Blazor Attributes)

```razor
@* Reader, Publisher, SuperUser can access *@
@attribute [Authorize(Policy = "HasAccess")]

@* Only SuperUser can access *@
@attribute [Authorize(Policy = "SuperUser")]

@* Publisher OR SuperUser can access *@
@attribute [Authorize(Roles = "Publisher,SuperUser")]
```

### 2. Conditional UI Rendering

```razor
@* Show button only for Publishers and SuperUsers *@
<AuthorizeView Roles="Publisher,SuperUser">
    <Authorized>
        <button @onclick="CreateDocument">Create Document</button>
    </Authorized>
</AuthorizeView>

@* Show delete button only for SuperUser *@
<AuthorizeView Roles="SuperUser">
    <Authorized>
        <button @onclick="DeleteDocument" class="btn btn-danger">Delete</button>
    </Authorized>
</AuthorizeView>
```

### 3. API Endpoint Authorization

```csharp
// All authenticated users
var group = routes.MapGroup("/api/documents")
    .RequireAuthorization();

// Create/Edit - Publisher and SuperUser only
group.MapPost("/", CreateDocument)
    .RequireAuthorization(policy => policy.RequireRole("Publisher", "SuperUser"));

// Delete - SuperUser only
group.MapDelete("/{id}", DeleteDocument)
    .RequireAuthorization(policy => policy.RequireRole("SuperUser"));
```

### 4. Data Filtering with CurrentUserService

```csharp
var currentUser = await CurrentUserService.GetCurrentUserAsync();

if (currentUser.IsSuperUser)
{
    // SuperUser: See all documents
    return await DocumentService.GetAllDocumentsAsync();
}
else
{
    // Reader/Publisher: Filter by permissions
    var allDocuments = await DocumentService.GetAllDocumentsAsync();
    return allDocuments.Where(doc =>
        currentUser.CanAccessDocument(
            doc.DocumentTypeId,
            doc.CounterPartyId,
            doc.CountryCode
        )
    ).ToList();
}
```

---

## Access Request Workflow

### For Users Without Access (HasAccess = false)

1. **User logs in** via Windows Authentication
2. **WindowsIdentityMiddleware** checks if user exists in `DocuScanUser` table
3. **If user does NOT exist** or has no permissions:
   - `HasAccess = false` claim is set
   - User is redirected to `/access-denied` page
4. **User submits access request:**
   - User sees form to request access with reason
   - Request is logged in `AuditTrail` table
   - (Optional) Email sent to administrators
5. **SuperUser processes request:**
   - SuperUser navigates to `/edit-userpermissions`
   - Filters for "users without permissions" (registration requests)
   - Grants appropriate permissions (document types, countries, counter parties)
6. **User logs in again:**
   - Now has `HasAccess = true` and assigned role (Reader/Publisher)
   - Can access system features based on role

---

## Summary Matrix (Quick Reference)

| Category | Reader | Publisher | SuperUser |
|----------|--------|-----------|-----------|
| **View Documents** | ✅ (filtered) | ✅ (filtered) | ✅ (all) |
| **Create/Edit Documents** | ❌ | ✅ | ✅ |
| **Delete Documents** | ❌ | ❌ | ✅ |
| **Check-in Scanned Files** | ❌ | ✅ | ✅ |
| **Send Emails** | ❌ | ✅ | ✅ |
| **Export to Excel** | ✅ (filtered) | ✅ (filtered) | ✅ (all) |
| **Manage Users/Permissions** | ❌ | ❌ | ✅ |
| **Administer Reference Data** | ❌ | ❌ | ✅ |
| **System Configuration** | ❌ | ❌ | ✅ |
| **View Audit Logs** | ❌ | ❌ | ✅ |
| **Action Reminders** | 👁️ (own) | ✅ (own) | ✅ (all) |

---

## Configuration Files

### appsettings.json

```json
{
  "IkeaDocuScan": {
    "ADGroupReader": "ADGroup.Builtin.Reader",
    "ADGroupPublisher": "ADGroup.Builtin.Publisher",
    "ADGroupSuperUser": "ADGroup.Builtin.SuperUser"
  }
}
```

**Customization:**
- Update AD group names to match your organization's Active Directory structure
- Example: `"ADGroupReader": "IKEA\\DocuScan_Readers"`
- Leave blank or null to disable role

---

## Testing Authorization

### Test Reader Access
```powershell
# Add user to Reader AD group
Add-ADGroupMember -Identity "ADGroup.Builtin.Reader" -Members "testuser"

# Or in database, grant read-only permissions
INSERT INTO UserPermissions (UserId, DocumentTypeId, CountryCode)
VALUES (1, NULL, 'US')  -- All document types in US
```

### Test Publisher Access
```powershell
# Add user to Publisher AD group
Add-ADGroupMember -Identity "ADGroup.Builtin.Publisher" -Members "testuser"
```

### Test SuperUser Access
```powershell
# Add user to SuperUser AD group (preferred)
Add-ADGroupMember -Identity "ADGroup.Builtin.SuperUser" -Members "testuser"

# Or set database flag
UPDATE DocuScanUser SET IsSuperUser = 1 WHERE AccountName = 'DOMAIN\testuser'
```

---

## Related Documentation

- **AUTHORIZATION_GUIDE.md** - Complete implementation guide
- **AD_GROUPS_QUICK_REFERENCE.md** - Active Directory group reference
- **DEPLOYMENT_GUIDE.md** - Production deployment steps
- **CLAUDE.md** - Development guidelines

---

**Status:** ✅ Current as of 2025-11-05
**Maintained by:** IkeaDocuScan Development Team
**Review Frequency:** Quarterly or on major changes
