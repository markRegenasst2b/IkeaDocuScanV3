# Database Access Verification Guide
**Purpose:** Verify that EF Core can access the new authorization tables

---

## Prerequisites

1. ✅ SQL scripts executed:
   - `01_Create_EndpointRegistry_Table.sql`
   - `02_Create_EndpointRolePermission_Table.sql`
   - `03_Create_PermissionChangeAuditLog_Table.sql`
   - `04_Seed_EndpointRegistry_And_Permissions.sql`

2. ✅ Application running in DEBUG mode
3. ✅ Database connection string configured in `appsettings.json` or `appsettings.Local.json`

---

## Method 1: Diagnostic API Endpoints (Recommended) 🚀

The application includes diagnostic endpoints (DEBUG mode only) to verify database access.

### Step 1: Start the Application
```bash
cd IkeaDocuScan-Web/IkeaDocuScan-Web
dotnet run
```

### Step 2: Test Database Connection
```bash
curl http://localhost:44100/api/diagnostic/db-connection
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Database connection successful",
  "connectionString": "Server=localhost;Database=IkeaDocuScan;Integrated Security=True;TrustServerCertificate=True;Password=***",
  "databaseName": "IkeaDocuScan",
  "providerName": "Microsoft.EntityFrameworkCore.SqlServer"
}
```

### Step 3: Test EndpointRegistry Table
```bash
curl http://localhost:44100/api/diagnostic/endpoint-registry
```

**Expected Response:**
```json
{
  "success": true,
  "tableName": "EndpointRegistry",
  "totalCount": 126,
  "sampleRecords": [
    {
      "endpointId": 1,
      "httpMethod": "GET",
      "route": "/api/documents/",
      "endpointName": "GetDocuments",
      "category": "Documents",
      "isActive": true
    },
    // ... 4 more records
  ],
  "message": "Successfully accessed EndpointRegistry table with 126 records"
}
```

### Step 4: Test EndpointRolePermission Table
```bash
curl http://localhost:44100/api/diagnostic/endpoint-role-permission
```

**Expected Response:**
```json
{
  "success": true,
  "tableName": "EndpointRolePermission",
  "totalCount": 500,
  "roleDistribution": [
    { "roleName": "SuperUser", "count": 126 },
    { "roleName": "Publisher", "count": 200 },
    { "roleName": "ADAdmin", "count": 100 },
    { "roleName": "Reader", "count": 74 }
  ],
  "sampleRecords": [
    {
      "permissionId": 1,
      "endpointId": 1,
      "roleName": "Reader",
      "endpointRoute": "/api/documents/",
      "endpointMethod": "GET"
    },
    // ... 9 more records
  ],
  "message": "Successfully accessed EndpointRolePermission table with 500 records"
}
```

### Step 5: Test All Tables at Once
```bash
curl http://localhost:44100/api/diagnostic/all-tables
```

**Expected Response:**
```json
{
  "success": true,
  "message": "All authorization tables are accessible via EF Core",
  "tables": {
    "EndpointRegistry": {
      "success": true,
      "totalCount": 126,
      "categories": [
        { "category": "ActionReminders", "count": 3 },
        { "category": "AuditTrail", "count": 7 },
        { "category": "Configuration", "count": 19 },
        // ... more categories
      ]
    },
    "EndpointRolePermission": {
      "success": true,
      "totalCount": 500,
      "roles": [
        { "roleName": "ADAdmin", "count": 100 },
        { "roleName": "Publisher", "count": 200 },
        { "roleName": "Reader", "count": 74 },
        { "roleName": "SuperUser", "count": 126 }
      ]
    },
    "PermissionChangeAuditLog": {
      "success": true,
      "totalCount": 0
    }
  }
}
```

### Step 6: Test EndpointAuthorizationService
```bash
curl http://localhost:44100/api/diagnostic/test-authorization-service
```

**Expected Response:**
```json
{
  "success": true,
  "message": "EndpointAuthorizationService is working correctly",
  "tests": {
    "getAllowedRoles": {
      "endpoint": "GET /api/documents/",
      "roles": ["Reader", "Publisher", "ADAdmin", "SuperUser"]
    },
    "checkAccess": {
      "readerCanGetDocuments": true,
      "publisherCanPostDocuments": true,
      "superUserCanDeleteDocuments": true
    },
    "allEndpoints": {
      "totalCount": 126,
      "categories": [
        { "category": "Documents", "count": 10 },
        { "category": "CounterParties", "count": 7 },
        // ... more categories
      ]
    }
  }
}
```

