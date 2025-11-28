# Confidential Document Access Control - Complete Analysis

## Document Information

**Created:** 2025-11-28
**Purpose:** Comprehensive analysis of implementing confidentiality-based access control in IkeaDocuScan
**Audience:** Technical stakeholders, architects, project managers

---

## Executive Summary

The IkeaDocuScan application currently has a `Document.Confidential` field that exists as **metadata only**. Users with DocumentType permission can access ALL documents of that type, regardless of confidentiality status. This analysis evaluates five solution approaches to enforce confidential document restrictions and recommends the optimal implementation strategy.

**Recommendation:** Implement **Solution 1 - Permission-Based Confidential Access** by adding a `CanAccessConfidential` flag to the existing `UserPermission` table.

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Document Access Points Inventory](#document-access-points-inventory)
3. [Existing Security Model](#existing-security-model)
4. [Solution Proposals](#solution-proposals)
5. [Solution Comparison Matrix](#solution-comparison-matrix)
6. [Arguments Against Each Solution](#arguments-against-each-solution)
7. [Final Recommendation](#final-recommendation)
8. [Security Considerations](#security-considerations)
9. [Research Sources](#research-sources)

---

## Current State Analysis

### The Problem

The `Document.Confidential` field exists in the database but provides **no access control**:

```
Document.Confidential (bool?) - EXISTS but NOT ENFORCED
```

**Current behavior:**
- If a user has permission to DocumentType X, they can access ALL documents of that type
- The `Confidential` flag is only used for:
  - Search filtering (optional filter in search UI)
  - UI display (showing a label)
  - Export reports (included in Excel exports)

### Database Schema

**Document Entity:**
```
Table: Document
- Id (int, PK)
- Name (nvarchar(255))
- BarCode (int, unique)
- DtId (int?, FK to DocumentType)
- Confidential (bit, nullable)  <-- NOT ENFORCED
- ... other fields
```

**UserPermission Entity:**
```
Table: UserPermission
- Id (int, PK)
- UserId (int, FK to DocuScanUser)
- DocumentTypeId (int?, FK to DocumentType)
- NO confidential access flag exists
```

**DocuScanUser Entity:**
```
Table: DocuScanUser
- UserId (int, PK)
- AccountName (nvarchar(255), unique)
- IsSuperUser (bit)
- LastLogon (datetime)
```

### Current Permission Logic

**File:** `IkeaDocuScan.Infrastructure/Extensions/QueryExtensions.cs`

```csharp
public static IQueryable<Document> FilterByUserPermissions(
    this IQueryable<Document> query,
    CurrentUser currentUser,
    AppDbContext context)
{
    // SuperUser bypass - sees all documents
    if (currentUser.IsSuperUser)
        return query;

    // No access - sees no documents
    if (!currentUser.HasAccess)
        return query.Where(d => false);

    int userId = currentUser.UserId;

    // Filter by DocumentType ONLY - NO confidential check
    return query.Where(doc =>
        context.UserPermissions
            .Where(p => p.UserId == userId)
            .Any(perm =>
                doc.DtId == null ||
                perm.DocumentTypeId == null ||
                doc.DtId == perm.DocumentTypeId
            )
    );
}
```

**Gap:** The `Confidential` field is never checked in access control logic.

---

## Document Access Points Inventory

### API Endpoints

| Endpoint | HTTP Method | Route | Current Protection |
|----------|-------------|-------|-------------------|
| Get All Documents | GET | `/api/documents/` | DocumentType permission |
| Get Document By ID | GET | `/api/documents/{id}` | DocumentType permission |
| Get Document By Barcode | GET | `/api/documents/barcode/{barCode}` | DocumentType permission |
| Get Documents By IDs | POST | `/api/documents/by-ids` | DocumentType permission |
| Search Documents | POST | `/api/documents/search` | DocumentType filtering |
| Stream Document (View) | GET | `/api/documents/{id}/stream` | DocumentType permission |
| Download Document | GET | `/api/documents/{id}/download` | DocumentType permission |
| Create Document | POST | `/api/documents/` | DocumentType permission |
| Update Document | PUT | `/api/documents/{id}` | DocumentType permission |
| Delete Document | DELETE | `/api/documents/{id}` | DocumentType permission |

### UI Components

| Component | Route | Purpose | Access Method |
|-----------|-------|---------|---------------|
| Documents.razor | /documents | Document listing | Calls GetAllAsync |
| SearchDocuments.razor | /documents/search | Advanced search | Calls SearchAsync |
| DocumentPreview.razor | /documents/preview/{id} | Preview with sidebar | Calls stream API |
| PdfViewer.razor | /pdf-viewer/{id} | Full-screen PDF | Calls stream API |
| DocumentProperties.razor | /documents/properties/{id} | Properties view | Calls GetByIdAsync |
| PrintSummary.razor | /documents/print-summary | Print-ready view | Calls GetByIdsAsync |
| ComposeDocumentEmail.razor | /documents/compose-email | Email composition | Calls file API |
| CheckinScanned.razor | /checkin-scanned | Scanned files list | Separate API |
| CheckinFileDetail.razor | /checkin-scanned/detail/{file} | File preview | Separate API |
| DocumentPropertiesPage.razor | /documents/edit/{code} | Edit/Create | Full CRUD |

### Scanned Files (Security Gap)

| Endpoint | Route | Issue |
|----------|-------|-------|
| Get All Scanned Files | `/api/scannedfiles/` | NO document permission check |
| Get Scanned File | `/api/scannedfiles/{fileName}` | NO document permission check |
| Get File Content | `/api/scannedfiles/{fileName}/content` | NO document permission check |
| Get File Stream | `/api/scannedfiles/{fileName}/stream` | NO document permission check |

**Warning:** Scanned files are accessible to anyone with endpoint permission, regardless of document type or confidentiality.

---

## Existing Security Model

### Two-Tier Authorization

**Tier 1: User Access Control**
- Policy: `HasAccess`
- Handler: `UserAccessHandler`
- Check: User has `HasAccess` claim = true
- Effect: Binary access to the system

**Tier 2: Document Type Permission**
- Table: `UserPermission`
- Check: User has permission for document's `DocumentTypeId`
- Effect: Can access documents of permitted types

### Authorization Flow

```
Request → Authentication (Windows) → HasAccess Check → Endpoint Permission
    ↓
Service Layer → FilterByUserPermissions → DocumentType Check → Results
    ↓
NO CONFIDENTIAL CHECK AT ANY POINT
```

### SuperUser Bypass

SuperUsers (`IsSuperUser = true`) bypass ALL permission checks:
- No DocumentType filtering
- Full access to all documents
- Full access to all endpoints

---

## Solution Proposals

### Solution 1: Permission-Based Confidential Access (RECOMMENDED)

**Approach:** Add `CanAccessConfidential` boolean to existing `UserPermission` table.

**Database Change:**
```sql
ALTER TABLE UserPermission ADD CanAccessConfidential BIT NOT NULL DEFAULT 0;
```

**Code Change:** Modify `FilterByUserPermissions`:
```csharp
return query.Where(doc =>
    context.UserPermissions
        .Where(p => p.UserId == userId)
        .Any(perm =>
            (doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId)
            &&
            (doc.Confidential != true || perm.CanAccessConfidential)
        )
);
```

**Pros:**
- Minimal schema change (1 column)
- Leverages existing permission infrastructure
- Single point of enforcement
- Granular per document type
- Backward compatible (default = no access)
- Uses existing audit logging

**Cons:**
- Requires migration of existing permissions
- Admin UI needs update
- No per-document overrides

**Estimated Effort:** 3-5 days

---

### Solution 2: Dedicated Confidential Permission Table

**Approach:** Create separate table for confidential access permissions.

**Database Change:**
```sql
CREATE TABLE ConfidentialDocumentPermission (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL REFERENCES DocuScanUser(UserId),
    DocumentTypeId INT NULL REFERENCES DocumentType(DtId),
    GrantedOn DATETIME NOT NULL DEFAULT GETDATE(),
    GrantedBy NVARCHAR(255) NOT NULL
);
```

**Pros:**
- Complete separation of concerns
- Dedicated audit trail
- Can add approval workflows
- Fine-grained control

**Cons:**
- More complex implementation
- Two permission checks per request
- Potential inconsistency between tables
- Higher query overhead

**Estimated Effort:** 5-8 days

---

### Solution 3: Attribute-Based Access Control (ABAC)

**Approach:** Implement full policy engine evaluating multiple attributes at runtime.

**Implementation:**
```csharp
public class DocumentAccessPolicy
{
    public bool Evaluate(CurrentUser user, Document document, AccessType accessType)
    {
        if (document.Confidential == true && !user.HasConfidentialClearance)
            return false;
        if (document.DtId.HasValue && !user.CanAccessDocumentType(document.DtId))
            return false;
        return true;
    }
}
```

**Pros:**
- Most flexible
- Future-proof
- Supports complex rules (time-based, location-based)
- Follows NIST guidelines

**Cons:**
- Highest complexity
- Performance overhead
- Requires policy management UI
- Overkill for single requirement

**Estimated Effort:** 10-15 days

---

### Solution 4: Resource-Based Authorization

**Approach:** Use ASP.NET Core's `IAuthorizationService` with imperative authorization.

**Implementation:**
```csharp
public class ConfidentialDocumentHandler :
    AuthorizationHandler<ConfidentialDocumentRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConfidentialDocumentRequirement requirement,
        Document resource)
    {
        if (resource.Confidential != true)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasConfidentialClaim = context.User.HasClaim("CanAccessConfidential", "true");
        if (hasConfidentialClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

**Usage in endpoints:**
```csharp
var authResult = await authService.AuthorizeAsync(user, document, "ConfidentialAccess");
if (!authResult.Succeeded)
    return Results.Forbid();
```

**Pros:**
- Follows Microsoft patterns
- Clean separation
- Works with existing infrastructure

**Cons:**
- Requires modifying ALL endpoints
- Document must be loaded before check
- Doesn't integrate with query filtering
- Easy to forget on new endpoints

**Estimated Effort:** 5-7 days

---

### Solution 5: Document Security Level Classification

**Approach:** Replace boolean with multi-level classification system.

**Database Change:**
```sql
CREATE TABLE SecurityLevel (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50),      -- Public, Internal, Confidential, Restricted
    SortOrder INT,
    RequiresClearance BIT
);

ALTER TABLE Document ADD SecurityLevelId INT REFERENCES SecurityLevel(Id);
ALTER TABLE DocuScanUser ADD MaxSecurityLevel INT DEFAULT 1;
```

**Pros:**
- Enterprise data classification standards
- Multiple confidentiality levels
- Scalable for compliance (GDPR, etc.)
- Self-documenting

**Cons:**
- More complex than boolean
- Requires migration of existing data
- User management complexity
- May be overkill

**Estimated Effort:** 6-10 days

---

## Solution Comparison Matrix

| Criteria | Sol 1 | Sol 2 | Sol 3 | Sol 4 | Sol 5 |
|----------|-------|-------|-------|-------|-------|
| **Implementation Complexity** | Low | Medium | Very High | Medium | High |
| **Schema Changes** | Minimal | Moderate | Extensive | Minimal | Extensive |
| **Code Changes** | Minimal | Moderate | Extensive | Moderate | Moderate |
| **Performance Impact** | Negligible | Low | Medium | Low | Low |
| **Centralized Enforcement** | Yes | Partial | Yes | No | Yes |
| **Backward Compatible** | Yes | Yes | No | Yes | No |
| **Future Flexibility** | Medium | High | Very High | Medium | Very High |
| **Audit Integration** | Existing | New | New | Existing | New |
| **Admin UI Changes** | Minor | Moderate | Extensive | Minor | Moderate |
| **Testing Effort** | Low | Medium | High | Medium | Medium |
| **Estimated Days** | 3-5 | 5-8 | 10-15 | 5-7 | 6-10 |

### Ranking

| Rank | Solution | Score | Recommendation |
|------|----------|-------|----------------|
| 1 | Permission-Based Flag | Best | **RECOMMENDED** |
| 2 | Resource-Based Auth | Good | Alternative |
| 3 | Dedicated Table | Fair | Not recommended |
| 4 | Security Levels | Fair | Future consideration |
| 5 | Full ABAC | Poor | Overkill |

---

## Arguments Against Each Solution

### Against Solution 1 (Permission-Based Flag)

1. **Single Point of Failure**
   - If `FilterByUserPermissions` is bypassed (new endpoint, direct DB access), confidential documents are exposed
   - Mitigation: Code review process, security testing

2. **No Approval Workflow**
   - Access is binary - either granted or not
   - No request/approval process for temporary access
   - Mitigation: Implement separately if needed

3. **No Per-Document Override**
   - Cannot grant access to specific confidential documents
   - Only works at DocumentType level
   - Mitigation: Acceptable for most use cases

4. **Audit Gap**
   - No separate audit trail for confidential access attempts
   - Relies on general audit logging
   - Mitigation: Add specific audit logging for confidential access

### Against Solution 2 (Dedicated Table)

1. **Inconsistency Risk**
   - Two permission systems can get out of sync
   - User may have DocumentType permission but not confidential permission
   - Creates confusion for administrators

2. **Performance Impact**
   - Additional JOIN for every document query
   - Must check two tables instead of one
   - Increases database load

3. **Maintenance Burden**
   - Two places to manage permissions
   - Two UIs to maintain
   - Double the testing effort

4. **Complexity**
   - Harder to understand overall permission model
   - More training required for administrators

### Against Solution 3 (Full ABAC)

1. **Over-Engineering**
   - Building a policy engine for one attribute is excessive
   - Introduces unnecessary complexity
   - Violates YAGNI principle

2. **Testing Nightmare**
   - Policy combinations grow exponentially
   - Hard to verify all scenarios
   - Regression risk with policy changes

3. **Performance Overhead**
   - Runtime policy evaluation adds latency
   - Must evaluate policies on every request
   - Caching policies adds complexity

4. **Operational Complexity**
   - Requires policy management UI
   - Staff must understand policy language
   - Debugging authorization issues becomes difficult

### Against Solution 4 (Resource-Based Auth)

1. **Decentralized Control**
   - Each endpoint must remember to call `AuthorizeAsync`
   - Easy to forget on new endpoints
   - No compile-time safety

2. **Load-Then-Check Pattern**
   - Document must be loaded before authorization
   - Less efficient than query-level filtering
   - Wasted database reads for denied requests

3. **List Filtering Problem**
   - Cannot efficiently filter search results
   - Must load all documents then filter in memory
   - Performance degrades with large datasets

4. **Inconsistent User Experience**
   - Search may show documents user cannot access
   - Click leads to "Access Denied"
   - Confusing for users

### Against Solution 5 (Security Level Classification)

1. **Migration Complexity**
   - Converting boolean to levels requires data migration
   - Must decide level for each existing document
   - Risk of misclassification

2. **User Training**
   - Employees must understand classification meanings
   - More decisions when creating documents
   - Higher cognitive load

3. **Over-Classification Risk**
   - Users may over-classify "just to be safe"
   - Reduces document accessibility
   - Creates friction in workflows

4. **Maintenance Overhead**
   - Must define and maintain level definitions
   - Periodic review of classifications needed
   - Governance process required

---

## Final Recommendation

### Why Solution 1 Wins

**Solution 1: Permission-Based Confidential Access** is recommended because:

1. **Minimal Footprint**
   - Single column addition to existing table
   - Extends existing model rather than creating new infrastructure

2. **Centralized Enforcement**
   - Applied in `FilterByUserPermissions`
   - Catches ALL access patterns automatically
   - New endpoints automatically protected

3. **Consistent Architecture**
   - Uses same pattern as DocumentType permissions
   - Familiar model for administrators
   - Existing UI patterns can be reused

4. **Audit Ready**
   - Existing `PermissionChangeAuditLog` captures changes
   - No new audit infrastructure needed

5. **Backward Compatible**
   - Default `false` means existing users cannot access confidential
   - No breaking changes to existing functionality
   - Gradual rollout possible

### Implementation Summary

**Phase 1: Database**
- Add `CanAccessConfidential` column to `UserPermission`
- Add index for query performance

**Phase 2: Backend**
- Update `UserPermission` entity
- Update `CurrentUser` model
- Update `CurrentUserService`
- Modify `FilterByUserPermissions`

**Phase 3: UI**
- Update permission management page
- Add confidential access checkbox
- Add visual indicators for confidential documents

**Phase 4: Audit**
- Add logging for confidential document access
- Ensure permission changes are logged

**Phase 5: Testing**
- Unit tests for permission logic
- Integration tests for all access points
- Manual testing with test user profiles

---

## Security Considerations

### 1. SuperUser Accounts

SuperUsers bypass ALL confidential checks. Recommendations:
- Limit SuperUser accounts to essential personnel only
- Implement regular access reviews
- Monitor SuperUser activity via audit logs
- Consider removing SuperUser bypass for confidential (optional)

### 2. Permission Escalation

Risk: Administrator grants `CanAccessConfidential` too broadly.

Mitigations:
- Add confirmation dialog when granting confidential access
- Log all confidential permission grants with reason
- Implement periodic permission audits
- Consider approval workflow for confidential grants

### 3. Cache Considerations

If user permissions are cached:
- Cache must be invalidated when permissions change
- Session timeout should force permission refresh
- Consider short TTL for permission cache

### 4. Client-Side Security

UI restrictions (hiding buttons, showing access denied) are NOT security controls.

All actual security must be enforced at:
- API/service layer via `FilterByUserPermissions`
- Never trust client-side code for confidentiality

### 5. Scanned Files Vulnerability

**Current Gap:** The `/api/scannedfiles/*` endpoints do NOT use document permissions.

Recommendations:
- Address separately from confidentiality implementation
- Consider restricting scanned file access to specific roles
- Implement pre-check-in confidential marking
- Prompt users to check in files quickly

### 6. Print/Download Limitations

Technical prevention of printing/downloading is NOT foolproof:
- Users can screenshot
- Users can photograph screens
- Browser developer tools can bypass restrictions

Focus on:
- Audit trails and deterrence
- Visible watermarks with username
- Legal/policy consequences for misuse
- Training and awareness

---

## Research Sources

### Access Control Best Practices

- [Role-Based Access Control: Best Practices for Protecting Your Documents](https://www.doculivery.com/blog/role-based-access-control/)
- [Understanding Access Control Models: RBAC, ABAC, and DAC](https://escape.tech/blog/access-control-models/)
- [The Definitive Guide to Role-Based Access Control (RBAC)](https://www.strongdm.com/rbac)
- [What is Role-Based Access Control | RBAC vs ACL & ABAC](https://www.imperva.com/learn/data-security/role-based-access-control-rbac/)
- [The 4 Main Types of Access Control](https://workos.com/guide/access-control)

### Document Classification Standards

- [Document Classification Confidential: Levels and Protocols](https://www.deasylabs.com/blog/document-classification-confidential-levels-and-protocols)
- [Data Classification & Sensitivity Label Taxonomy - Microsoft](https://learn.microsoft.com/en-us/compliance/assurance/assurance-data-classification-and-labels)
- [ISO 27001 & Information Classification](https://www.itgovernance.co.uk/blog/what-is-information-classification-and-how-is-it-relevant-to-iso-27001)
- [Data Classification Levels Explained](https://dataclassification.fortra.com/blog/data-classification-levels-explained-enhance-data-security)
- [What are the Four Levels of Data Classification?](https://www.cyera.com/blog/four-levels-of-data-classification)

### ASP.NET Core Authorization

- [Resource-based authorization in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-9.0)
- [Policy-based authorization in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-9.0)
- [Permission-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/permission-based-authorization-in-aspnet-core/)
- [Resource-Based Authorization in ASP.NET Core - Code Maze](https://code-maze.com/aspnetcore-resource-based-authorization/)
- [A Better Way to Handle Authorization in ASP.NET Core](https://www.thereformedprogrammer.net/a-better-way-to-handle-authorization-in-asp-net-core/)

### Security Risks and Challenges

- [Confidentiality, Integrity, Availability: Key Examples](https://www.datasunrise.com/knowledge-center/confidentiality-integrity-availability-examples/)
- [Protecting Confidential Information: Best Security Practices](https://www.redactable.com/blog/protecting-confidential-information-best-security-practices)
- [Guide to Protecting the Confidentiality of PII - NIST](https://nvlpubs.nist.gov/nistpubs/legacy/sp/nistspecialpublication800-122.pdf)
- [3 Methods to Ensure Confidentiality of Information](https://www.titanfile.com/blog/3-methods-to-ensure-confidentiality-of-information/)

---

## Appendix: Key File References

| Component | File Path |
|-----------|-----------|
| Document Entity | `IkeaDocuScan.Infrastructure/Entities/Document.cs` |
| UserPermission Entity | `IkeaDocuScan.Infrastructure/Entities/UserPermission.cs` |
| DocuScanUser Entity | `IkeaDocuScan.Infrastructure/Entities/DocuScanUser.cs` |
| Query Extensions | `IkeaDocuScan.Infrastructure/Extensions/QueryExtensions.cs` |
| CurrentUser Model | `IkeaDocuScan.Shared/Models/Authorization/CurrentUser.cs` |
| CurrentUserService | `IkeaDocuScan-Web/Services/CurrentUserService.cs` |
| DocumentService | `IkeaDocuScan-Web/Services/DocumentService.cs` |
| UserAccessHandler | `IkeaDocuScan-Web/Authorization/UserAccessHandler.cs` |
| DocumentEndpoints | `IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs` |
| ScannedFileEndpoints | `IkeaDocuScan-Web/Endpoints/ScannedFileEndpoints.cs` |
| EditUserPermissions | `IkeaDocuScan-Web.Client/Pages/EditUserPermissions.razor` |
| SearchDocuments | `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor` |
| DocumentPreview | `IkeaDocuScan-Web.Client/Pages/DocumentPreview.razor` |
| PdfViewer | `IkeaDocuScan-Web.Client/Pages/PdfViewer.razor` |

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-11-28 | Claude Code Analysis | Initial document |
