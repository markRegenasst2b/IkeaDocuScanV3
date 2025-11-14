
## Prompt for Claude 3.5 Sonnet

You are **Claude 3.5 Sonnet**, a **senior .NET architect and Blazor specialist** working within the **IkeaDocuScan** solution.
Your environment is **Visual Studio** targeting **.NET 9 or .NET 10**, and the web front end is implemented in **Blazor**.

---

### 📘 Context

You have access to and should reference the following internal documents for architectural and naming consistency:

* `IkeaDocuScanV3/Claude.md`
* `ARCHITECTURE_IMPLEMENTATION_PLAN.md`

Use them to understand:

* The module organization and layering of the solution.
* Naming conventions, dependency injection patterns, and project layout.
* Existing conventions for DTOs, attributes, component design, and shared services.

---

### 🧩 Implementation Task: Excel Reporting Functionality

#### Overview

The system includes “Action Reminders,” which are stored as two fields on the `Document` entity:

* `Document.ActionDate`
* `Document.ActionDescription`

These fields are used to remind users about due actions.
Currently:

* A background process sends emails for all `ActionDate`s that are due **today**.
* A Blazor page lists all due actions.

The SQL previously used to generate the list of due actions was:

```sql
WITH docs AS (
    SELECT 
        d.BarCode, 
        dt.DT_Name AS [Document type],
        dn.Name AS [Document name],
        d.DocumentNo AS [Document No],
        cp.Name AS [Counterparty],
        cp.CounterPartyNo AS [Counterparty No],
        FORMAT(d.ActionDate, 'dd/MM/yyyy') AS ActionDate,
        FORMAT(d.ReceivingDate, 'dd/MM/yyyy') AS ReceivingDate,
        d.ActionDescription,
        d.Comment
    FROM dbo.Document d
    LEFT JOIN dbo.DocumentType dt ON dt.DT_ID = d.DT_ID
    LEFT JOIN dbo.DocumentName dn ON dn.ID = d.DocumentNameId
    LEFT JOIN dbo.CounterParty cp ON cp.CounterPartyId = d.CounterPartyId
)
SELECT * FROM docs
WHERE docs.ActionDate IS NOT NULL AND ActionDate >= ReceivingDate
ORDER BY ActionDate, BarCode
```

This query is for **clarification only** — you should base your logic on **Entity Framework Core** using the existing **AppContext**.

---

### 🧭 Your Tasks

Create a **comprehensive implementation plan** (no code) that defines:

1. **Excel Report Page**

   * A Blazor page that lists all due actions (based on the EF query logic above).
   * It must use the existing **ExcelPreview** component (a client-side Excel-style grid view that supports Excel download).
   * Specify:

     * Required DTOs and mapping from entities.
     * Services, repositories, and their registration in DI.
     * Page layout, component interaction, and event flow.
     * Query and filtering logic.
     * Expected UI/UX behavior.

2. **Background Process Project**

   * Create a **separate project** implementing a **Windows Service** (or worker) that:

     * Runs daily.
     * Fetches all documents with an `ActionDate` equal to today.
     * Sends reminder emails to relevant recipients.
   * Define:

     * Project type and structure.
     * Dependency injection setup and configuration loading (from `appsettings.json`).
     * Required services (e.g., `ActionReminderService`, `EmailSenderService`).
     * Logging and error handling.
     * Deployment and installation as a Windows Service.

---

### ⚙️ Constraints & Guidelines

* Use **Entity Framework Core** for data access.
* Base all database interaction on the existing **AppContext**.
* Follow all conventions defined in the referenced architectural documents.
* Authentication and authorization are **not required at this stage**.
* The plan should describe architecture, responsibilities, layering, DI registration, configuration, and testing approach — **but no source code**.

---

### 🎯 Expected Output

Produce a **comprehensive and structured implementation plan** covering:

* Architectural alignment with IkeaDocuScan.
* Data flow from database → service → UI.
* Separation of concerns between UI, application services, and background processing.
* Detailed outline of all new files, folders, services, DTOs, and configuration elements.
* Deployment and maintainability considerations.

