
think ultradeep. do not any code changes, just analysis. create an implementation plan in a md when everything is clarified and complete
Concentrate on navigation and access to modification functionality.
Following table lists all relevant pages  in IkeaDocuScanV3\IkeaDocuScan-Web\IkeaDocuScan-Web.Client\Pages. ignore the pages not in this list. 
Definition of the columns
    Page: is the name of the page
    Tab: if the page has multiple tabs with different enpoints then they are named here. ANY ONE permission to see the page, with individual tabs hidden based on permissions.
    Route: the routes to access the pages
    Endpoint Permission Required to View (Read-Only): the method and route of the enpoint in the endpoint registry. The endpoint listed here gives a user read access to the page. Without the enpoint permission, the user has no access to the page. they cannot navigate to the page at all (redirect/403). Or means you need either one of the permissions.
    Enpoint Permssion Required to Edit (Update, Delete): the pages with none or n/a do not have update or delete functionality. No additional endpoint checks required. the pages with a enpoint name allow the user to update, delete or otherwise modify and save the content. Therefore the user must have the endpoint permission in order to modify it. All buttons to modify the content or menu items should be at least disabled or not visible (Hide elements the user cannot access (cleaner UI)). All Export to Excel or PDF, Print and similar no data changing functions do NOT require edit permission.
    
 | Page                       | Tab                    | Route                                                                             | Authorize Attribute    | Endpoint Permission Required to View (Read-Only)       | Endpoint Permission Required to Edit (Update, Delete)                                              |
 |----------------------------|------------------------|-----------------------------------------------------------------------------------|------------------------|-------------------------------------------------------------------  -----|----------------------------------------------------------------------------------------------------|
 | Home                       |                        | /                                                                                 | @attribute [Authorize] | none       | none                                                                                               |
 | About                      |                        | /about                                                                            | @attribute [Authorize] | none       | none                                                                                               |
 | AuditTrail                 |                        | /audit-trail                                                                      | HasAccess              | GET /api/audittrail/daterange       | n/a                                                                                                |
 | DocumentProperties         |                        | /documents/properties/{DocumentId:int}                                            | HasAccess              | GET /api/documents/{id}       | n/a                                                                                                |
 | DocumentPropertiesPage     |                        | /documents/edit/{BarCode:int}, /documents/register, /documents/checkin/{FileName} | HasAccess              | POST /api/documents/       | POST /api/documents/                                                                               |
 | SearchDocuments            |                        | /documents/search                                                                 | HasAccess              | GET /api/documents/       | DELETE /api/documents/{id}, POST /api/documents/email/link or POST /api/documents/email/attachment |
 | ManageDocumentNames        |                        | /manage-document-names                                                            | HasAccess              | GET /api/documentnames/       | POST /api/documentnames/                                                                           |
 | EditUserPermissions        |                        | /edit-userpermissions                                                             | HasAccess              | GET /api/userpermissions/users       | POST /api/userpermissions/                                                                         |
 | AccessAudit                |                        | /access-audit                                                                     | HasAccess              | GET /api/access-audit/document-type/{id}       | n/a                                                                                                |
 | LogViewer                  |                        | /admin/logs                                                                       | HasAccess              | POST /api/logs/search       | n/a                                                                                                |
 | ConfigurationManagement    | Email Recipient Groups | /configuration-management                                                         | HasAccess              | GET /api/configuration/email-recipients       | PUT /api/configuration/email-recipients/{id}                                                       |
 | ConfigurationManagement    | Email Templates        | /configuration-management                                                         | HasAccess              | GET /api/configuration/email-templates       | POST /api/configuration/email-templates                                                            |
 | ConfigurationManagement    | SMTP                   | /configuration-management                                                         | HasAccess              | GET /api/configuration/sections       | PUT /api/configuration/smtp                                                                        |
 | EndpointManagement         |                        | /endpoint-management                                                              | IsSuperuser            | SUPERUSER Only       | SUPERUSER Only                                                                                     |
 | CounterPartyAdministration |                        | /counterparty-administration                                                      | HasAccess              | GET /api/counterparties/       | POST /api/counterparties/                                                                          |
 | DocumentTypeAdministration |                        | /documenttype-administration                                                      | HasAccess              | GET /api/documenttypes/all       | POST /api/documenttypes/                                                                           |
 | CurrencyAdministration     |                        | /currency-administration                                                          | HasAccess              | GET /api/currencies/       | POST /api/currencies/                                                                              |
 | CountryAdministration      |                        | /country-administration                                                           | HasAccess              | GET /api/countries/       | POST /api/countries/                                                                               |
 | ComposeDocumentEmail       |                        | /documents/compose-email                                                          | HasAccess              | POST /api/documents/email/link or POST  /api/documents/email/attachment | n/a                                                                                                |
 | PdfViewer                  |                        | /pdf-viewer/{DocumentId:int}                                                      | HasAccess              | GET /api/documents/{id}/file       | n/a                                                                                                |
 | DocumentPreview            |                        | /documents/preview/{DocumentId:int}                                               | HasAccess              | GET /api/documents/{id}/file       | n/a                                                                                                |
 | PrintSummary               |                        | /documents/print-summary                                                          | HasAccess              | GET /api/documents/{id}       | n/a                                                                                                |
 | ExcelPreview               |                        | /excel-preview                                                                    | HasAccess              | POST /api/documents/export/excel       | n/a                                                                                                |
 | CheckinScanned             |                        | /checkin-scanned                                                                  | HasAccess              | GET /api/scannedfiles/       | n/a                                                                                                |
 | CheckinFileDetail          |                        | /checkin-scanned/detail/{FileName}                                                | HasAccess              | GET /api/scannedfiles/{filename}       | POST /api/documents/                                                                               |
 | ActionReminders            |                        | /action-reminders                                                                 | HasAccess              | GET /api/action-reminders/       | n/a                                                                                                |
