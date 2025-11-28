# Confidential Document Access Control - Implementation Plan

## Document Information

**Created:** 2025-11-28
**Version:** 1.0
**Status:** Planned
**Solution:** Permission-Based Confidential Access (Solution 1)

---

## Table of Contents

1. [Overview](#overview)
2. [Current State](#current-state)
3. [Target State](#target-state)
4. [Implementation Phases](#implementation-phases)
5. [Phase 1: Database Changes](#phase-1-database-changes)
6. [Phase 2: Entity and Model Updates](#phase-2-entity-and-model-updates)
7. [Phase 3: Service Layer Updates](#phase-3-service-layer-updates)
8. [Phase 4: Query Extension Updates](#phase-4-query-extension-updates)
9. [Phase 5: UI Updates](#phase-5-ui-updates)
10. [Phase 6: Audit Trail Enhancement](#phase-6-audit-trail-enhancement)
11. [Phase 7: Testing](#phase-7-testing)
12. [Rollback Plan](#rollback-plan)
13. [Security Considerations](#security-considerations)

---

## Overview

### Problem Statement

The `Document.Confidential` field currently exists as metadata only. Users with DocumentType permission can access ALL documents of that type, regardless of the `Confidential` flag value. This means confidential documents can be:
- Viewed in the PDF viewer
- Downloaded
- Printed
- Emailed (as attachments or links)
- Included in search results

### Solution Summary

Add a `CanAccessConfidential` boolean flag to the existing `UserPermission` table. Modify the centralized `FilterByUserPermissions` query extension to enforce confidential access restrictions at the database query level.

### Benefits

- Minimal schema change (single column addition)
- Centralized enforcement via `FilterByUserPermissions`
- Backward compatible (default = no confidential access)
- Leverages existing permission management infrastructure
- Existing audit logging captures permission changes

---

## Current State

### Document Entity

**File:** `IkeaDocuScan.Infrastructure/Entities/Document.cs`

```csharp
public partial class Document
{
    // ... other properties
    public bool? Confidential { get; set; }  // Line ~94 - EXISTS but NOT ENFORCED
}
```

### UserPermission Entity

**File:** `IkeaDocuScan.Infrastructure/Entities/UserPermission.cs`

```csharp
public partial class UserPermission
{
    public int Id { get; set; }
    public int? DocumentTypeId { get; set; }
    public int UserId { get; set; }
    // NO confidential access flag
}
```

### Current Permission Check

**File:** `IkeaDocuScan.Infrastructure/Extensions/QueryExtensions.cs` (Lines 25-49)

```csharp
public static IQueryable<Document> FilterByUserPermissions(
    this IQueryable<Document> query,
    CurrentUser currentUser,
    AppDbContext context)
{
    if (currentUser.IsSuperUser)
        return query;

    if (!currentUser.HasAccess)
        return query.Where(d => false);

    int userId = currentUser.UserId;

    return query.Where(doc =>
        context.UserPermissions
            .Where(p => p.UserId == userId)
            .Any(perm =>
                doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId
            )
    );
    // NO CHECK for Confidential flag
}
```

### Access Points Affected

| Access Point | File | Line | Method |
|-------------|------|------|--------|
| Get All Documents | DocumentService.cs | ~150 | `GetAllAsync()` |
| Get Document By ID | DocumentService.cs | ~180 | `GetByIdAsync()` |
| Get Document By Barcode | DocumentService.cs | ~210 | `GetByBarCodeAsync()` |
| Get Documents By IDs | DocumentService.cs | ~240 | `GetByIdsAsync()` |
| Search Documents | DocumentService.cs | ~495 | `SearchAsync()` |
| Get Document File | DocumentService.cs | ~1048 | `GetDocumentFileAsync()` |
| Stream Document | DocumentEndpoints.cs | 122 | `/api/documents/{id}/stream` |
| Download Document | DocumentEndpoints.cs | 138 | `/api/documents/{id}/download` |

---

## Target State

### Updated Permission Model

Users will require BOTH:
1. DocumentType permission (existing)
2. `CanAccessConfidential = true` permission (if document is confidential)

### Access Matrix

| User Permission | Document.Confidential | Access Result |
|----------------|----------------------|---------------|
| SuperUser | Any | ALLOWED |
| HasAccess + DocType + CanAccessConfidential | true | ALLOWED |
| HasAccess + DocType + CanAccessConfidential | false/null | ALLOWED |
| HasAccess + DocType (no confidential) | true | DENIED |
| HasAccess + DocType (no confidential) | false/null | ALLOWED |
| No DocType permission | Any | DENIED |

---

## Implementation Phases

```
Phase 1: Database Changes ────────────────────► Migration script
    │
    ▼
Phase 2: Entity and Model Updates ────────────► C# entity changes
    │
    ▼
Phase 3: Service Layer Updates ───────────────► CurrentUserService
    │
    ▼
Phase 4: Query Extension Updates ─────────────► FilterByUserPermissions
    │
    ▼
Phase 5: UI Updates ──────────────────────────► Permission management UI
    │
    ▼
Phase 6: Audit Trail Enhancement ─────────────► Logging confidential access
    │
    ▼
Phase 7: Testing ─────────────────────────────► Verification
```

---

## Phase 1: Database Changes

### Step 1.1: Create Migration

**Command:**
```bash
cd IkeaDocuScanV3/IkeaDocuScan.Infrastructure
dotnet ef migrations add AddConfidentialAccessPermission --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

### Step 1.2: Migration Script Content

The migration should generate something similar to:

```csharp
// File: IkeaDocuScan.Infrastructure/Migrations/YYYYMMDDHHMMSS_AddConfidentialAccessPermission.cs

public partial class AddConfidentialAccessPermission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "CanAccessConfidential",
            table: "UserPermission",
            type: "bit",
            nullable: false,
            defaultValue: false);

        // Add index for query performance
        migrationBuilder.CreateIndex(
            name: "IX_UserPermission_UserId_CanAccessConfidential",
            table: "UserPermission",
            columns: new[] { "UserId", "CanAccessConfidential" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserPermission_UserId_CanAccessConfidential",
            table: "UserPermission");

        migrationBuilder.DropColumn(
            name: "CanAccessConfidential",
            table: "UserPermission");
    }
}
```

### Step 1.3: Apply Migration

**Development:**
```bash
dotnet ef database update --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

**Production:** Generate SQL script for DBA review:
```bash
dotnet ef migrations script --idempotent --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web -o Migrations/ConfidentialAccess.sql
```

### Step 1.4: Verify Migration

```sql
-- Verify column exists
SELECT COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserPermission' AND COLUMN_NAME = 'CanAccessConfidential';

-- Verify all existing records have default value
SELECT COUNT(*) as TotalRecords,
       SUM(CASE WHEN CanAccessConfidential = 0 THEN 1 ELSE 0 END) as DefaultValue
FROM UserPermission;
```

---

## Phase 2: Entity and Model Updates

### Step 2.1: Update UserPermission Entity

**File:** `IkeaDocuScan.Infrastructure/Entities/UserPermission.cs`

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Index("UserId", Name = "IX_UserPermissions_UserId")]
[Index("UserId", "CanAccessConfidential", Name = "IX_UserPermission_UserId_CanAccessConfidential")]
public partial class UserPermission
{
    [Key]
    public int Id { get; set; }

    public int? DocumentTypeId { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// When true, user can access documents marked as Confidential for this DocumentType.
    /// Default is false - users cannot access confidential documents.
    /// </summary>
    public bool CanAccessConfidential { get; set; } = false;

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("UserPermissions")]
    public virtual DocumentType? DocumentType { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserPermissions")]
    public virtual DocuScanUser User { get; set; } = null!;
}
```

### Step 2.2: Update CurrentUser Model

**File:** `IkeaDocuScan.Shared/Models/Authorization/CurrentUser.cs`

```csharp
namespace IkeaDocuScan.Shared.Models.Authorization;

/// <summary>
/// Represents the current authenticated user with their permissions
/// </summary>
public class CurrentUser
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public bool IsSuperUser { get; set; }
    public bool HasAccess { get; set; }
    public DateTime? LastLogon { get; set; }

    /// <summary>
    /// List of document type IDs the user can access (null = all)
    /// </summary>
    public List<int>? AllowedDocumentTypes { get; set; }

    /// <summary>
    /// List of document type IDs where user can access confidential documents.
    /// Null means user has global confidential access (typically SuperUser only).
    /// Empty list means no confidential access at all.
    /// </summary>
    public List<int>? ConfidentialAllowedDocumentTypes { get; set; }

    /// <summary>
    /// Quick check if user has any confidential access permissions
    /// </summary>
    public bool HasAnyConfidentialAccess =>
        IsSuperUser ||
        (ConfidentialAllowedDocumentTypes != null && ConfidentialAllowedDocumentTypes.Count > 0);

    /// <summary>
    /// Check if user can access a specific document type
    /// </summary>
    public bool CanAccessDocumentType(int? documentTypeId)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        if (!documentTypeId.HasValue)
            return true; // No document type restriction

        if (AllowedDocumentTypes == null || AllowedDocumentTypes.Count == 0)
            return true; // No restrictions = access all

        return AllowedDocumentTypes.Contains(documentTypeId.Value);
    }

    /// <summary>
    /// Check if user can access a confidential document of a specific type
    /// </summary>
    public bool CanAccessConfidentialDocumentType(int? documentTypeId)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        // First check basic document type access
        if (!CanAccessDocumentType(documentTypeId))
            return false;

        // Then check confidential access
        if (ConfidentialAllowedDocumentTypes == null)
            return false; // No confidential access at all

        if (ConfidentialAllowedDocumentTypes.Count == 0)
            return false; // Empty list = no confidential access

        // Check if user has confidential access for this specific type
        // or has a global confidential permission (DocumentTypeId = null in permission)
        if (!documentTypeId.HasValue)
            return ConfidentialAllowedDocumentTypes.Count > 0;

        return ConfidentialAllowedDocumentTypes.Contains(documentTypeId.Value) ||
               ConfidentialAllowedDocumentTypes.Contains(0); // 0 represents global confidential access
    }

    /// <summary>
    /// Check if user can access a document based on its document type and confidentiality
    /// </summary>
    public bool CanAccessDocument(int? documentTypeId, bool? isConfidential = false)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        // Check basic document type access
        if (!CanAccessDocumentType(documentTypeId))
            return false;

        // If document is confidential, check confidential access
        if (isConfidential == true)
            return CanAccessConfidentialDocumentType(documentTypeId);

        return true;
    }
}
```

### Step 2.3: Update UserPermissionDto

**File:** `IkeaDocuScan.Shared/DTOs/UserPermissions/UserPermissionDto.cs`

Add to existing DTO:

```csharp
/// <summary>
/// When true, user can access confidential documents for this DocumentType
/// </summary>
public bool CanAccessConfidential { get; set; }
```

### Step 2.4: Update CreateUserPermissionDto

**File:** `IkeaDocuScan.Shared/DTOs/UserPermissions/CreateUserPermissionDto.cs`

Add to existing DTO:

```csharp
/// <summary>
/// When true, user can access confidential documents for this DocumentType
/// </summary>
public bool CanAccessConfidential { get; set; } = false;
```

### Step 2.5: Update UpdateUserPermissionDto

**File:** `IkeaDocuScan.Shared/DTOs/UserPermissions/UpdateUserPermissionDto.cs`

Add to existing DTO:

```csharp
/// <summary>
/// When true, user can access confidential documents for this DocumentType
/// </summary>
public bool CanAccessConfidential { get; set; }
```

---

## Phase 3: Service Layer Updates

### Step 3.1: Update CurrentUserService

**File:** `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/CurrentUserService.cs`

Locate the method that loads user permissions (around line 92-117) and modify:

```csharp
// After loading UserPermissions, extract confidential permissions
// Find the section where AllowedDocumentTypes is populated

// EXISTING CODE (around line 113-117):
// Extracts distinct DocumentTypeId values into AllowedDocumentTypes

// ADD AFTER EXISTING CODE:
// Extract document types where user has confidential access
var confidentialPermissions = await _context.UserPermissions
    .Where(p => p.UserId == userId && p.CanAccessConfidential)
    .Select(p => p.DocumentTypeId)
    .Distinct()
    .ToListAsync();

// Handle null DocumentTypeId (global confidential access) as 0
currentUser.ConfidentialAllowedDocumentTypes = confidentialPermissions
    .Select(dtId => dtId ?? 0)  // Convert null to 0 for global access
    .ToList();

// If user has a permission with null DocumentTypeId and CanAccessConfidential,
// they have global confidential access
if (confidentialPermissions.Any(dtId => dtId == null))
{
    // User has global confidential access - set to null to indicate "all"
    // Or keep the list with 0 to indicate global
}
```

**Complete updated section example:**

```csharp
// Load all user permissions
var permissions = await _context.UserPermissions
    .Where(p => p.UserId == userId)
    .ToListAsync();

// Extract allowed document types
var allowedTypes = permissions
    .Where(p => p.DocumentTypeId.HasValue)
    .Select(p => p.DocumentTypeId!.Value)
    .Distinct()
    .ToList();

// Check for global access (null DocumentTypeId)
bool hasGlobalAccess = permissions.Any(p => !p.DocumentTypeId.HasValue);

currentUser.AllowedDocumentTypes = hasGlobalAccess ? null : allowedTypes;

// Extract confidential access permissions
var confidentialTypes = permissions
    .Where(p => p.CanAccessConfidential)
    .Select(p => p.DocumentTypeId ?? 0)  // 0 = global confidential
    .Distinct()
    .ToList();

currentUser.ConfidentialAllowedDocumentTypes = confidentialTypes.Count > 0
    ? confidentialTypes
    : new List<int>();  // Empty list = no confidential access
```

### Step 3.2: Update UserPermissionService

**File:** `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/UserPermissionService.cs`

Update mapping methods to include `CanAccessConfidential`:

```csharp
// In CreateAsync method - map CanAccessConfidential from DTO to entity
var permission = new UserPermission
{
    UserId = dto.UserId,
    DocumentTypeId = dto.DocumentTypeId,
    CanAccessConfidential = dto.CanAccessConfidential  // ADD THIS
};

// In UpdateAsync method - update CanAccessConfidential
existingPermission.DocumentTypeId = dto.DocumentTypeId;
existingPermission.CanAccessConfidential = dto.CanAccessConfidential;  // ADD THIS

// In mapping to DTO - include CanAccessConfidential
private static UserPermissionDto MapToDto(UserPermission entity)
{
    return new UserPermissionDto
    {
        Id = entity.Id,
        UserId = entity.UserId,
        DocumentTypeId = entity.DocumentTypeId,
        DocumentTypeName = entity.DocumentType?.DtName,
        CanAccessConfidential = entity.CanAccessConfidential  // ADD THIS
    };
}
```

---

## Phase 4: Query Extension Updates

### Step 4.1: Update FilterByUserPermissions

**File:** `IkeaDocuScan.Infrastructure/Extensions/QueryExtensions.cs`

**Replace the entire method:**

```csharp
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IQueryable to support permission-based filtering
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Filters documents based on user permissions.
    /// A document is accessible if:
    /// 1. User is SuperUser (bypasses all checks), OR
    /// 2. User has DocumentType permission AND
    ///    - Document is not confidential, OR
    ///    - Document is confidential AND user has CanAccessConfidential for that type
    ///
    /// SuperUser bypass: If user is SuperUser, no filtering is applied.
    /// No access: If user has no access, returns empty result.
    /// </summary>
    /// <param name="query">The document query to filter</param>
    /// <param name="currentUser">The current authenticated user with permissions</param>
    /// <param name="context">Database context for UserPermission lookups</param>
    /// <returns>Filtered queryable of documents</returns>
    public static IQueryable<Document> FilterByUserPermissions(
        this IQueryable<Document> query,
        CurrentUser currentUser,
        AppDbContext context)
    {
        // SuperUser bypass - sees all documents including confidential
        if (currentUser.IsSuperUser)
            return query;

        // No access - sees no documents
        if (!currentUser.HasAccess)
            return query.Where(d => false); // Empty result

        int userId = currentUser.UserId;

        // Filter documents where user has appropriate permissions
        return query.Where(doc =>
            context.UserPermissions
                .Where(p => p.UserId == userId)
                .Any(perm =>
                    // DocumentType filter: match if both null OR values equal
                    (doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId)
                    &&
                    // Confidential filter:
                    // - Non-confidential documents: always accessible if DocType matches
                    // - Confidential documents: require CanAccessConfidential permission
                    (
                        doc.Confidential != true  // Not confidential - allow access
                        ||
                        perm.CanAccessConfidential  // Is confidential but user has permission
                    )
                )
        );
    }

    /// <summary>
    /// Filters documents excluding confidential ones entirely.
    /// Use this for public-facing queries or reports that should never include confidential documents.
    /// </summary>
    public static IQueryable<Document> ExcludeConfidential(this IQueryable<Document> query)
    {
        return query.Where(d => d.Confidential != true);
    }

    /// <summary>
    /// Filters to only confidential documents.
    /// Use this for confidential document management views.
    /// </summary>
    public static IQueryable<Document> OnlyConfidential(this IQueryable<Document> query)
    {
        return query.Where(d => d.Confidential == true);
    }
}
```

### Step 4.2: Verify All Usages

Search the codebase for all calls to `FilterByUserPermissions` and verify they will work with the updated logic:

**Expected locations:**
- `DocumentService.cs` - Multiple methods
- Any custom queries that access documents

```bash
# Search command to find all usages
grep -rn "FilterByUserPermissions" --include="*.cs"
```

---

## Phase 5: UI Updates

### Step 5.1: Update EditUserPermissions Page

**File:** `IkeaDocuScan-Web.Client/Pages/EditUserPermissions.razor`

Add a checkbox column for confidential access in the permissions grid:

```razor
@* In the permissions table header *@
<th>Document Type</th>
<th>Can Access Confidential</th>  @* ADD THIS COLUMN *@
<th>Actions</th>

@* In the permissions table body *@
<td>@permission.DocumentTypeName</td>
<td>
    <input type="checkbox"
           checked="@permission.CanAccessConfidential"
           @onchange="@(e => OnConfidentialAccessChanged(permission, (bool)e.Value!))"
           class="form-check-input" />
    @if (permission.CanAccessConfidential)
    {
        <span class="badge bg-warning text-dark ms-2">
            <i class="bi bi-shield-lock"></i> Confidential
        </span>
    }
</td>
<td>
    @* existing action buttons *@
</td>
```

**Add the event handler in the code section:**

```csharp
private async Task OnConfidentialAccessChanged(UserPermissionDto permission, bool newValue)
{
    try
    {
        permission.CanAccessConfidential = newValue;
        await UserPermissionService.UpdateAsync(new UpdateUserPermissionDto
        {
            Id = permission.Id,
            UserId = permission.UserId,
            DocumentTypeId = permission.DocumentTypeId,
            CanAccessConfidential = newValue
        });

        // Show success message
        await ShowSuccessMessage($"Confidential access {(newValue ? "granted" : "revoked")} for {permission.DocumentTypeName}");
    }
    catch (Exception ex)
    {
        // Revert on error
        permission.CanAccessConfidential = !newValue;
        await ShowErrorMessage($"Failed to update: {ex.Message}");
    }
}
```

### Step 5.2: Update Add Permission Modal

When adding a new permission, include the confidential access option:

```razor
<div class="mb-3">
    <label class="form-label">Document Type</label>
    <select class="form-select" @bind="newPermission.DocumentTypeId">
        <option value="">-- All Document Types --</option>
        @foreach (var docType in documentTypes)
        {
            <option value="@docType.DtId">@docType.DtName</option>
        }
    </select>
</div>

<div class="mb-3 form-check">
    <input type="checkbox" class="form-check-input" id="canAccessConfidential"
           @bind="newPermission.CanAccessConfidential" />
    <label class="form-check-label" for="canAccessConfidential">
        <i class="bi bi-shield-lock text-warning"></i>
        Can Access Confidential Documents
    </label>
    <small class="form-text text-muted d-block">
        When enabled, user can view, download, and print documents marked as confidential.
    </small>
</div>
```

### Step 5.3: Add Visual Indicators for Confidential Documents

**File:** `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor`

Add a confidential badge in search results:

```razor
@* In the document row *@
<td>
    @document.BarCode
    @if (document.Confidential == true)
    {
        <span class="badge bg-danger ms-1" title="Confidential Document">
            <i class="bi bi-lock-fill"></i>
        </span>
    }
</td>
```

**File:** `IkeaDocuScan-Web.Client/Pages/DocumentPreview.razor`

Add confidential warning banner:

```razor
@if (document?.Confidential == true)
{
    <div class="alert alert-warning d-flex align-items-center mb-3" role="alert">
        <i class="bi bi-shield-exclamation me-2 fs-4"></i>
        <div>
            <strong>Confidential Document</strong> -
            This document contains sensitive information.
            Do not share or distribute without authorization.
        </div>
    </div>
}
```

### Step 5.4: Update Access Denied Message

When a user tries to access a confidential document without permission, show a specific message:

**File:** `IkeaDocuScan-Web.Client/Pages/DocumentPreview.razor`

```razor
@if (accessDenied)
{
    <div class="alert alert-danger">
        <h4><i class="bi bi-shield-x"></i> Access Denied</h4>
        <p>You do not have permission to view this confidential document.</p>
        <p>If you believe you should have access, please contact your administrator.</p>
        <a href="/documents/search" class="btn btn-primary">Return to Search</a>
    </div>
}
```

---

## Phase 6: Audit Trail Enhancement

### Step 6.1: Log Confidential Document Access

**File:** `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/DocumentService.cs`

In methods that access document files (`GetDocumentFileAsync`, etc.), add audit logging for confidential access:

```csharp
public async Task<DocumentFileDto?> GetDocumentFileAsync(int id)
{
    var currentUser = await _currentUserService.GetCurrentUserAsync();

    // ... existing query code ...

    var document = await query.FirstOrDefaultAsync();

    if (document == null)
        return null;

    // Log confidential document access
    if (document.Confidential == true)
    {
        await _auditTrailService.LogAsync(
            AuditAction.Read,
            $"CONFIDENTIAL document accessed: BarCode={document.BarCode}, Name={document.Name}",
            currentUser.AccountName,
            document.BarCode.ToString()
        );
    }

    // ... rest of method ...
}
```

### Step 6.2: Add Confidential-Specific Audit Action (Optional)

**File:** `IkeaDocuScan.Shared/Enums/AuditAction.cs`

```csharp
public enum AuditAction
{
    Create,
    Read,
    Update,
    Delete,
    CheckIn,
    Export,
    AccessRequest,
    Login,
    Logout,
    ConfidentialAccess,    // ADD: Specifically for confidential document access
    ConfidentialDenied     // ADD: When access to confidential doc is denied
}
```

### Step 6.3: Log Permission Changes

Ensure the existing `PermissionChangeAuditLog` captures confidential permission changes. This should happen automatically if the `UserPermissionService` update triggers the audit log.

Verify in `UserPermissionService.UpdateAsync`:

```csharp
// Log the permission change including confidential access
var auditEntry = new PermissionChangeAuditLog
{
    // ... existing fields ...
    OldValue = JsonSerializer.Serialize(new {
        DocumentTypeId = oldPermission.DocumentTypeId,
        CanAccessConfidential = oldPermission.CanAccessConfidential
    }),
    NewValue = JsonSerializer.Serialize(new {
        DocumentTypeId = dto.DocumentTypeId,
        CanAccessConfidential = dto.CanAccessConfidential
    }),
    ChangeType = "PermissionModified"
};
```

---

## Phase 7: Testing

### Step 7.1: Unit Test Cases

Create test file: `IkeaDocuScan.Tests/ConfidentialAccessTests.cs`

```csharp
public class ConfidentialAccessTests
{
    // Test 1: SuperUser can access confidential documents
    [Fact]
    public async Task SuperUser_CanAccessConfidentialDocument()
    {
        // Arrange
        var currentUser = new CurrentUser { IsSuperUser = true, HasAccess = true };
        var document = new Document { Confidential = true, DtId = 1 };

        // Act
        var canAccess = currentUser.CanAccessDocument(document.DtId, document.Confidential);

        // Assert
        Assert.True(canAccess);
    }

    // Test 2: User without confidential permission cannot access
    [Fact]
    public async Task User_WithoutConfidentialPermission_CannotAccessConfidentialDocument()
    {
        // Arrange
        var currentUser = new CurrentUser
        {
            IsSuperUser = false,
            HasAccess = true,
            AllowedDocumentTypes = new List<int> { 1 },
            ConfidentialAllowedDocumentTypes = new List<int>()  // Empty = no access
        };

        // Act
        var canAccess = currentUser.CanAccessDocument(1, true);

        // Assert
        Assert.False(canAccess);
    }

    // Test 3: User with confidential permission can access
    [Fact]
    public async Task User_WithConfidentialPermission_CanAccessConfidentialDocument()
    {
        // Arrange
        var currentUser = new CurrentUser
        {
            IsSuperUser = false,
            HasAccess = true,
            AllowedDocumentTypes = new List<int> { 1 },
            ConfidentialAllowedDocumentTypes = new List<int> { 1 }
        };

        // Act
        var canAccess = currentUser.CanAccessDocument(1, true);

        // Assert
        Assert.True(canAccess);
    }

    // Test 4: User can access non-confidential without confidential permission
    [Fact]
    public async Task User_WithoutConfidentialPermission_CanAccessNonConfidentialDocument()
    {
        // Arrange
        var currentUser = new CurrentUser
        {
            IsSuperUser = false,
            HasAccess = true,
            AllowedDocumentTypes = new List<int> { 1 },
            ConfidentialAllowedDocumentTypes = new List<int>()
        };

        // Act
        var canAccess = currentUser.CanAccessDocument(1, false);

        // Assert
        Assert.True(canAccess);
    }

    // Test 5: Global confidential permission (DocumentTypeId = null)
    [Fact]
    public async Task User_WithGlobalConfidentialPermission_CanAccessAnyConfidentialDocument()
    {
        // Arrange
        var currentUser = new CurrentUser
        {
            IsSuperUser = false,
            HasAccess = true,
            AllowedDocumentTypes = null,  // Global document access
            ConfidentialAllowedDocumentTypes = new List<int> { 0 }  // 0 = global confidential
        };

        // Act
        var canAccess = currentUser.CanAccessDocument(999, true);  // Any document type

        // Assert
        Assert.True(canAccess);
    }
}
```

### Step 7.2: Integration Test Checklist

Manual testing with test user profiles:

| Test Case | User Profile | Document | Expected Result |
|-----------|-------------|----------|-----------------|
| 1 | superuser | Confidential Doc Type 1 | Can view, download, print |
| 2 | publisher (no conf perm) | Confidential Doc Type 1 | Access Denied |
| 3 | publisher (with conf perm) | Confidential Doc Type 1 | Can view, download, print |
| 4 | publisher (no conf perm) | Non-confidential Doc Type 1 | Can view, download, print |
| 5 | reader (no conf perm) | Confidential Doc Type 1 | Access Denied |
| 6 | reader (with conf perm) | Confidential Doc Type 1 | Can view (based on role) |

### Step 7.3: API Testing

```http
### Test 1: Get confidential document without permission (should return 404/empty)
GET /api/documents/{confidential-doc-id}
Authorization: [user-without-confidential-access]

### Test 2: Get confidential document with permission (should return document)
GET /api/documents/{confidential-doc-id}
Authorization: [user-with-confidential-access]

### Test 3: Search should exclude confidential documents for unauthorized users
POST /api/documents/search
Authorization: [user-without-confidential-access]
Content-Type: application/json
{
    "confidential": true
}
# Should return empty results

### Test 4: Stream confidential document without permission
GET /api/documents/{confidential-doc-id}/stream
Authorization: [user-without-confidential-access]
# Should return 404

### Test 5: Download confidential document with permission
GET /api/documents/{confidential-doc-id}/download
Authorization: [user-with-confidential-access]
# Should return file
```

### Step 7.4: UI Testing Checklist

- [ ] Permission management page shows "Can Access Confidential" checkbox
- [ ] Adding new permission includes confidential option
- [ ] Editing permission allows toggling confidential access
- [ ] Confidential documents show visual indicator (badge/icon) in search results
- [ ] Document preview shows confidential warning banner
- [ ] Access denied page shows when unauthorized user tries to view confidential doc
- [ ] Audit trail logs confidential document access

---

## Rollback Plan

### If Issues Occur After Deployment

**Step 1: Revert Code Changes**
```bash
git revert <commit-hash>
```

**Step 2: Rollback Database Migration**
```bash
dotnet ef database update <previous-migration-name> --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

Or manually:
```sql
-- Remove the new column
ALTER TABLE UserPermission DROP COLUMN CanAccessConfidential;

-- Remove the index
DROP INDEX IF EXISTS IX_UserPermission_UserId_CanAccessConfidential ON UserPermission;
```

**Step 3: Verify Application Functions**
- Test document access works as before
- Verify no errors in logs

---

## Security Considerations

### 1. SuperUser Bypass

SuperUsers bypass ALL confidential checks. Ensure SuperUser accounts are:
- Limited to essential personnel
- Subject to regular access reviews
- Monitored via audit logs

### 2. Permission Escalation

Risk: Admin grants `CanAccessConfidential` too broadly.

Mitigation:
- Add confirmation dialog when granting confidential access
- Log all confidential permission grants
- Regular permission audits

### 3. Cache Invalidation

If `CurrentUser` is cached, confidential permissions must be refreshed when:
- User's permissions are modified
- User logs in again

Verify `CurrentUserService` caching behavior and ensure permissions are reloaded appropriately.

### 4. Client-Side Security

The UI restrictions (hiding buttons, showing access denied) are NOT security controls. All actual security is enforced at the API/service layer via `FilterByUserPermissions`.

Never rely on client-side code to enforce confidentiality.

### 5. Print/Export Considerations

Even with view access, consider whether confidential documents should be:
- Printable (add watermarks?)
- Exportable to Excel
- Emailable

These may require additional business rules beyond this implementation.

### 6. Scanned Files Gap

**Important:** The `/api/scannedfiles/*` endpoints do NOT use `FilterByUserPermissions`. Confidential documents should be checked in promptly so they fall under document permission protection.

Consider adding a separate confidential check for scanned files if they may contain sensitive pre-check-in content.

---

## Post-Implementation Checklist

- [ ] Migration applied successfully
- [ ] All existing users default to `CanAccessConfidential = false`
- [ ] SuperUsers can access all confidential documents
- [ ] Regular users cannot access confidential documents by default
- [ ] Granting confidential permission allows access
- [ ] Revoking confidential permission denies access
- [ ] Audit logs capture confidential document access
- [ ] UI shows confidential indicators
- [ ] Permission management UI works correctly
- [ ] No performance degradation in document queries
- [ ] Documentation updated

---

## References

- Current QueryExtensions: `IkeaDocuScan.Infrastructure/Extensions/QueryExtensions.cs`
- UserPermission Entity: `IkeaDocuScan.Infrastructure/Entities/UserPermission.cs`
- CurrentUser Model: `IkeaDocuScan.Shared/Models/Authorization/CurrentUser.cs`
- CurrentUserService: `IkeaDocuScan-Web/Services/CurrentUserService.cs`
- DocumentService: `IkeaDocuScan-Web/Services/DocumentService.cs`
- DocumentEndpoints: `IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs`
- EditUserPermissions UI: `IkeaDocuScan-Web.Client/Pages/EditUserPermissions.razor`
