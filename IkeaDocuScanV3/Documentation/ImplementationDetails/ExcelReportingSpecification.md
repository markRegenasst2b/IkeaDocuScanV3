You are Claude 3.5 Sonnet, a senior .NET architect and Blazor specialist.

## Context
You are working within a Visual Studio environment targeting **.NET 9 or .NET 10**, using **Blazor** for the web front-end.  
This project is part of the **IkeaDocuScan** solution.

You have access to and should reference the following documents for context and architectural alignment:
- `IkeaDocuScanV3/Claude.md`
- `ARCHITECTURE_IMPLEMENTATION_PLAN.md`

Please read and interpret these two documents to understand:
- Overall architecture and layering of the system.
- Naming conventions, dependency injection patterns, and project layout.
- Any existing conventions for DTOs, attributes, and component design.

---

## Task
Create a **comprehensive implementation plan** for the following new functionality, without writing any code.

### Excel Reporting Functionality

#### Overview
Develop a Blazor page that previews a collection of DTO objects in a table and allows users to export that data as an Excel file.

#### Requirements Summary

1. **DTO Objects**
   - All DTOs inherit from a single abstract base class.
   - Each property is decorated with a custom attribute:
     ```csharp
     [ExcelExport(DisplayName, DataType, Format)]
     ```
   - The attribute defines how each property appears in both the grid and Excel export.

2. **Blazor Page (ExcelPreview.razor)**
   - Location: `IkeaDocuScan-Web/Pages/ExcelPreview.razor`
   - Accepts an `IEnumerable<T>` of DTO objects as a parameter.
   - Dynamically renders a data grid using reflection on the attribute metadata.
   - Includes paging controls (10, 25, 100 rows per page; default 25).
   - Provides a “Download as Excel” button that triggers export of the current data view.

3. **Excel Reporting Library**
   - Create a separate project named **ExcelReporting** (class library / DLL).
   - The library:
     - Uses **Syncfusion XlsIO** (NuGet package) for Excel generation.
     - Provides a service or utility class to generate Excel files based on the DTOs and their attributes.
     - Applies formatting rules from the attribute parameters.
     - Returns an in-memory stream or byte array for Blazor download.

4. **Integration**
   - The Blazor component calls into the `ExcelReporting` library for Excel generation.
   - Follow the dependency injection and service registration patterns defined in `ARCHITECTURE_IMPLEMENTATION_PLAN.md`.
   - Keep UI logic minimal; delegate reflection and transformation logic to the ExcelReporting project.

---

## Deliverable
Produce a **structured, detailed implementation plan** only — not code.  
The plan should include:

1. **Project structure and namespace layout**
2. **Key classes, interfaces, and their responsibilities**
3. **Step-by-step implementation sequence**
4. **Setup details** (NuGet packages, DI configuration, etc.)
5. **Testing and validation strategy**
6. **Blazor UI/UX considerations** (grid rendering, paging, and download UX)
7. **Open questions or assumptions** (list anything that may need clarification)

Follow the same formal structure, tone, and section naming conventions used in `ARCHITECTURE_IMPLEMENTATION_PLAN.md`.

---

### Important Constraints
- Do **not** generate source code; produce only the plan.
- Assume the user will later request code generation based on your plan.
- Be explicit about responsibilities between the web layer and ExcelReporting library.
- Highlight extensibility considerations (e.g., supporting new DTO types in the future).

---

**End Goal:**  
Deliver a clear, implementation-ready plan aligned with the IkeaDocuScan architectural guidelines that defines exactly *how* to build and integrate the Excel preview and export functionality.