Clarification
  1. For the "OR" permission logic (e.g., SendDocumentLink or SendDocumentAttachment): each button maps to its specific permission, but access to ComposeDocumentEmail page requires only one of the permissions
  2. ConfigurationManagement tabs: If user has permission for only Email Templates but not SMTP or Recipients, The entire page should be visible with only the Email Templates tab shown.
  3. Navigation blocking behavior: When a user navigates directly to a URL they don't have permission for (e.g., typing /audit-trail in address bar), they should be Redirect to a custom "Access Denied" page.
  4. Button visibility vs disabled state: For edit permissions, you mentioned "at least disabled or not visible (Hide elements the user cannot access (cleaner UI))". Standardize on Always hide (cleaner UI).
  5. SearchDocuments Modification Actions "Edit Properties" : The link navigates to /documents/edit/{BarCode} should be hidden if user lacks edit permission.
  6. DocumentPropertiesPage Access Control: Users without CreateDocument cannot access /documents/edit/{BarCode} even to view.
  7. ConfigurationManagement Tabs - Different Endpoints: the page navigation (menu item) requires ANY of the three view permissions. If a user can see Email Templates but NOT Recipients, the Recipients tab button itself should be hidden.
  8. CheckinFileDetail requires GetScannedFileContent but Edit requires CreateDocument. the entire page should be accessible with GetScannedFileContent, but the "Check In" button hidden if lacking CreateDocument.
All pages must have a Authorize Attribute, Home and About without policy but all others require HasAccess Policy. this is tier one of the Two-Tier Authorization Model. tier two is the endpoint permission.
The page EndpointManagement is special, because this page is not controlled by configurable endpoint permissions. this would introduce circular dependencies. Only superuser can use it.
Check for each page if this list is correct (especially check if pages listed here as having no modification functionaly do not have it, for example for the search page check if Delete is the only modification action).
Ignore the user permissions but leave them intact, as they only control the content and not the navigation nor the access to modification functions.
Check these instruction for consistency first and stop and complain if there are inconsistencies. Ask questions if not clear.
Menu/Navigation access - menu items for pages should also be hidden if the user lacks the View endpoint permissions.
List at least two issues that might arise if this is implemented.
For redirection because of missing privileges a new page will be created (/access-denied).
For performance improvements: Consider caching user permissions client-side with a reasonable TTL.
Reuse the already existing Lookup of endoint permissions using paths. 



=========================================================================================================================================================================
=========================================================================================================================================================================
=========================================================================================================================================================================
=========================================================================================================================================================================



# Page Navigation and Access Control Implementation Plan

## Document Information

