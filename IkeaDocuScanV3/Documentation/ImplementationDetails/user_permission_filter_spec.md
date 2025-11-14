
## 🧠 Claude Prompt: Add Document Access Control via UserPermissions (EF Core + SQL Server)

### 🧩 Goal

Implement fine-grained document access control for `DocuscanUser` based on `UserPermission` entries.

A `UserPermission` defines which documents a user can access. Each permission may have optional filters for:

* `DocumentTypeId`
* `CounterPartyId`
* `CountryCode`

Access is granted **if any** of the user’s `UserPermissions` satisfies:

```sql
(Document.CounterPartyId IS NULL OR UserPermission.CounterPartyId IS NULL OR Document.CounterPartyId = UserPermission.CounterPartyId)
AND (Document.CountryCode    IS NULL OR UserPermission.CountryCode    IS NULL OR Document.CountryCode    = UserPermission.CountryCode)
AND (Document.DocumentTypeId IS NULL OR UserPermission.DocumentTypeId IS NULL OR Document.DocumentTypeId = UserPermission.DocumentTypeId)
```

If no matching `UserPermission` exists, the document must be **hidden** from search results and inaccessible in detail views.
All endpoints returning documents must enforce this rule.

---

### 🎯 Task for Claude

Propose **only the required changes** to introduce this feature in the app.

Structure your proposal as follows:

#### 1. **Data Model Changes**

* Specify if any adjustments are needed in EF Core entity definitions:

  * `UserPermission` (navigation properties, indexes, optional fields)
  * `Document`
* Recommend any helpful EF Core annotations or SQL indexes to optimize lookups.

#### 2. **Repository / EF Core Query Layer**

* Propose efficient query modifications (LINQ or raw SQL) to enforce the access control logic.
* Offer **two or more** implementation strategies, such as:

  * Filtering with `.Any()` using subqueries
  * Using `JOIN` or `EXISTS` clauses
  * Precomputing accessible `DocumentIds` for a user and caching them

For each approach, discuss:

* Query readability and maintainability
* Expected SQL generation and index usage
* Scalability for large datasets

#### 3. **Service / Business Layer**

* Describe how to integrate permission filtering into the document search and retrieval services.
* Specify whether to:

  * Filter documents at the repository level (preferred for performance)
  * Or enforce checks in the business layer before returning results
* Recommend where and how to reuse the same filtering logic (e.g., via an `IQueryable` extension).

#### 4. **Controller / API Layer**

* Show how endpoints (e.g., `/api/documents/search`, `/api/documents/{id}`) should apply the permission filter.
* Ensure consistent enforcement for all document-related endpoints.

#### 5. **Performance Considerations**

* Evaluate:

  * SQL Server execution plan implications
  * Potential index improvements (`CounterPartyId`, `CountryCode`, `DocumentTypeId`)
  * Caching options (per-user permission sets)
  * Trade-offs between EF Core LINQ queries vs. stored procedures for filtering
* Recommend the **most performant and maintainable** approach.

#### 6. **Testing and Validation**

* Specify unit and integration tests needed to:

  * Verify access control correctness
  * Confirm that users without matching `UserPermissions` see zero documents
  * Ensure no regressions in performance

---

### 🧭 Guidelines

* **Be concise:** Propose only the minimal necessary code and architectural changes.
* **Be explicit:** Include pseudocode, EF Core LINQ examples, or repository query snippets.
* **Think in options:** Compare implementation strategies and justify the most efficient one.
* **Performance is key:** Prioritize SQL and EF Core–level filtering to minimize memory usage and round-trips.

