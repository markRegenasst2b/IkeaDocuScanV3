# IkeaDocuScan Architecture & Implementation Plan

## Document Information

**Last Updated:** 2025-01-27
**Version:** 2.0
**Project:** IkeaDocuScan-V3 Blazor Application

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Current Architecture](#current-architecture)
3. [Blazor Rendering Configuration](#blazor-rendering-configuration-working)
4. [Known Issues & Solutions](#known-issues--solutions)
5. [Critical Rules](#critical-rules)
6. [File Structure](#file-structure)
7. [Testing Checklist](#testing-checklist)

---

## Project Overview

IkeaDocuScan is an enterprise document management and scanning system built with .NET 9.0 Aspire. It uses a Blazor hybrid rendering architecture (server-side + WebAssembly client) with real-time updates via SignalR, Windows Authentication with Active Directory integration, and comprehensive audit logging.

### Existing Projects

```
IkeaDocuScanV3/
├── IkeaDocuScan-Web/              (Server - ASP.NET Core)
│   ├── Services/                  Business logic services
│   ├── Middleware/                WindowsIdentityMiddleware
│   ├── Components/Pages/          Server-rendered pages
│   ├── Endpoints/                 REST API endpoints
│   └── Hubs/                      SignalR hubs
├── IkeaDocuScan-Web.Client/       (Client - Blazor WebAssembly)
│   ├── Pages/                     Client-rendered pages
│   ├── Components/                Shared components
│   ├── Services/                  HTTP API clients
│   └── wwwroot/js/                External JavaScript
├── IkeaDocuScan.Infrastructure/   (Data Access)
│   ├── Data/                      EF Core DbContext
│   └── Entities/                  Database models
├── IkeaDocuScan.Shared/           (Cross-cutting)
│   ├── DTOs/                      Data transfer objects
│   ├── Interfaces/                Service interfaces
│   ├── Exceptions/                Custom exceptions
│   └── Configuration/             Config helpers
├── IkeaDocuScanV3.AppHost/        (Aspire Orchestration)
└── IkeaDocuScanV3.ServiceDefaults/ (Shared Configuration)
```

### Authentication & Authorization

- ✅ Windows Authentication via `WindowsIdentityMiddleware`
- ✅ Active Directory group resolution
- ✅ Database-backed permissions (`DocuScanUser`, `UserPermission`)
- ✅ Custom authorization policies: `HasAccess`, `SuperUser`

### Historical Architecture Decisions

#### SSR Disabled Decision (January 2025)

**Decision Date:** 2025-01-23

**Problem:** "No root component exists with SSR component ID X" errors during full page navigation with InteractiveAuto render mode.

**Root Cause:**
- InteractiveAuto with SSR enabled caused component ID mismatches during full page navigation
- Server pre-rendered components with IDs that client couldn't match during hydration
- After error, Blazor's event handler registration broke, making buttons unresponsive

**Solution:** Disable server-side pre-rendering (SSR) globally while keeping InteractiveAuto render mode.

**Configuration:**
```csharp
// Program.cs lines 159-160
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(prerender: false)
    .AddInteractiveWebAssemblyRenderMode(prerender: false)
```

**Trade-offs:**
- ✅ Eliminates SSR component ID errors
- ✅ Maintains InteractiveAuto benefits (automatic server/client switching)
- ✅ Preserves full page navigation functionality
- ❌ Slightly slower initial page load (no pre-rendered HTML)
- ❌ Blank screen during initial WebAssembly download

**Alternative Considered:**
Single-page approach with conditional rendering was rejected to maintain natural browser navigation patterns and URL-based routing.

**Current Status:** This decision was superseded by the current hybrid rendering configuration (see below) which uses InteractiveServer and InteractiveWebAssembly modes separately with prerendering disabled only for WebAssembly pages.

---

## Current Architecture

### Layered Architecture

```
Client (Blazor WASM) → HTTP APIs → Services → Infrastructure → Database
                    ↘ SignalR Hub for real-time updates
```

### Key Patterns

1. **Service-Oriented Design:** All business logic in services with interface-based DI
2. **API Endpoints:** REST endpoints using Minimal API style
3. **Real-Time Updates:** SignalR for data change notifications
4. **Audit Logging:** Comprehensive audit trail for compliance

---

## Blazor Rendering Configuration (WORKING)

**Last Updated:** 2025-01-27

### Current Working Configuration

The IkeaDocuScan application uses **Blazor Hybrid Rendering** with the following configuration that is confirmed to work correctly:

#### Server Configuration (Program.cs)

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IkeaDocuScan_Web.Client._Imports).Assembly);
```

**Location:** `IkeaDocuScan-Web/IkeaDocuScan-Web/Program.cs` (lines 151-155)

#### Client-Side Page Configuration

**For WebAssembly pages (client-side interaction):**

```razor
@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))
```

**Location:** Applied at the top of client-side pages, e.g., `IkeaDocuScan-Web.Client/Pages/DocumentPropertiesPage.razor` (line 6)

### Why Prerendering is Disabled

**Prerendering is set to `false`** to prevent component lifecycle issues:

1. **Navigation Issues:** With prerendering enabled, full page navigation with InteractiveWebAssemblyRenderMode caused "No root component exists with SSR component ID" errors
2. **Component Disposal:** Prerendering caused components to be disposed and recreated, breaking:
   - JavaScript interop initialization
   - Navigation interception for unsaved changes warnings
   - Event handler registration
3. **State Management:** Prerendering caused state inconsistencies during navigation transitions

### Architecture Components

#### Client Components
- **Location:** `IkeaDocuScan-Web.Client/Pages/`
- **Render Mode:** InteractiveWebAssemblyRenderMode (no prerender)
- **Examples:**
  - `DocumentPropertiesPage.razor` - Document editing with change detection
  - `Documents.razor` - Document listing
  - `CheckinScanned.razor` - File check-in interface

#### Server Components
- **Location:** `IkeaDocuScan-Web/Pages/`
- **Render Mode:** Default server-side rendering
- **Examples:**
  - `Identity.razor` - Authentication pages
  - `ServerHome.razor` - Server-rendered home page
  - `Error.razor` - Error handling pages

#### Shared Components
- **Location:** `IkeaDocuScan-Web.Client/Components/`
- **Inherit render mode from parent page**
- **Examples:**
  - `DocumentSectionFields.razor` - Form field groups
  - `CounterPartySelector.razor` - Complex selection controls
  - `ThirdPartySelector.razor` - Multi-select components

### Communication Patterns

#### API Communication
- Client components call HTTP APIs via `*HttpService.cs` classes
- Server-side services implement business logic
- All services use interfaces defined in `IkeaDocuScan.Shared/Interfaces/`

#### Real-Time Updates
- SignalR hub: `DataUpdateHub.cs` at `/hubs/data-updates`
- Used for notifying clients of data changes
- Clients subscribe to hub notifications

### JavaScript Interop

#### Navigation Interception
- **File:** `wwwroot/js/navigationInterceptor.js`
- **Purpose:** Prevents navigation when unsaved changes exist
- **Pattern:** External JS file with DotNetObjectReference for callbacks
- **Initialization:** Called in `OnAfterRenderAsync(firstRender: true)`

#### Best Practices for JS Interop in WebAssembly Mode
1. **Wait for first render:** Initialize JS in `OnAfterRenderAsync` with `firstRender: true`
2. **Check script availability:** Poll for script existence before invoking (max 5 seconds)
3. **Use external files:** Avoid inline JS via `eval()` - use separate `.js` files
4. **Clean up resources:** Dispose `DotNetObjectReference` in `IDisposable.Dispose()`
5. **Handle disposal timing:** External JS persists after component disposal

### Known Issues & Solutions

#### Issue: Component Disposed Before Navigation Handler Fires
**Problem:** When using `RegisterLocationChangingHandler`, the component gets disposed before the handler can prevent navigation.

**Solution:** Use external JavaScript to intercept link clicks at the DOM level before Blazor processes them:
```javascript
document.addEventListener('click', handler, true); // Capture phase
```

#### Issue: Change Detection Triggers During Initial Load
**Problem:** Child components loading data asynchronously trigger change detection callbacks before the page is ready.

**Solution:** Use `enableChangeTracking` flag with delayed activation:
```csharp
// Disable during load
enableChangeTracking = false;

// Enable after delay
await Task.Delay(500);
enableChangeTracking = true;
```

#### Issue: Infinite Loop in Change Detection
**Problem:** `CheckForChanges()` calling `StateHasChanged()` triggers child re-renders, which call `CheckForChanges()` again.

**Solution:** Use recursive call guard and state-change detection:
```csharp
if (isCheckingForChanges) return;
try {
    isCheckingForChanges = true;
    // Only call StateHasChanged() if state actually changed
    if (hadChanges != hasUnsavedChanges) {
        StateHasChanged();
    }
} finally {
    isCheckingForChanges = false;
}
```

### Critical Rules

1. ✅ **ALWAYS** disable prerendering for WebAssembly pages: `prerender: false`
2. ✅ **NEVER** use `RegisterLocationChangingHandler` for navigation interception in WASM mode
3. ✅ **ALWAYS** wait for scripts to load before initializing JS interop
4. ✅ **ALWAYS** disable change tracking during page load
5. ✅ **ALWAYS** use external JS files instead of inline eval
6. ✅ **ALWAYS** guard against recursive calls in change detection
7. ✅ **NEVER** assume component lifecycle methods run synchronously with navigation

### File Structure

```
IkeaDocuScan-Web/
├── IkeaDocuScan-Web/               # Server project
│   ├── Program.cs                   # Render mode registration
│   ├── Components/
│   │   └── App.razor                # Root component
│   ├── Pages/                       # Server-rendered pages
│   └── Services/                    # Business logic
│
└── IkeaDocuScan-Web.Client/        # WebAssembly project
    ├── Pages/                       # Client-rendered pages
    │   └── DocumentPropertiesPage.razor
    ├── Components/                  # Shared components
    ├── Services/                    # HTTP API clients
    ├── Layout/                      # Layout components
    └── wwwroot/
        └── js/
            └── navigationInterceptor.js  # External JS
```

### Testing Checklist

When modifying rendering configuration, verify:

- [ ] Pages load without "SSR component ID" errors
- [ ] Navigation between pages works smoothly
- [ ] JavaScript interop initializes correctly
- [ ] Component lifecycle methods fire in expected order
- [ ] Event handlers persist through navigation
- [ ] No infinite loops or excessive re-renders
- [ ] Change detection works for all form fields
- [ ] Navigation warnings appear when expected
- [ ] Browser back/forward buttons work correctly

---

**Note:** This configuration has been tested and verified to work correctly as of January 2025. Any changes to render modes should be carefully tested against all the issues documented above.
