# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

IkeaDocuScan is an enterprise document management and scanning system built with .NET 9.0 Aspire. It uses a Blazor hybrid rendering architecture (server-side + WebAssembly client) with real-time updates via SignalR, Windows Authentication with Active Directory integration, and comprehensive audit logging.

## Essential Development Commands

### Building and Running

```bash
# Build the entire solution
dotnet build

# Run the web application (from solution root)
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web

# Run with Aspire orchestration
dotnet run --project IkeaDocuScanV3.AppHost

# Restore dependencies
dotnet restore
```

### Database Migrations

```bash
# Add new migration (run from Infrastructure project directory)
cd IkeaDocuScan.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web

# Update database
dotnet ef database update --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

### Configuration Encryption

```bash
# Encrypt sensitive configuration values (Windows only)
dotnet run --project ConfigEncryptionTool
```

## Architecture

### Layered Architecture

The codebase follows clean architecture principles with strict separation:

```
IkeaDocuScan-Web.Client (Blazor WASM)
    ↓
IkeaDocuScan-Web (ASP.NET Core + Blazor Server)
    ↓
Services Layer (Business Logic)
    ↓
IkeaDocuScan.Infrastructure (EF Core + Data Access)
    ↓
SQL Server Database
```

**IkeaDocuScan.Shared** provides cross-cutting concerns (DTOs, interfaces, exceptions, configuration helpers) shared by all layers.

### Key Architectural Patterns

1. **Hybrid Blazor Rendering (SSR Disabled)**
   - **Render Mode**: InteractiveAuto with prerendering disabled globally
   - **SSR Decision**: Server-side pre-rendering (SSR) is disabled to avoid component ID conflicts during navigation
   - **Configuration**: `Program.cs` lines 159-160 set `prerender: false` for both Server and WebAssembly modes
   - **Rationale**: Full page navigation with InteractiveAuto caused "No root component exists with SSR component ID" errors
   - Server-side components in `IkeaDocuScan-Web/Pages/` (Identity.razor, ServerHome.razor, Error.razor)
   - Client-side components in `IkeaDocuScan-Web.Client/Pages/` (Documents.razor, CheckinScanned.razor, etc.)
   - Components communicate via HTTP API endpoints and SignalR

2. **Service-Oriented Design**
   - All business logic in services under `IkeaDocuScan-Web/Services/`
   - Each service has a corresponding interface in `IkeaDocuScan.Shared/Interfaces/`
   - Dependency injection configured in `Program.cs`

3. **API Endpoints (Minimal API style)**
   - REST endpoints defined in `IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs`
   - Standard CRUD pattern: GET, POST, PUT, DELETE
   - Client-side service `DocumentHttpService.cs` consumes these endpoints

4. **Real-Time Updates**
   - SignalR hub: `IkeaDocuScan-Web/Hubs/DataUpdateHub.cs`
   - Clients subscribe to data change notifications
   - Services invoke hub methods to notify connected clients

5. **Authentication & Authorization**
   - **Windows**: WindowsIdentityMiddleware.cs resolves AD users and groups
   - **Linux/WSL**: TestAuthenticationHandler simulates Windows auth for development
   - Custom authorization policies: `HasAccess`, `SuperUser` (Authorization/UserAccessHandler.cs)
   - Database-backed permissions: `DocuScanUser` and `UserPermission` entities

6. **Configuration Layering** (Priority: highest to lowest)
   - Environment variables (IIS App Pool)
   - `secrets.encrypted.json` (DPAPI-encrypted, not committed)
   - `appsettings.Local.json` (server-specific, not committed)
   - `appsettings.{Environment}.json`
   - `appsettings.json` (defaults)

## Project Structure

### IkeaDocuScan-Web (Server)
- **Components/**: Blazor root components (App.razor, Routes.razor)
- **Pages/**: Server-rendered Razor pages
- **Services/**: Business logic
  - `DocumentService.cs`: Document CRUD operations
  - `AuditTrailService.cs`: Audit logging for compliance
  - `ScannedFileService.cs`: File system operations with caching and security validation
  - `EmailService.cs`: SMTP email notifications (MailKit)
  - `UserIdentityService.cs`: AD user resolution
  - `CurrentUserService.cs`: Scoped user context (caches DB lookups)
  - `UserPermissionService.cs`: User permission CRUD operations
  - `CounterPartyService.cs`: Counter party management
  - `CountryService.cs`: Country data
  - `DocumentTypeService.cs`: Document type management
- **Endpoints/**: RESTful API definitions
  - `DocumentEndpoints.cs`: Document CRUD API
  - `CounterPartyEndpoints.cs`: Counter party API
  - `CountryEndpoints.cs`: Country API
  - `DocumentTypeEndpoints.cs`: Document type API
  - `UserPermissionEndpoints.cs`: User permission and DocuScanUser API
  - `ScannedFileEndpoints.cs`: Scanned file access API
  - `AuditTrailEndpoints.cs`: Audit trail logging and retrieval API
- **Middleware/**: Custom middleware (Windows auth, exception handling)
- **Authorization/**: Custom authorization policies and handlers
- **Hubs/**: SignalR hubs for real-time updates
- **Program.cs**: Application startup and DI configuration

### IkeaDocuScan-Web.Client (WebAssembly)
- **Pages/**: Client-side Razor components
  - `Documents.razor`: Main document management UI
  - `CheckinScanned.razor`: Scanned file listing
  - `CheckinFileDetail.razor`: File detail view
  - `EditUserPermissions.razor`: User permissions management
- **Services/**: Client-side HTTP services (all implement corresponding interfaces)
  - `DocumentHttpService.cs`: Document API client
  - `CounterPartyHttpService.cs`: Counter party API client
  - `CountryHttpService.cs`: Country API client
  - `DocumentTypeHttpService.cs`: Document type API client
  - `UserPermissionHttpService.cs`: User permission API client
  - `ScannedFileHttpService.cs`: Scanned file API client
  - `AuditTrailHttpService.cs`: Audit trail API client
- **Program.cs**: WebAssembly host configuration (registers all HTTP services)

### IkeaDocuScan.Infrastructure
- **Data/AppDbContext.cs**: EF Core DbContext with all entity configurations
- **Entities/**: Database models (12 entity types)
  - Core: `Document`, `DocumentFile`, `AuditTrail`
  - Reference data: `DocumentType`, `DocumentName`, `Country`, `Currency`
  - Relationships: `CounterParty`, `CounterPartyRelation`
  - Security: `DocuScanUser`, `UserPermission`

### IkeaDocuScan.Shared
- **DTOs/**: Data transfer objects (separate Create/Update/Read DTOs)
- **Interfaces/**: Service interfaces for DI and testability
- **Exceptions/**: Custom exceptions (BusinessException, ValidationException, DocumentNotFoundException)
- **Configuration/**:
  - `IkeaDocuScanOptions.cs`: Strongly-typed configuration
  - `DpapiConfigurationHelper.cs`: Windows DPAPI encryption
  - `EncryptedJsonConfigurationProvider.cs`: Custom config provider
- **Enums/**: `AuditAction` for audit trail
- **Models/**: `CurrentUser` for authorization context

### IkeaDocuScanV3.AppHost
- Aspire orchestration for distributed application management
- Configures service defaults (telemetry, health checks, resilience)

### IkeaDocuScanV3.ServiceDefaults
- OpenTelemetry integration (metrics, tracing, logging)
- Health check endpoints
- HTTP resilience handlers
- Service discovery configuration

### ConfigEncryptionTool
- CLI tool for encrypting configuration values using Windows DPAPI
- Outputs `secrets.encrypted.json` (excluded from source control)

## Critical Implementation Details

### Security Requirements

1. **File Path Validation** (ScannedFileService.cs:~150)
   - All file paths MUST be validated against path traversal attacks
   - Use `Path.GetFullPath()` and verify the resolved path starts with the base directory
   - Never trust user-supplied file paths

2. **File Extension Whitelist** (appsettings.json: IkeaDocuScan:AllowedFileExtensions)
   - Only allow explicitly whitelisted extensions (.pdf, .jpg, .png, .tif, .tiff, .doc, .docx, .xls, .xlsx)
   - Case-insensitive comparison
   - Default max file size: 50MB

3. **DPAPI Encryption** (Windows-specific)
   - Sensitive configuration (connection strings, SMTP credentials) encrypted with DPAPI
   - Only decrypt on Windows machines with proper user context
   - Linux/WSL: Use unencrypted appsettings.Local.json for development

4. **Authorization Checks**
   - Endpoints protected with `[Authorize(Policy = "HasAccess")]`
   - SuperUser policy for administrative operations
   - CurrentUserService provides user context throughout the request lifecycle

### Database Relationships

Key foreign key relationships in AppDbContext.cs:
- `Document.CounterPartyId` → `CounterParty.Id` (required)
- `Document.CountryId` → `Country.Id` (required)
- `Document.DocumentTypeId` → `DocumentType.Id` (required)
- `CounterPartyRelation` joins CounterParty to multiple entities
- `UserPermission.UserId` → `DocuScanUser.Id` (optional, null = public access)

Cascade delete configured for `DocumentFile` when parent `Document` is deleted.

### Service Dependencies

When adding or modifying services:
1. Define interface in `IkeaDocuScan.Shared/Interfaces/`
2. Implement service in `IkeaDocuScan-Web/Services/`
3. Register in `Program.cs` (scoped for DB-dependent services, transient for stateless)
4. Inject `ICurrentUserService` for user context
5. Inject `IAuditTrailService` for compliance logging
6. Consider SignalR notifications via `IHubContext<DataUpdateHub>`

### Audit Trail Pattern

All data-modifying operations MUST log to audit trail:
```csharp
await _auditTrailService.LogAsync(AuditAction.Create, $"Document {dto.Name} created", userId);
```

Actions defined in `AuditAction` enum: Create, Read, Update, Delete, CheckIn, Export, AccessRequest, Login, Logout.

### Email Notifications

Email service (`EmailService.cs`) used for:
- Access request notifications to administrators
- Document activity notifications to users
- System alerts

SMTP configuration in `appsettings.json` or `secrets.encrypted.json`.

### Caching Strategy

ScannedFileService implements in-memory caching:
- 60-second TTL for file list cache
- Cache key: scanned files directory path
- Invalidate cache on write operations (check-in, delete)

## Development Environment

### Windows vs. Linux/WSL

**Windows (Production-like):**
- Windows Authentication (NTLM/Negotiate)
- DPAPI encryption works natively
- Active Directory group resolution

**Linux/WSL (Development):**
- TestAuthenticationHandler simulates Windows auth
- No DPAPI support (use plain appsettings.Local.json)
- Mock AD groups via configuration or test handler

### Configuration Files

**Not committed to Git:**
- `appsettings.Local.json` (server-specific paths, connection strings)
- `secrets.encrypted.json` (DPAPI-encrypted secrets)
- `*.user` files (Visual Studio user settings)

**Committed to Git:**
- `appsettings.json` (defaults with placeholders)
- `appsettings.Development.json` (dev logging levels)

### Launch Profiles

Defined in `Properties/launchSettings.json`:
- HTTP: http://localhost:44100
- HTTPS: https://localhost:44101
- IIS Express profile available for Visual Studio

## Blazorise UI Framework

The application uses Blazorise 1.8.5 with Bootstrap5 theme.

**Key components:**
- DataGrid: `<DataGrid>` for tabular data (Documents.razor)
- Modal: `<Modal>` for dialogs
- Buttons: `<Button Color="Color.Primary">`
- Forms: `<Field>`, `<FieldLabel>`, `<TextEdit>`, `<Select>`

Setup instructions in `Documentation/BLAZORISE_SETUP_INSTRUCTIONS.md`.

## Common Development Scenarios

### Adding a new entity

1. Create entity class in `IkeaDocuScan.Infrastructure/Entities/`
2. Add `DbSet<TEntity>` to `AppDbContext.cs`
3. Configure relationships in `OnModelCreating()`
4. Create migration: `dotnet ef migrations add AddEntity`
5. Create DTOs in `IkeaDocuScan.Shared/DTOs/`
6. Create service interface in `IkeaDocuScan.Shared/Interfaces/`
7. Implement service in `IkeaDocuScan-Web/Services/`
8. Add endpoints in `IkeaDocuScan-Web/Endpoints/`
9. Register service in `Program.cs`
10. Create Razor component in `IkeaDocuScan-Web.Client/Pages/`

### Adding a new API endpoint

1. Add method to `DocumentEndpoints.cs` (or create new endpoints file)
2. Map endpoint in `Program.cs`: `app.MapDocumentEndpoints();`
3. Create corresponding method in client-side `DocumentHttpService.cs`
4. Update Razor component to call client service

### Adding real-time updates

1. Inject `IHubContext<DataUpdateHub>` into service
2. Call `await _hubContext.Clients.All.SendAsync("MethodName", data);`
3. In Blazor component, subscribe to hub method: `_hubConnection.On<T>("MethodName", handler);`

### Modifying authentication/authorization

- Windows auth logic: `Middleware/WindowsIdentityMiddleware.cs`
- Test auth logic: `TestAuthenticationHandler.cs` (registered in Program.cs on Linux)
- Authorization policies: `Authorization/UserAccessHandler.cs`
- User resolution: `Services/UserIdentityService.cs`
- Active Directory groups: See `Documentation/AD_GROUPS_QUICK_REFERENCE.md`

## Documentation

Comprehensive guides available in `Documentation/`:
- `README_CHECKIN_SCANNED.md`: Scanned file check-in feature
- `DEPLOYMENT_GUIDE.md`: Production deployment steps
- `AUTHORIZATION_GUIDE.md`: Authentication and authorization system
- `EMAIL_SERVICE_GUIDE.md`: Email notification configuration
- `BLAZORISE_SETUP_INSTRUCTIONS.md`: UI framework setup
- `AD_GROUPS_QUICK_REFERENCE.md`: Active Directory group reference

## Testing

**Current Status:** No test projects exist yet.

**Testing TODO:**
- Unit tests for ScannedFileService (path traversal, extension filtering)
- Integration tests for DocumentEndpoints
- Unit tests for DpapiConfigurationHelper
- Mocking tests for services using interfaces

**Test-Ready Features:**
- All services use interfaces (easy to mock)
- Dependency injection throughout
- Custom exceptions for specific error cases
- Global exception handler provides consistent error responses
