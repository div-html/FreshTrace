# FreshTrace: Farm-to-Fork Food Traceability & Freshness Monitoring System

## Overview
**FreshTrace** is a comprehensive, standalone three-tier agricultural supply chain web platform adhering to **IEEE 830 SRS guidelines**, **SDG 12 (Target 12.3: Food Loss and Wastage Reduction)**, and **SDG 3 (Good Health and Well-being)**.

---

## Color Theme & Aesthetics
- **Theme:** Pistachio (Pista) Green & Warm Cream (Dark mode completely removed for pure, organic brand identity)
- **Palette Tokens:**
  - **Signature Pistachio Green:** `#629650` (Primary button: `#4c7b3d`, hover: `#3b622e`)
  - **Light Pistachio Tints & Badges:** `#e2eedc`, `#f2f7ee`, `#c7dec0`
  - **Warm Cream & Parchment Canvas:** `#faf7f0`, `#fdfcf9`, `#f4eee2`
  - **Crisp Surfaces & Cards:** `#ffffff` and `#fcfaf5` with warm sand/cream borders (`#e6dfd1`)
  - **Deep Contrast Forest & Charcoal Text:** `#1a2717`, `#344331`, `#677763` (Exceeds WCAG 2.1 AA/AAA contrast ratios)

### Profile Page & Farm Registration Experience
- **Progressive Vertical Storytelling:**
  - **Start (Top):** Producer Contact Credentials Hero Card showcasing the FreshTrace brand logo emblem, verified producer indicator, full name, email (unique system ID), and phone number. Includes a smooth down-arrow prompt button to jump to farm land records.
  - **Scrolled Down (Bottom):** Farm Land & Provenance Registration Card with automatic GPS detection, cultivated area, parcel topographic radar preview, and ledger synchronization.
  - **Smooth "Float Right-In" Physics:** As the user scrolls down, the farm registration card seamlessly glides into place from the right (`transform: translateX(85px) -> translateX(0)`, `opacity: 0 -> 1` with a custom `cubic-bezier(0.16, 1, 0.3, 1)` easing curve).
- **Pure React Architecture:** The entire application is built and rendered with React 18, state hooks (`useState`, `useEffect`, `useRef`), and seamless view orchestration.

---

## Implemented Requirements from the Synopsis Document

### 1. User Classes & Roles (Module 1: Authentication & Role Management)
The document specifies distinct stakeholder user classes:
- **Admin:** Manages users, products, and system settings; reviews and approves stakeholder accounts.
- **Farmer:** Registers farms, crops, and initial harvest batches with unique Batch IDs.
- **Processor:** Receives raw batches, manages splits and merges (batch genealogy), and logs processing wastage.
- **Transporter (Distributor / Logistics):** Dispatches shipments, logs condition telemetry (temperature in °C and humidity), and GPS checkpoints.
- **Retailer:** Receives batches, tracks low-stock/shelf-life expiry alerts, and records sale or discard actions.
- **Quality Inspector:** Performs batch inspections, records grades and safety certificates, and blocks defective batches.
- **Consumer (No Login Required):** Free access to scan QR codes or enter Batch IDs to verify provenance and cold-chain integrity without an account.

### 2. Module 1 Authentication Specifications
- **Clean Real-World Auth Flow:**
  - **Seamless Login / Register Selector:** Tabbed pill switcher allowing users to easily toggle between **Sign In** and **Register** without any redundant bottom prompt links.
  - **Animated Background Transit Scenery:** A dynamic, CSS-animated scenery in the background featuring a refrigerated agricultural delivery truck traveling continuously along a highway from **Farm Origin (Shimla)** with windmill and barn to **Market Hub (Delhi NCR)** with eco-warehouse and stores.
  - **Login Page Fields:** Email Address and Password (with show/hide eye toggle, remember me, and forgot password).
  - **Registration Page Fields:** Full Name, Email Address, Password (with eye toggle), Phone Number, and Supply Chain Role dropdown (`Farmer`, `Processor`, `Transporter`, `Retailer`, `Quality Inspector`).
  - **Clean & Distraction-Free UI:** Top logo removed from the auth card for maximum simplicity; clean Pistachio Green & Warm Cream aesthetics.
- **Role Landing Dashboards:** Upon login, each user lands on a customized preview matching their role:
  - *Farmer:* Views registered farms, harvest lots, and crop status.
  - *Transporter:* Views assigned refrigerated trucks, GPS checkpoints, and real-time transit temperatures.
  - *Admin:* Views global user statistics, active batches, and pending registrations awaiting approval.
  - *Processor:* Views raw lots received and wastage reduction metrics.
  - *Retailer:* Views in-store batch stock and expiry warnings.
  - *Inspector:* Views pending batch inspections and grade certifications.

### 3. Consumer Traceability Portal (Module 9)
- Consumers can verify batches (e.g. `BATCH-2026-APL-098`, `BATCH-2026-TOM-441`, `BATCH-2026-STR-892`) with **zero login required**.
- Displays interactive origin farm data, GPS coordinates, inspection certificate, cold-chain temperature log, and freshness index.

---

## Project Structure
```
FreshTrace/
├── index.html                 # Self-contained, zero-dependency interactive portal
├── server.js                  # Fast Node.js server (Node 18+)
├── package.json               # Project manifest and scripts
├── README.md                  # System & design documentation
└── src/
    ├── App.jsx                # Core React application component
    ├── main.jsx               # React DOM entrypoint
    ├── components/
    │   ├── Navbar.jsx         # Header with branding and consumer quick-trace
    │   ├── HeroSection.jsx    # Supply chain journey, SDG badges & demo role chips
    │   ├── LoginForm.jsx      # Role-based sign-in form with remember me
    │   ├── RegisterForm.jsx   # Stakeholder registration with strength meter
    │   ├── ConsumerTraceModal.jsx # Zero-login batch QR provenance viewer
    │   ├── ForgotPasswordModal.jsx # Password reset modal
    │   └── RoleDashboardModal.jsx # Role-specific landing dashboard preview
    ├── data/
    │   └── mockData.js        # Normalized mock entities (Tables 01 to 27)
    └── styles/
        └── theme.css          # Pista green & white design tokens
```

---

## How to Run
### Option 1: Direct in Browser
Open `index.html` directly in any modern browser (Chrome, Edge, Brave, Firefox) by double-clicking it.

### Option 2: Run via Node.js
```bash
npm start
# or
node server.js
```
Then navigate to `http://localhost:3000`.
