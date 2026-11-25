# FreshTrace: Farm-to-Fork Food Traceability & Freshness Monitoring System (C# / ASP.NET Core)

## Overview
**FreshTrace** is a farm-to-fork agricultural supply chain traceability web platform built with **C# and ASP.NET Core Razor Pages (.NET 11)**, adhering to **IEEE 830 SRS guidelines**, **SDG 12 (Target 12.3: Food Loss and Wastage Reduction)**, and **SDG 3 (Good Health and Well-being)**.

---

## Color Theme & Aesthetics
- **Theme:** Pistachio (Pista) Green & Warm Cream
- **Palette Tokens:**
  - **Signature Pistachio Green:** `#629650` (Primary button: `#4c7b3d`, hover: `#3b622e`)
  - **Light Pistachio Tints & Badges:** `#e2eedc`, `#f2f7ee`, `#c7dec0`
  - **Warm Cream & Parchment Canvas:** `#faf7f0`, `#fdfcf9`, `#f4eee2`
  - **Crisp Surfaces & Cards:** `#ffffff` and `#fcfaf5` with warm sand/cream borders (`#e6dfd1`)
  - **Deep Contrast Forest & Charcoal Text:** `#1a2717`, `#344331`, `#677763`

---

## C# Architecture & Features

### 1. Authentication & Role Management (`Pages/Index.cshtml.cs`, `Services/FreshTraceDataService.cs`)
- Cookie-based authentication (`CookieAuthenticationDefaults`) and server-side session state.
- PBKDF2 password hashing (`Rfc2898DeriveBytes`) and persistent JSON data store (`data/freshtrace-state.json`).
- **Sign In & Register** tabs with animated Farm-to-Fork truck transit scenery and Forgot Password modal.
- **Demo Accounts** (Password: `Password123!`):
  - Farmer: `farmer.ramesh@greenvalley.com`
  - Admin: `admin@freshtrace.org`
  - Retailer: `retailer@freshmart.com`
  - Distributor: `distributor@coldchain.com`
  - Consumer: `consumer@freshtrace.org`

### 2. Farmer Portal (`Pages/Farmer.cshtml` / `Pages/Farmer.cshtml.cs`)
- Collapsible left botanical navigation rail with 4 sections:
  - **Dashboard:** KPI summary, recent harvest batches, registered farms, and cold-chain readiness.
  - **Batch Information:** Status filter pills, search, **Log New Harvest Batch** modal, dispatch workflow, and **Batch Provenance & QR Label** modal rendered via pure C# SVG generation (`Services/QrSvgRenderer.cs`).
  - **Shipments:** Live cold-chain shipment tracking, 4-step journey progress, delivery confirmation, and ready-for-pickup queue.
  - **Profile:** Producer identity editor, GPS auto-detection, and registered farm parcels management.

### 3. Role Dashboards & Consumer Traceability (`Pages/Dashboard.cshtml`, `Pages/Profile.cshtml`)
- Dedicated role portals for **Admin**, **Retailer**, **Distributor**, and **Consumer**.
- **Instant Batch Origin Lookup** for consumers + `/api/trace/{batchCode}` JSON endpoint and `/health` check.
- Standalone **Producer Profile & Farm Registration** page (`/Profile`).

---

## Project Structure
```
FreshTrace/
├── FreshTrace.csproj                # ASP.NET Core (.NET 11) C# Web Project
├── Program.cs                       # App startup, auth, session, Razor Pages & API routes
├── Properties/
│   └── launchSettings.json          # Local HTTP/HTTPS launch URLs
├── Models/
│   └── FreshTraceModels.cs          # C# domain models & records
├── Services/
│   ├── FreshTraceDataService.cs     # Thread-safe data store, auth & business logic
│   └── QrSvgRenderer.cs             # Pure C# deterministic SVG QR generator
├── Pages/
│   ├── Index.cshtml(.cs)            # Sign In / Register & Forgot Password
│   ├── Farmer.cshtml(.cs)           # 4-section Farmer Workspace & modals
│   ├── Dashboard.cshtml(.cs)        # Admin, Retailer, Distributor & Consumer portals
│   ├── Profile.cshtml(.cs)          # Producer Profile & Farm Registration
│   └── Shared/
│       ├── _Layout.cshtml           # Shared HTML layout
│       └── _TruckBackdrop.cshtml    # Animated farm-to-fork backdrop partial
├── wwwroot/
│   ├── css/theme.css                # Pista green & warm cream theme stylesheet
│   └── js/app.js                    # Progressive UI helpers (GPS, eye toggle, copy, modals)
└── data/
    └── freshtrace-state.json        # Persisted runtime state (auto-created on startup)
```

---

## How to Run
Requires [.NET 11 SDK](https://dotnet.microsoft.com/download) (or compatible .NET SDK).

```bash
dotnet run
```
Then open `http://localhost:5050` in your browser.