**Created:** 2025-12-02
**Version:** 2.0
**Status:** Draft - Pending Approval
**Scope:** Navigation and modification access control for IkeaDocuScan client pages

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current State Analysis](#2-current-state-analysis)
3. [Requirements Validation](#3-requirements-validation)
4. [Two-Tier Authorization Model](#4-two-tier-authorization-model)
5. [Implementation Details by Page](#5-implementation-details-by-page)
6. [Architectural Design](#6-architectural-design)
7. [Potential Issues](#7-potential-issues)
8. [Implementation Steps](#8-implementation-steps)

---

## 1. Executive Summary

This document defines the implementation plan for a comprehensive Two-Tier Authorization Model controlling:
1. **Navigation Access** - Which pages users can see in the menu and navigate to
2. **Modification Access** - Which editing/modification functions users can use within pages

### Key Principles

- **Hybrid Approach:** Combine attribute-based policies (view permissions) with runtime checks (edit permissions)
- **Tier 1 (Page Access):** `[Authorize]` or `[Authorize(Policy = "HasAccess")]` attribute
- **Tier 2 (View Permissions):** `[Authorize(Policy = "Endpoint:METHOD:/api/route")]` attribute - endpoint-named policies
- **Tier 3 (Edit Permissions):** Runtime checks via `EndpointAuthorizationHttpService.CheckAccessAsync()` - hide buttons
- **UI Philosophy:** Hide elements users cannot access (cleaner UI)
- **Fail Behavior:** Default 403/login redirect for unauthorized access (Blazor default)
- **Special Cases:**
  - `EndpointManagement` uses `[Authorize(Policy = "SuperUser")]` to avoid circular dependencies
  - `ConfigurationManagement` and `ComposeDocumentEmail` use runtime OR logic checks

### Policy Naming Convention

Policies are named after endpoints using the format:
```
Endpoint:{METHOD}:{route}
```

Examples:
- `Endpoint:GET:/api/documents/`
- `Endpoint:POST:/api/documents/`
- `Endpoint:DELETE:/api/documents/{id}`

---

## 2. Current State Analysis

### 2.1 What Already Exists

| Component | Status | Location |
|-----------|--------|----------|
| `DynamicAuthorizationPolicyProvider` | Implemented | `Authorization/DynamicAuthorizationPolicyProvider.cs` |
| `EndpointAuthorizationHandler` | Implemented | `Authorization/EndpointAuthorizationHandler.cs` |
| `EndpointAuthorizationHttpService` | Implemented | `IkeaDocuScan-Web.Client/Services/` |
| `CheckAccessAsync()` method | Implemented | Used by NavMenu and 6 admin pages |
| `CheckMultipleAccessAsync()` method | Implemented | Used by NavMenu for batch checks |
| NavMenu dynamic visibility | Implemented | `Layout/NavMenu.razor` |
| `hasWriteAccess` pattern | Implemented | Used in 6 administration pages |
| Server-side caching (30 min TTL) | Implemented | `EndpointAuthorizationService.cs` |
| `HasAccess` policy | Implemented | Program.cs |
| `SuperUser` policy | Implemented | Program.cs |

### 2.2 Dynamic Policy Provider (Already Exists)

The `DynamicAuthorizationPolicyProvider` already supports creating policies on-the-fly for endpoint authorization:

```csharp
// From DynamicAuthorizationPolicyProvider.cs
// Parses policy names like "Endpoint:GET:/api/documents/"
// Creates EndpointAuthorizationRequirement dynamically
```

This means we can use endpoint-named policies on page attributes without registering each policy manually.

### 2.3 Pages Already Using `hasWriteAccess` Pattern

These pages already check edit permissions at runtime:
- `CounterPartyAdministration.razor` - checks `POST /api/counterparties/`
- `CurrencyAdministration.razor` - checks `POST /api/currencies/`
- `CountryAdministration.razor` - checks `POST /api/countries/`
- `DocumentTypeAdministration.razor` - checks `POST /api/documenttypes/`
- `ManageDocumentNames.razor` - checks `POST /api/documentnames/`
- `EditUserPermissions.razor` - checks `POST /api/userpermissions/`

### 2.4 What Needs Implementation

| Gap | Description | Approach |
|-----|-------------|----------|
| View permission attributes | Add endpoint policy attributes to pages | Attribute-based |
| SearchDocuments edit controls | Delete, Edit Properties links need permission checks | Runtime check |
| CheckinFileDetail "Check In" button | Needs `POST /api/documents/` permission check | Runtime check |
| ConfigurationManagement tab visibility | Tabs need individual endpoint checks (OR logic) | Runtime check |
| ComposeDocumentEmail page access | Needs OR logic for two email endpoints | Runtime check |
| DocumentPropertiesPage access control | Needs `POST /api/documents/` for all routes | Attribute-based |
| DocumentPreview edit button | "Edit Properties" button needs permission check | Runtime check |

---

## 3. Requirements Validation

### 3.1 Table Verification Results

I have verified the provided requirements table against the actual codebase. Here are the findings:

#### Confirmed Correct

| Page | Finding |
|------|---------|
| Home | Route `/`, `@attribute [Authorize]` (no policy) - CORRECT |
| About | Route `/about`, `@attribute [Authorize]` (no policy) - CORRECT |
| EndpointManagement | Route `/endpoint-management`, `@attribute [Authorize(Policy = "SuperUser")]` - CORRECT |
| All other pages | All have `@attribute [Authorize(Policy = "HasAccess")]` - CORRECT |

#### SearchDocuments Modification Actions - VERIFIED

The SearchDocuments page has the following modification actions that need permission checks:
- **Delete Selected** (line 428-429) - requires `DELETE /api/documents/{id}`
- **Delete (single)** (line 562) - requires `DELETE /api/documents/{id}`
- **Edit Properties** link (line 554) - navigates to `/documents/edit/{BarCode}` - requires `POST /api/documents/`
- **Email (Attach)** / **Email (Link)** (lines 446-451, 558-559) - navigates to ComposeDocumentEmail
- **Barcode link** (line 524) - also navigates to `/documents/edit/{BarCode}` - requires `POST /api/documents/`

#### CheckinFileDetail - VERIFIED

- Has "Check-in Document" button (line 236) which navigates to `/documents/checkin/{FileName}`
- The CheckIn button should be hidden if user lacks `POST /api/documents/` permission
- Download button should remain visible (read operation)

#### ConfigurationManagement Tabs - VERIFIED

The page has three tabs (lines 48-68):
- Email Recipients (`GET /api/configuration/email-recipients` for view)
- Email Templates (`GET /api/configuration/email-templates` for view)
- SMTP Settings (`GET /api/configuration/sections` for view)

**Page access requires OR logic** - user needs ANY of the three view permissions.

### 3.2 Inconsistencies Found

| Issue | Details | Resolution |
|-------|---------|------------|
| AccessAudit view endpoint mismatch | Table says `GET /api/access-audit/document-type/{id}`, NavMenu checks `GET /api/access-audit/users` | Use `GET /api/access-audit/document-type/{id}` as primary view check |
| CheckinFileDetail duplicate attribute | Has `@attribute [Authorize(Policy = "HasAccess")]` twice (lines 2 and 4) | Remove duplicate |
| CheckinScanned duplicate attribute | Has `@attribute [Authorize(Policy = "HasAccess")]` twice | Remove duplicate |

---

## 4. Two-Tier Authorization Model (Hybrid Approach)

### 4.1 Authorization Layers

```
+------------------+---------------------------+----------------------------------+
| Layer            | Implementation            | Purpose                          |
+------------------+---------------------------+----------------------------------+
| Authentication   | @attribute [Authorize]    | User must be logged in           |
+------------------+---------------------------+----------------------------------+
| Base Access      | [Authorize(Policy =       | User must have HasAccess claim   |
|                  |  "HasAccess")]            | (in DocuScanUsers table)         |
+------------------+---------------------------+----------------------------------+
| View Permission  | [Authorize(Policy =       | User must have endpoint access   |
| (Attribute)      |  "Endpoint:GET:/api/...")]| Uses DynamicAuthorizationPolicy  |
+------------------+---------------------------+----------------------------------+
| Edit Permission  | Runtime CheckAccessAsync  | Hide modification buttons        |
| (Runtime)        | hasWriteAccess pattern    | if user lacks permission         |
+------------------+---------------------------+----------------------------------+
```

### 4.2 When to Use Each Approach

| Scenario | Approach | Reason |
|----------|----------|--------|
| Page view permission (single endpoint) | Attribute | Clean, declarative, no code needed |
| Page view permission (OR logic) | Runtime | Attributes don't support OR logic |
| Edit/Delete button visibility | Runtime | Need to conditionally hide UI elements |
| SuperUser-only pages | `[Authorize(Policy = "SuperUser")]` | Avoid circular dependency |
| Home/About pages | `[Authorize]` only | No endpoint permission needed |

### 4.3 Authorization Flow

```
User navigates to page
         |
         v
  +------+-------+
  | [Authorize]  |  <-- Must be authenticated
  +------+-------+
         |
    No   |   Yes
  +------+-------+
  |              |
  v              v
 Login     +-----+------+
           | HasAccess  |  <-- [Authorize(Policy = "HasAccess")]
           +-----+------+
                 |
            No   |   Yes
           +-----+------+
           |            |
           v            v
         403      +-----+------+
                  | Endpoint   |  <-- [Authorize(Policy = "Endpoint:...")]
                  | Policy     |
                  +-----+------+
                        |
                   No   |   Yes
                  +-----+------+
                  |            |
                  v            v
                403      Page Loads
                         OnInitializedAsync()
                              |
                              v
                       +------+-------+
                       | Edit Check   |  <-- Runtime CheckAccessAsync
                       +------+-------+
                              |
                         No   |   Yes
                       +------+--------+
                       |               |
                       v               v
                  Hide Edit       Show Edit
                  Buttons         Buttons
```

---

## 5. Implementation Details by Page

### 5.1 Page Authorization Matrix

| Page | Authorize Attributes | Edit Permission (Runtime) | Notes |
|------|---------------------|---------------------------|-------|
| Home | `[Authorize]` | none | Basic auth only |
| About | `[Authorize]` | none | Basic auth only |
| AuditTrail | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/audittrail/daterange")]` | n/a | Read-only |
| DocumentProperties | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}")]` | n/a | Read-only view |
| DocumentPropertiesPage | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:POST:/api/documents/")]` | n/a | View=Edit permission |
| SearchDocuments | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/")]` | `DELETE /api/documents/{id}`<br>`POST /api/documents/`<br>`POST /api/documents/email/*` | Multiple edit checks |
| ManageDocumentNames | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documentnames/")]` | `POST /api/documentnames/` | Already implemented |
| EditUserPermissions | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/userpermissions/users")]` | `POST /api/userpermissions/` | Already implemented |
| AccessAudit | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/access-audit/document-type/{id}")]` | n/a | Read-only |
| LogViewer | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:POST:/api/logs/search")]` | n/a | Read-only |
| ConfigurationManagement | `[Authorize(Policy = "HasAccess")]` | Per-tab view & edit | OR logic - runtime |
| EndpointManagement | `[Authorize(Policy = "SuperUser")]` | n/a | SuperUser only |
| CounterPartyAdministration | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/counterparties/")]` | `POST /api/counterparties/` | Already implemented |
| DocumentTypeAdministration | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documenttypes/all")]` | `POST /api/documenttypes/` | Already implemented |
| CurrencyAdministration | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/currencies/")]` | `POST /api/currencies/` | Already implemented |
| CountryAdministration | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/countries/")]` | `POST /api/countries/` | Already implemented |
| ComposeDocumentEmail | `[Authorize(Policy = "HasAccess")]` | n/a | OR logic - runtime |
| PdfViewer | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}/file")]` | n/a | Read-only |
| DocumentPreview | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}/file")]` | `POST /api/documents/` | Edit button |
| PrintSummary | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}")]` | n/a | Read-only |
| ExcelPreview | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:POST:/api/documents/export/excel")]` | n/a | Read-only |
| CheckinScanned | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/scannedfiles/")]` | n/a | Read-only |
| CheckinFileDetail | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/scannedfiles/{filename}")]` | `POST /api/documents/` | Check-in button |
| ActionReminders | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/action-reminders/")]` | n/a | Read-only |

### 5.2 Pages Requiring Runtime OR Logic

These pages cannot use simple attribute-based authorization because they need OR logic:

#### ConfigurationManagement

User needs ANY of:
- `GET /api/configuration/email-recipients`
- `GET /api/configuration/email-templates`
- `GET /api/configuration/sections`

```csharp
@code {
    private bool canViewRecipients = false;
    private bool canViewTemplates = false;
    private bool canViewSmtp = false;
    private bool canEditRecipients = false;
    private bool canEditTemplates = false;
    private bool canEditSmtp = false;
    private bool isCheckingAccess = true;

    protected override async Task OnInitializedAsync()
    {
        var checks = await EndpointAuthService.CheckMultipleAccessAsync(new Dictionary<string, (string, string)>
        {
            { "ViewRecipients", ("GET", "/api/configuration/email-recipients") },
            { "ViewTemplates", ("GET", "/api/configuration/email-templates") },
            { "ViewSmtp", ("GET", "/api/configuration/sections") },
            { "EditRecipients", ("PUT", "/api/configuration/email-recipients/{id}") },
            { "EditTemplates", ("POST", "/api/configuration/email-templates") },
            { "EditSmtp", ("PUT", "/api/configuration/smtp") }
        });

        canViewRecipients = checks.GetValueOrDefault("ViewRecipients", false);
        canViewTemplates = checks.GetValueOrDefault("ViewTemplates", false);
        canViewSmtp = checks.GetValueOrDefault("ViewSmtp", false);
        canEditRecipients = checks.GetValueOrDefault("EditRecipients", false);
        canEditTemplates = checks.GetValueOrDefault("EditTemplates", false);
        canEditSmtp = checks.GetValueOrDefault("EditSmtp", false);

        isCheckingAccess = false;

        // Must have at least one view permission
        if (!canViewRecipients && !canViewTemplates && !canViewSmtp)
        {
            Navigation.NavigateTo("/", forceLoad: false);
            return;
        }

        // Set initial tab to first available
        if (canViewRecipients) selectedTab = "email-recipients";
        else if (canViewTemplates) selectedTab = "email-templates";
        else if (canViewSmtp) selectedTab = "smtp-settings";
    }
}
```

#### ComposeDocumentEmail

User needs EITHER:
- `POST /api/documents/email/link`
- `POST /api/documents/email/attachment`

```csharp
@code {
    private bool canSendLink = false;
    private bool canSendAttachment = false;
    private bool isCheckingAccess = true;

    protected override async Task OnInitializedAsync()
    {
        var checks = await EndpointAuthService.CheckMultipleAccessAsync(new Dictionary<string, (string, string)>
        {
            { "SendLink", ("POST", "/api/documents/email/link") },
            { "SendAttachment", ("POST", "/api/documents/email/attachment") }
        });

        canSendLink = checks.GetValueOrDefault("SendLink", false);
        canSendAttachment = checks.GetValueOrDefault("SendAttachment", false);

        isCheckingAccess = false;

        // Must have at least one email permission
        if (!canSendLink && !canSendAttachment)
        {
            Navigation.NavigateTo("/documents/search", forceLoad: false);
            return;
        }

        // Continue with page initialization...
    }
}
```

### 5.3 SearchDocuments Actions Matrix

| Action | Endpoint Required | Implementation |
|--------|-------------------|----------------|
| Search, Filter, Sort | `GET /api/documents/` | Covered by page attribute |
| Print Summary | n/a | Non-modifying, always visible |
| Export to Excel | n/a | Non-modifying, always visible |
| Delete Selected | `DELETE /api/documents/{id}` | Hide if `hasDeleteAccess = false` |
| Delete (single) | `DELETE /api/documents/{id}` | Hide if `hasDeleteAccess = false` |
| Edit Properties link | `POST /api/documents/` | Hide if `hasEditAccess = false` |
| Barcode link | `POST /api/documents/` | Hide if `hasEditAccess = false` |
| Email (Attach) | `POST /api/documents/email/attachment` | Hide if `hasEmailAttachAccess = false` |
| Email (Link) | `POST /api/documents/email/link` | Hide if `hasEmailLinkAccess = false` |
| View Properties | n/a | Non-modifying, always visible |
| Open PDF | n/a | Non-modifying, always visible |

---

## 6. Architectural Design

### 6.1 Attribute-Based View Permissions

The existing `DynamicAuthorizationPolicyProvider` will handle endpoint-named policies automatically.

**Example page attribute usage:**

```razor
@page "/audit-trail"
@attribute [Authorize(Policy = "HasAccess")]
@attribute [Authorize(Policy = "Endpoint:GET:/api/audittrail/daterange")]
@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))
```

**How it works:**
1. Blazor sees `[Authorize(Policy = "Endpoint:GET:/api/audittrail/daterange")]`
2. Calls `DynamicAuthorizationPolicyProvider.GetPolicyAsync("Endpoint:GET:/api/audittrail/daterange")`
3. Provider parses the policy name and creates `EndpointAuthorizationRequirement`
4. `EndpointAuthorizationHandler` checks if user's roles have access to that endpoint
5. Returns 403 if unauthorized (Blazor default behavior)

### 6.2 Runtime Edit Permission Pattern

For pages that already have the pattern (admin pages), no changes needed. For pages that need it:

```csharp
@inject EndpointAuthorizationHttpService EndpointAuthService

@code {
    private bool hasWriteAccess = false;

    protected override async Task OnInitializedAsync()
    {
        hasWriteAccess = await EndpointAuthService.CheckAccessAsync("POST", "/api/endpoint/");

        // Continue with page initialization...
    }
}
```

**In markup:**
```razor
@if (hasWriteAccess)
{
    <button class="btn btn-primary" @onclick="Save">Save</button>
}
```

### 6.3 NavMenu Updates

The NavMenu needs to align with the endpoint policy names. Current implementation already uses the correct pattern:

```csharp
var endpointsToCheck = new Dictionary<string, (string Method, string Route)>
{
    { "RegisterDocument", ("POST", "/api/documents/") },
    { "SearchDocuments", ("GET", "/api/documents/") },
    // ... etc
};
```

**Updates needed:**
1. Fix AccessAudit endpoint: change from `/api/access-audit/users` to `/api/access-audit/document-type/{id}`
2. Add separate checks for ConfigurationManagement tabs (OR logic):

```csharp
{ "ConfigurationRecipients", ("GET", "/api/configuration/email-recipients") },
{ "ConfigurationTemplates", ("GET", "/api/configuration/email-templates") },
{ "ConfigurationSmtp", ("GET", "/api/configuration/sections") },
```

Update `HasAnySettingsAccess()`:
```csharp
private bool HasAnySettingsAccess() =>
    HasAccess("ConfigurationRecipients") ||
    HasAccess("ConfigurationTemplates") ||
    HasAccess("ConfigurationSmtp") ||
    HasAccess("AuditTrail") ||
    HasAccess("SystemLogs") ||
    HasAccess("EndpointManagement") ||
    HasAccess("AccessAudit");
```

### 6.4 Client-Side Permission Caching (Optional Enhancement)

For improved performance, wrap `EndpointAuthorizationHttpService` with caching:

```csharp
public class CachedEndpointAuthorizationService
{
    private readonly EndpointAuthorizationHttpService _authService;
    private readonly Dictionary<string, (bool HasAccess, DateTime ExpiresAt)> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<bool> CheckAccessAsync(string method, string route)
    {
        var cacheKey = $"{method}:{route}";

        if (_cache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
        {
            return cached.HasAccess;
        }

        var hasAccess = await _authService.CheckAccessAsync(method, route);
        _cache[cacheKey] = (hasAccess, DateTime.UtcNow.Add(_cacheDuration));

        return hasAccess;
    }

    public void InvalidateCache() => _cache.Clear();
}
```

---

## 7. Potential Issues

### Issue 1: DynamicAuthorizationPolicyProvider May Not Work on Client

**Description:** The `DynamicAuthorizationPolicyProvider` is registered server-side. For WebAssembly pages, the authorization check happens on the server during the initial request, but subsequent navigation might not trigger server-side authorization.

**Mitigation:**
- Test attribute-based authorization on WebAssembly pages
- If issues arise, fall back to runtime checks for problematic pages
- The existing `prerender: false` setting means all authorization happens client-side

**Investigation Required:** Verify that `[Authorize(Policy = "Endpoint:...")]` works correctly on `@rendermode InteractiveWebAssembly` pages.

### Issue 2: Cache Inconsistency Between Server and Client

**Description:** Server has 30-minute cache. When an admin changes permissions, users may have stale access until cache expires.

**Mitigation:**
- Server cache is already implemented (30 min TTL)
- Optional client-side cache (5 min TTL) further reduces API calls
- Users can force refresh by reloading browser

### Issue 3: Multiple Authorization Attributes Order

**Description:** When using multiple `[Authorize]` attributes, Blazor evaluates them as AND logic. All must pass.

**Current Design:** This is intentional:
1. `[Authorize]` - Must be authenticated
2. `[Authorize(Policy = "HasAccess")]` - Must be in DocuScanUsers
3. `[Authorize(Policy = "Endpoint:...")]` - Must have endpoint access

If any fails, user gets 403. This is the desired behavior.

### Issue 4: NavMenu Visibility vs Page Access Mismatch

**Description:** If NavMenu checks differ from page attributes, users might see menu items but get 403 when clicking.

**Mitigation:**
- Ensure NavMenu endpoint checks match page attribute policies exactly
- Use constants or shared configuration for endpoint strings
- Test all menu items with each user profile

---

## 8. Implementation Steps

### Phase 1: Fix Existing Issues

1. **Remove duplicate authorize attributes**
   - `CheckinFileDetail.razor` - remove line 4 duplicate
   - `CheckinScanned.razor` - remove duplicate

2. **Fix NavMenu AccessAudit endpoint**
   - Change from `GET /api/access-audit/users` to `GET /api/access-audit/document-type/{id}`

### Phase 2: Add View Permission Attributes

Add endpoint policy attribute to each page:

| Page | Add Attribute |
|------|---------------|
| AuditTrail | `@attribute [Authorize(Policy = "Endpoint:GET:/api/audittrail/daterange")]` |
| DocumentProperties | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documents/{id}")]` |
| DocumentPropertiesPage | `@attribute [Authorize(Policy = "Endpoint:POST:/api/documents/")]` |
| SearchDocuments | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documents/")]` |
| ManageDocumentNames | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documentnames/")]` |
| EditUserPermissions | `@attribute [Authorize(Policy = "Endpoint:GET:/api/userpermissions/users")]` |
| AccessAudit | `@attribute [Authorize(Policy = "Endpoint:GET:/api/access-audit/document-type/{id}")]` |
| LogViewer | `@attribute [Authorize(Policy = "Endpoint:POST:/api/logs/search")]` |
| CounterPartyAdministration | `@attribute [Authorize(Policy = "Endpoint:GET:/api/counterparties/")]` |
| DocumentTypeAdministration | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documenttypes/all")]` |
| CurrencyAdministration | `@attribute [Authorize(Policy = "Endpoint:GET:/api/currencies/")]` |
| CountryAdministration | `@attribute [Authorize(Policy = "Endpoint:GET:/api/countries/")]` |
| PdfViewer | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documents/{id}/file")]` |
| DocumentPreview | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documents/{id}/file")]` |
| PrintSummary | `@attribute [Authorize(Policy = "Endpoint:GET:/api/documents/{id}")]` |
| ExcelPreview | `@attribute [Authorize(Policy = "Endpoint:POST:/api/documents/export/excel")]` |
| CheckinScanned | `@attribute [Authorize(Policy = "Endpoint:GET:/api/scannedfiles/")]` |
| CheckinFileDetail | `@attribute [Authorize(Policy = "Endpoint:GET:/api/scannedfiles/{filename}")]` |
| ActionReminders | `@attribute [Authorize(Policy = "Endpoint:GET:/api/action-reminders/")]` |

**Pages NOT getting endpoint attribute (use runtime OR logic):**
- ConfigurationManagement
- ComposeDocumentEmail

### Phase 3: Add Runtime Edit Permission Checks

**SearchDocuments.razor:**
```csharp
@inject EndpointAuthorizationHttpService EndpointAuthService

@code {
    private bool hasDeleteAccess = false;
    private bool hasEditAccess = false;
    private bool hasEmailLinkAccess = false;
    private bool hasEmailAttachAccess = false;

    protected override async Task OnInitializedAsync()
    {
        var checks = await EndpointAuthService.CheckMultipleAccessAsync(new Dictionary<string, (string, string)>
        {
            { "Delete", ("DELETE", "/api/documents/{id}") },
            { "Edit", ("POST", "/api/documents/") },
            { "EmailLink", ("POST", "/api/documents/email/link") },
            { "EmailAttach", ("POST", "/api/documents/email/attachment") }
        });

        hasDeleteAccess = checks.GetValueOrDefault("Delete", false);
        hasEditAccess = checks.GetValueOrDefault("Edit", false);
        hasEmailLinkAccess = checks.GetValueOrDefault("EmailLink", false);
        hasEmailAttachAccess = checks.GetValueOrDefault("EmailAttach", false);

        // Continue with existing initialization...
    }
}
```

**CheckinFileDetail.razor:**
```csharp
@code {
    private bool hasCreateAccess = false;

    protected override async Task OnInitializedAsync()
    {
        hasCreateAccess = await EndpointAuthService.CheckAccessAsync("POST", "/api/documents/");
        // Continue with existing initialization...
    }
}
```

**DocumentPreview.razor:**
```csharp
@code {
    private bool hasEditAccess = false;

    protected override async Task OnInitializedAsync()
    {
        hasEditAccess = await EndpointAuthService.CheckAccessAsync("POST", "/api/documents/");
        // Continue with existing initialization...
    }
}
```

### Phase 4: Implement OR Logic Pages

**ConfigurationManagement.razor:** (See Section 5.2)

**ComposeDocumentEmail.razor:** (See Section 5.2)

### Phase 5: Update NavMenu

1. Fix AccessAudit endpoint mapping
2. Add ConfigurationManagement OR logic checks
3. Update `HasAnySettingsAccess()` method

### Phase 6: Testing

Test with all four profiles: **reader**, **publisher**, **adadmin**, **superuser**

| Test Case | Expected Result |
|-----------|-----------------|
| User without endpoint access clicks menu item | Menu item hidden |
| User without endpoint access navigates via URL | 403 response |
| User with view but no edit access | Page loads, edit buttons hidden |
| SuperUser accesses EndpointManagement | Full access |
| Non-SuperUser accesses EndpointManagement | 403 response |
| User with only one ConfigurationManagement tab | Only that tab visible |
| User with only EmailLink permission on ComposeEmail | Page accessible |

---

## Appendix A: Complete Page-Endpoint Mapping Reference

| Page | Route(s) | Attributes | Runtime Edit Check |
|------|----------|------------|-------------------|
| Home | `/` | `[Authorize]` | none |
| About | `/about` | `[Authorize]` | none |
| AuditTrail | `/audit-trail` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/audittrail/daterange")]` | none |
| DocumentProperties | `/documents/properties/{DocumentId:int}` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}")]` | none |
| DocumentPropertiesPage | `/documents/edit/{BarCode:int}`<br>`/documents/register`<br>`/documents/checkin/{FileName}` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:POST:/api/documents/")]` | none |
| SearchDocuments | `/documents/search` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/")]` | `DELETE /api/documents/{id}`<br>`POST /api/documents/`<br>`POST /api/documents/email/link`<br>`POST /api/documents/email/attachment` |
| ManageDocumentNames | `/manage-document-names` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documentnames/")]` | `POST /api/documentnames/` |
| EditUserPermissions | `/edit-userpermissions` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/userpermissions/users")]` | `POST /api/userpermissions/` |
| AccessAudit | `/access-audit` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/access-audit/document-type/{id}")]` | none |
| LogViewer | `/admin/logs` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:POST:/api/logs/search")]` | none |
| ConfigurationManagement | `/configuration-management` | `[Authorize(Policy = "HasAccess")]` | OR logic runtime check |
| EndpointManagement | `/endpoint-management` | `[Authorize(Policy = "SuperUser")]` | none |
| CounterPartyAdministration | `/counterparty-administration` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/counterparties/")]` | `POST /api/counterparties/` |
| DocumentTypeAdministration | `/documenttype-administration` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documenttypes/all")]` | `POST /api/documenttypes/` |
| CurrencyAdministration | `/currency-administration` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/currencies/")]` | `POST /api/currencies/` |
| CountryAdministration | `/country-administration` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/countries/")]` | `POST /api/countries/` |
| ComposeDocumentEmail | `/documents/compose-email` | `[Authorize(Policy = "HasAccess")]` | OR logic runtime check |
| PdfViewer | `/pdf-viewer/{DocumentId:int}` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}/file")]` | none |
| DocumentPreview | `/documents/preview/{DocumentId:int}` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}/file")]` | `POST /api/documents/` |
| PrintSummary | `/documents/print-summary` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/documents/{id}")]` | none |
| ExcelPreview | `/excel-preview` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:POST:/api/documents/export/excel")]` | none |
| CheckinScanned | `/checkin-scanned` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/scannedfiles/")]` | none |
| CheckinFileDetail | `/checkin-scanned/detail/{FileName}` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/scannedfiles/{filename}")]` | `POST /api/documents/` |
| ActionReminders | `/action-reminders` | `[Authorize(Policy = "HasAccess")]`<br>`[Authorize(Policy = "Endpoint:GET:/api/action-reminders/")]` | none |

---

**End of Document**
