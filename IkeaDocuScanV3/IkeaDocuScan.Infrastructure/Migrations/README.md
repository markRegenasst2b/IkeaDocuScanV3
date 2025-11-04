# Database Migrations

This folder contains Entity Framework Core database migrations for the IkeaDocuScan application.

## Running Migrations

To apply migrations to your database, run the following command from the solution root:

```bash
cd IkeaDocuScan.Infrastructure
dotnet ef database update --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

Or from the Web project:

```bash
cd IkeaDocuScan-Web/IkeaDocuScan-Web
dotnet ef database update --project ../../IkeaDocuScan.Infrastructure
```

## Migration: RemoveCounterPartyRelationTables (20251104000000)

**Created:** November 4, 2025

**Purpose:** Removes the `CounterPartyRelation` and `CounterPartyRelationType` tables from the database as they are no longer used in the application.

### What This Migration Does

**Up Migration:**
- Drops the `CounterPartyRelation` table if it exists
- Drops the `CounterPartyRelationType` table if it exists
- Uses `IF EXISTS` checks to safely drop tables without errors if they don't exist

**Down Migration:**
- Recreates both tables with their original schema
- Restores all foreign key relationships
- Restores all indexes

### Why These Tables Were Removed

The `CounterPartyRelation` and `CounterPartyRelationType` tables were part of an older design that tracked hierarchical relationships between counter parties. This functionality is no longer used in the application.

**Removed Components:**
- `CounterPartyRelation` entity class
- `CounterPartyRelationType` entity class
- DbSet definitions in `AppDbContext`
- Navigation properties in `CounterParty` entity
- EF Core configuration in `OnModelCreating`

### Safety

The migration uses `IF EXISTS` checks to ensure it runs safely even if the tables have already been removed manually or don't exist in your database.

### Rollback

If you need to rollback this migration:

```bash
dotnet ef database update 0 --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

This will execute the `Down` migration and recreate the tables. However, note that:
- The entity classes have been deleted from the codebase
- The application code no longer references these tables
- You would need to restore the entity classes from source control to use them again