---

## Method 2: SQL Server Management Studio (SSMS)

### Verify Tables Exist
```sql
USE IkeaDocuScan;
GO

-- Check if tables exist
SELECT TABLE_NAME, TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('EndpointRegistry', 'EndpointRolePermission', 'PermissionChangeAuditLog')
ORDER BY TABLE_NAME;
```

**Expected Output:**
```
TABLE_NAME                    TABLE_TYPE
--------------------------    ----------
EndpointRegistry              BASE TABLE
EndpointRolePermission        BASE TABLE
PermissionChangeAuditLog      BASE TABLE
```

### Verify Data Seeded
```sql
-- Count records
SELECT 'EndpointRegistry' AS TableName, COUNT(*) AS RecordCount
FROM EndpointRegistry
UNION ALL
SELECT 'EndpointRolePermission', COUNT(*)
FROM EndpointRolePermission
UNION ALL
SELECT 'PermissionChangeAuditLog', COUNT(*)
FROM PermissionChangeAuditLog;
```

**Expected Output:**
```
TableName                     RecordCount
--------------------------    -----------
EndpointRegistry              126
EndpointRolePermission        500+
PermissionChangeAuditLog      0
```

### Verify Categories and Roles
```sql
-- Endpoints by category
SELECT Category, COUNT(*) AS EndpointCount
FROM EndpointRegistry
GROUP BY Category
ORDER BY Category;

-- Permissions by role
SELECT RoleName, COUNT(*) AS PermissionCount
FROM EndpointRolePermission
GROUP BY RoleName
ORDER BY RoleName;
```

---

## Method 3: Application Logs

### Check Serilog Logs
When the application starts, EF Core logs database operations:

**Location:** `C:\Logs\IkeaDocuScan\log-YYYYMMDD.json`

**Look for:**
```json
{
  "@t": "2025-11-17T...",
  "@mt": "Entity Framework Core ... initialized '...' using provider 'Microsoft.EntityFrameworkCore.SqlServer'",
  "SourceContext": "Microsoft.EntityFrameworkCore.Infrastructure"
}
```

### Enable Verbose EF Core Logging (Temporary)

Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

Restart application and watch console for:
```
[12:34:56 DBG] Entity Framework Core 9.0.0 initialized 'AppDbContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer'
[12:34:56 DBG] Executed DbCommand (15ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT COUNT(*) FROM [EndpointRegistry]
```

---

## Method 4: dotnet ef Command-Line

### Verify DbContext Can See Tables
```bash
cd IkeaDocuScan.Infrastructure
dotnet ef dbcontext info --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

**Expected Output:**
```
Provider name: Microsoft.EntityFrameworkCore.SqlServer
Database name: IkeaDocuScan
Data source: localhost
Options: None
```

### List All Registered Entity Types
```bash
dotnet ef dbcontext list --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

**Expected Output (should include):**
```
IkeaDocuScan.Infrastructure.Data.AppDbContext
  - EndpointRegistry
  - EndpointRolePermission
  - PermissionChangeAuditLog
  - (other entities...)
```

---

## Method 5: Browser-Based Testing

### Option A: Swagger/OpenAPI (if enabled)
1. Navigate to `https://localhost:44101/swagger`
2. Find "Diagnostic" section
3. Expand `GET /api/diagnostic/all-tables`
4. Click "Try it out"
5. Click "Execute"
6. Review response

### Option B: Browser Developer Tools
1. Open `https://localhost:44101`
2. Open browser DevTools (F12)
3. Go to Console tab
4. Execute:
```javascript
fetch('/api/diagnostic/all-tables')
  .then(r => r.json())
  .then(data => console.table(data.tables));
```

---

## Troubleshooting

### Error: "Cannot connect to database"
**Cause:** Connection string incorrect or SQL Server not running

**Fix:**
1. Verify SQL Server is running: `sqlcmd -S localhost -Q "SELECT @@VERSION"`
2. Check connection string in `appsettings.Local.json`
3. Test connection: `sqlcmd -S localhost -d IkeaDocuScan -Q "SELECT 1"`

### Error: "Invalid object name 'EndpointRegistry'"
**Cause:** SQL scripts not executed

