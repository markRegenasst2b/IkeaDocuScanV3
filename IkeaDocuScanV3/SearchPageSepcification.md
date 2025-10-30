
---

# 🧠 **Claude System Instruction**

You are **Claude**, an expert **UX and functional specification assistant**.
Your role is to **interpret, refine, and expand structured specifications** for web applications.
Follow these rules carefully:

* Focus on **clarity, completeness, and logical structure** — not on writing code.
* When uncertain, **ask clarifying questions** before making assumptions.
* Use clear headings, bullet points, and tables to express structure.
* Ensure consistency in terminology and relationships between filters, lists, and actions.
* Your outputs must be **implementation-ready specifications** for incremental UI building.
* Do **not** produce code unless explicitly instructed later.

---

# 🧭 **Project Specification: SearchDocument Page**

## 🎯 Task Intent

Claude, your task is to **create and refine the specification** for a web page called **SearchDocument Page**.
This page enables users to search, filter, and manage documents efficiently.
You will **not write code** at this stage — only clarify and expand the functional and UI specification.
If any detail is ambiguous, **ask for clarification first**.

---

## 🧩 Page Overview

The **SearchDocument Page** contains two primary sections:

1. **Filter Criteria Panel (Upper Section)** — users define filters for document search.
2. **Document Results List (Lower Section)** — displays retrieved documents and supports row and bulk actions.

---

## 1. 🔍 Filter Criteria Panel

### General Filters

| Filter                             | Type                  | Behavior                                                   |
| ---------------------------------- | --------------------- | ---------------------------------------------------------- |
| **Search String**                  | String                | Free-text search in `documentfile.bytes`.                  |
| **Barcode**                        | String                | Accepts a comma-separated list of integers.                |
| **Document Types**                 | Multi-select list box | Lists all document types.                                  |
| **Document Name**                  | Dropdown              | Lists document names, filtered by selected document types. |
| **Document Number**                | String                | “Contains” search.                                         |
| **Version No**                     | String                | “Contains” search.                                         |
| **Associated to PUA/Agreement No** | String                | “Contains” search.                                         |
| **Associated to Appendix No**      | String                | “Contains” search.                                         |

---

### Counterparty Filters

| Filter                   | Type     | Behavior                                                |
| ------------------------ | -------- | ------------------------------------------------------- |
| **Counterparty Name**    | String   | Free-text search in counterparty and third-party names. |
| **Counterparty No**      | String   | Exact search on `CounterParty.CounterPartyNoAlpha`.     |
| **Counterparty Country** | Dropdown | Exact match on `CounterParty.Country`.                  |
| **Counterparty City**    | String   | Free-text search on `CounterParty.City`.                |

---

### Document Attributes

| Filter                | Type     | Options / Behavior |
| --------------------- | -------- | ------------------ |
| **Fax**               | Dropdown | Yes / No           |
| **Original Received** | Dropdown | Yes / No           |
| **Confidential**      | Dropdown | Yes / No           |
| **Bank Confirmation** | Dropdown | Yes / No           |
| **Authorisation To**  | String   | “Contains” search. |

---

### Financial Filters

| Filter       | Type        | Behavior                                 |
| ------------ | ----------- | ---------------------------------------- |
| **Amount**   | Float range | Open-ended on either side.               |
| **Currency** | Dropdown    | Exact match from list of all currencies. |

---

### Date Filters

All date filters accept open-ended ranges (start and/or end optional).

* **Date of Contract**
* **Receiving Date**
* **Sending Out Date**
* **Forwarded to Signatories Date**
* **Dispatch Date**
* **Action Date**

---

## 2. 📄 Document Results List

### Initial State

When the result set is empty, show:

> “Start your search by entering the criteria above.”

### Display Columns

| Column                            | Description                 |
| --------------------------------- | --------------------------- |
| Select                            | Checkbox to select document |
| Bar Code                          | Unique barcode              |
| Document Type                     | Document type               |
| Document Name                     | Document name               |
| Counterparty                      | Counterparty name           |
| Counterparty No.                  | Counterparty number         |
| Country                           | Counterparty country        |
| Third Party                       | Related third party         |
| Date of Contract                  | Contract date               |
| Comment                           | Notes field                 |
| Fax                               | Yes/No                      |
| Original Received                 | Yes/No                      |
| Document No.                      | Internal identifier         |
| Associated to PUA / Agreement No. | Reference number            |
| Version No.                       | Version identifier          |
| Currency                          | Currency code               |
| Amount                            | Numeric amount              |

---

### Pagination

* Default page size: **25**
* Options: **10, 25, 100**
* Includes pager controls: *Previous / Next / Page numbers*

---

## 3. ⚙️ Row Actions (Per Document)

Each row has an **Action Menu** with these options:

| Action                     | Behavior                                                                      |
| -------------------------- | ----------------------------------------------------------------------------- |
| **Open**                   | Downloads and displays the PDF from `documentfile` in a new tab.              |
| **View Properties**        | Shows detailed metadata in tabular form.                                      |
| **Edit Properties**        | Navigates to `/document/edit/{barcode}`.                                      |
| **Send as Email (Attach)** | Opens email client, prefilled text/subject/recipient, attaches PDF.           |
| **Send as Email (Link)**   | Opens email client, prefilled text/subject/recipient, includes download link. |
| **Delete**                 | Confirms and deletes document and its `documentfile`.                         |

The pages for displaying the pdf and the propertis are out of scope for this task.
---

## 4. 🧰 Bulk Actions (Selected Rows)

Each result includes a **Select** checkbox.
The list provides **Selection Controls** and **Bulk Actions**.

### Selection Controls

* **Select All** – Selects all rows on current page.
* **Deselect All** – Deselects all rows on current page.
* **Invert Selection** – Toggles selection state of all rows.

### Bulk Actions (enabled when ≥1 document selected)

| Action                     | Description                                        |
| -------------------------- | -------------------------------------------------- |
| **Delete Selected**        | Confirms and deletes selected documents and files. |
| **Print Summary**          | Generates summary report for selected documents.   |
| **Print Detailed**         | Generates detailed report for selected documents.  |
| **Send as Email (Attach)** | Prefilled email, attaches selected PDFs.           |
| **Send as Email (Link)**   | Prefilled email, includes download links.          |

The pages for printing summaries and details are out of scope for this task.
---

## 5. 💡 Behavioral Rules

* Results list remains empty until search is executed.
* Maximal number of retrieved documents is configurable (default: 1000).
* Starting search without any filter criteria returns all documents up to the maximum.
* Clearing filters resets list to initial state.
* Dropdown dependencies (e.g., *Document Name* → *Document Type*) must update dynamically.
* Disabled actions indicate why they’re unavailable (e.g., “No document selected”).
* Confirmation dialogs show number of affected items.
* Column headers are sortable (ascending/descending). When used with retrieved data, the data set must be sorted accordingly. When a query is issued, the sorting must be applied to the query results before omitting the results exceeding the maximum number of retrieved documents.

---

## 6. 🧱 Development Guidelines

* **Do not write code yet.**
* **Ask clarifying questions** before implementing unclear sections.
* Use this as the foundation for **incremental page construction** (UI layout, data behavior, logic).
* Maintain accessible, consistent UI and naming conventions.

---

## ✅ Next Step for Claude

1. Confirm your understanding of this full specification.
2. Identify any missing information or ambiguities.
3. Once clarified, proceed to define structure and behavior incrementally — one part of the page at a time.

---


