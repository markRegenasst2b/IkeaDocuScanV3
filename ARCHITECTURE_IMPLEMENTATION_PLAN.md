# IkeaDocuScan Architecture Implementation Plan

## Overview
This document outlines the architecture and implementation plan for adding a data access layer to the IkeaDocuScan-V3 Blazor application with Entity Framework Core, supporting highly interactive CRUD operations with real-time updates.

---

## Current State

### Existing Projects
```
IkeaDocuScanV3/
├── IkeaDocuScan-Web/              (Server - ASP.NET Core)
│   ├── Services/                  UserIdentityService
│   ├── Middleware/                WindowsIdentityMiddleware
│   └── Components/Pages/          ServerHome.razor, Identity.razor
├── IkeaDocuScan-Web.Client/       (Client - Blazor WebAssembly)
│   └── Pages/                     Home.razor, Counter.razor, Weather.razor
├── IkeaDocuScanV3.AppHost/        (Aspire Orchestration)
└── IkeaDocuScanV3.ServiceDefaults/ (Shared Configuration)
```

### Authentication
- ✅ Windows Authentication working via `WindowsIdentityMiddleware`
- ✅ Username captured: `TALLINN\markr`
- ✅ Available in `HttpContext.User.Identity.Name`

### Current Architecture Issues
- No data access layer
- No separation between EF entities and DTOs
- Client pages (Counter, Weather) are placeholder demos to be removed
- No real-time update capability

### Architecture Decisions

#### Blazor Rendering Mode - SSR Disabled
**Decision Date:** 2025-01-23

**Decision:** Disable server-side pre-rendering (SSR) globally while keeping InteractiveAuto render mode.

**Configuration:**
```csharp
// Program.cs lines 159-160
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(prerender: false)
    .AddInteractiveWebAssemblyRenderMode(prerender: false)
```

**Problem Solved:**
- "No root component exists with SSR component ID X" errors during page navigation
- Component event handlers becoming unresponsive after navigation
- Component tree desynchronization between server and client

**Technical Details:**
- InteractiveAuto with SSR enabled caused component ID mismatches during full page navigation
- Server pre-rendered components with IDs that client couldn't match during hydration
- After error, Blazor's event handler registration broke, making buttons unresponsive

**Trade-offs:**
- ✅ Eliminates SSR component ID errors
- ✅ Maintains InteractiveAuto benefits (automatic server/client switching)
- ✅ Preserves full page navigation functionality
- ❌ Slightly slower initial page load (no pre-rendered HTML)
- ❌ Blank screen during initial WebAssembly download

**Alternative Considered:**
Single-page approach with conditional rendering was rejected to maintain natural browser navigation patterns and URL-based routing.

---

## Target Architecture

See full document at: `/ARCHITECTURE_IMPLEMENTATION_PLAN.md`

## Continuation Prompt

```
I need to implement the data access layer for IkeaDocuScan-V3 based on the architecture defined in ARCHITECTURE_IMPLEMENTATION_PLAN.md.

Please:
1. Create the IkeaDocuScan.Shared project with folder structure
2. Create the IkeaDocuScan.Infrastructure project
3. Scaffold the DbContext from the existing IkeaDocuScan database using SQL user 'docuscanch'
4. Set up project references (no circular dependencies)
5. Implement a sample Document entity with CRUD operations and real-time updates
6. Create the client page for document management

The database already exists, and Windows authentication is already working via WindowsIdentityMiddleware.
```

---

**Document Version:** 1.0
**Last Updated:** 2025-01-16
**Full Document:** `/ARCHITECTURE_IMPLEMENTATION_PLAN.md`