**Fix:**
```bash
sqlcmd -S localhost -d IkeaDocuScan -i "01_Create_EndpointRegistry_Table.sql"
sqlcmd -S localhost -d IkeaDocuScan -i "02_Create_EndpointRolePermission_Table.sql"
sqlcmd -S localhost -d IkeaDocuScan -i "03_Create_PermissionChangeAuditLog_Table.sql"
```

### Error: "No data in tables" (totalCount = 0)
**Cause:** Seed script not executed

**Fix:**
```bash
sqlcmd -S localhost -d IkeaDocuScan -i "04_Seed_EndpointRegistry_And_Permissions.sql"
```

### Error: "EndpointRegistries not found in DbContext"
**Cause:** AppDbContext.cs missing DbSet declarations

**Fix:** Verify these lines exist in `AppDbContext.cs`:
```csharp
public virtual DbSet<EndpointRegistry> EndpointRegistries { get; set; }
public virtual DbSet<EndpointRolePermission> EndpointRolePermissions { get; set; }
public virtual DbSet<PermissionChangeAuditLog> PermissionChangeAuditLogs { get; set; }
```

### Error: "Could not load type 'IEndpointAuthorizationService'"
**Cause:** Service not registered in DI container

**Fix:** Verify these lines exist in `Program.cs`:
```csharp
builder.Services.AddScoped<IEndpointAuthorizationService, EndpointAuthorizationService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
```

---

## Success Criteria ✅

Your database tables are correctly accessible if:

- ✅ `/api/diagnostic/db-connection` returns `"success": true`
- ✅ `/api/diagnostic/endpoint-registry` returns `"totalCount": 126`
- ✅ `/api/diagnostic/endpoint-role-permission` returns `"totalCount": 500+`
- ✅ `/api/diagnostic/all-tables` returns `"success": true` for all 3 tables
- ✅ `/api/diagnostic/test-authorization-service` completes without errors
- ✅ No exceptions in application logs
- ✅ Cache hits logged after first request

---

## Quick Verification Script (PowerShell)

```powershell
# Save as Test-DatabaseAccess.ps1

$baseUrl = "http://localhost:44100"

Write-Host "Testing Database Access..." -ForegroundColor Cyan

# Test 1: Database Connection
Write-Host "`n1. Testing database connection..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/diagnostic/db-connection"
if ($response.success) {
    Write-Host "   ✅ Database connection: SUCCESS" -ForegroundColor Green
} else {
    Write-Host "   ❌ Database connection: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($response.message)" -ForegroundColor Red
}

# Test 2: EndpointRegistry Table
Write-Host "`n2. Testing EndpointRegistry table..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/diagnostic/endpoint-registry"
if ($response.success) {
    Write-Host "   ✅ EndpointRegistry: $($response.totalCount) records" -ForegroundColor Green
} else {
    Write-Host "   ❌ EndpointRegistry: FAILED" -ForegroundColor Red
}

# Test 3: EndpointRolePermission Table
Write-Host "`n3. Testing EndpointRolePermission table..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/diagnostic/endpoint-role-permission"
if ($response.success) {
    Write-Host "   ✅ EndpointRolePermission: $($response.totalCount) records" -ForegroundColor Green
} else {
    Write-Host "   ❌ EndpointRolePermission: FAILED" -ForegroundColor Red
}

# Test 4: All Tables
Write-Host "`n4. Testing all tables..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/diagnostic/all-tables"
if ($response.success) {
    Write-Host "   ✅ All tables accessible" -ForegroundColor Green
} else {
    Write-Host "   ❌ Some tables failed" -ForegroundColor Red
}

# Test 5: Authorization Service
Write-Host "`n5. Testing EndpointAuthorizationService..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/diagnostic/test-authorization-service"
if ($response.success) {
    Write-Host "   ✅ Authorization service: SUCCESS" -ForegroundColor Green
} else {
    Write-Host "   ❌ Authorization service: FAILED" -ForegroundColor Red
}

Write-Host "`n✅ Database verification complete!" -ForegroundColor Cyan
```

**Usage:**
```powershell
# Make sure application is running first
cd Documentation/SQL_Scripts
./Test-DatabaseAccess.ps1
```

---

## Next Steps After Verification

Once database access is confirmed:

1. ✅ **Step 5:** Test single endpoint with dynamic authorization
2. ✅ **Step 6:** Implement cache management endpoints
3. ✅ **Step 7:** Migrate all 86 endpoints
4. ✅ **Step 8:** Build admin UI

See `IMPLEMENTATION_STATUS_REPORT.md` for detailed next steps.
