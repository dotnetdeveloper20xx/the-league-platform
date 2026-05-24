# 🏏 The League Platform

> **The all-in-one SaaS platform for sports club management.** Memberships, sessions, events, competitions, payments, facilities — unified under one roof.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?logo=angular)](https://angular.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-v4-06B6D4?logo=tailwindcss)](https://tailwindcss.com/)
[![DaisyUI](https://img.shields.io/badge/DaisyUI-5.x-5A0EF8)](https://daisyui.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 🎯 What Is The League?

**The League** is a production-grade, multi-tenant SaaS platform that replaces the spreadsheets, WhatsApp groups, and disconnected tools that sports clubs rely on today.

One platform. Every sport. Every club operation — from member sign-up to match-day scoring.

### Who It Serves

| Role | What They Get |
|------|--------------|
| **Platform Owner** | Recurring revenue from club subscriptions, transaction fees on every payment, full platform analytics |
| **Club Administrators** | Complete club operations: members, finances, sessions, events, competitions, facilities |
| **Club Members** | Self-service portal: book sessions, register for events, pay fees, track everything |

### Sports Supported

Cricket • Football • Hockey • Rugby • Tennis • Swimming • Athletics • Golf • Multi-Sport • Community Groups

---

## 💰 Business Model

| Revenue Stream | How It Works |
|---------------|-------------|
| **Subscription Tiers** | Free / Starter (£29/mo) / Pro (£79/mo) / Enterprise (£199/mo) |
| **Transaction Fees** | 1-2% on every payment processed through the platform (Stripe Connect) |
| **Add-Ons** | SMS packs, extra storage, white-label branding, custom domains |
| **Enterprise** | Custom pricing for governing bodies managing 50+ clubs |

---

## ✨ Key Features

### 🏢 Platform Management
- Multi-tenant architecture with complete data isolation per club
- Tiered subscriptions with feature gating and usage limits
- Self-service club onboarding with guided wizard
- Platform-wide analytics, health scores, and churn prediction
- 14-day free trial with automatic conversion flow

### 👥 Member Management
- Full member lifecycle: application → active → renewal/expiry
- Family accounts with dependents
- Custom fields per club (Text, Number, Date, Select, etc.)
- CSV/Excel bulk import with validation
- QR code member cards and NFC check-in

### 💳 Memberships & Billing
- Flexible billing cycles (weekly to lifetime)
- Auto-renewal with dunning (failed payment retries)
- Discounts: EarlyBird, Loyalty, Family, PromoCode, Referral
- Membership freeze/pause with optional fees
- Capacity-based waitlists

### 📅 Sessions & Bookings
- Recurring schedule templates with auto-generation
- Real-time capacity management and waitlists
- Cancellation deadline enforcement
- Attendance tracking (Confirmed, Attended, NoShow)
- Waitlist auto-promotion with 24-hour acceptance window

### 🎉 Events
- Ticketed events with QR code check-in
- RSVP events (Attending/NotAttending/Maybe)
- Event series and multi-session events
- Full lifecycle management (Draft → Published → Completed)
- Automatic refunds on cancellation

### 🏆 Competitions & Live Scoring
- League, Tournament, Cup, Knockout, Round-Robin formats
- Automatic fixture generation
- Real-time match scoring via SignalR (live to spectators)
- Auto-calculated standings (points, goal difference, form)
- Sport-specific scoring (cricket overs, football goals, tennis sets)
- Public scoreboard URLs — no login required

### 💰 Payments & Finance
- Stripe Connect, PayPal, GoCardless Direct Debit, Cash, Cheque
- Invoice generation with lifecycle tracking
- Payment plans with installments
- Member balance ledger (credits, debits, outstanding)
- Double-entry bookkeeping (chart of accounts, journal entries)
- Platform transaction fee collection (1-2%)

### 🏟️ Facilities & Equipment
- Facility booking with conflict detection
- Peak/off-peak pricing (member vs non-member rates)
- Equipment inventory with loan management
- Maintenance scheduling with auto-blocking
- Reservation system with advance booking limits

### 📚 Programs & Coaching
- Courses, camps, academies, private lessons
- Enrollment with capacity and waitlists
- Attendance tracking and completion rates
- Certificate issuance (≥80% attendance)
- Skill level progression

### 📊 Analytics & Retention
- Club health score (0-100)
- Churn prediction (attendance drops, missed payments)
- Member engagement metrics
- Revenue forecasting (next 3 months)
- Platform benchmarking (your club vs average)

### 🔔 Real-Time Notifications
- In-app (SignalR WebSocket) — delivered within 2 seconds
- Email (SendGrid), SMS (Twilio), Push (PWA)
- Per-member channel preferences
- Webhook integrations for external tools

### 🛒 Club Shop
- Product listings with variants (size, colour)
- Stock management with restock notifications
- Order lifecycle (Pending → Confirmed → Dispatched → Delivered)
- Integrated payment processing

### 🔗 Integrations
- GoCardless Direct Debit
- Xero / QuickBooks accounting sync
- Google Calendar / Outlook sync
- Facebook & X (Twitter) auto-posting
- Public REST API with API key auth

---

## 🏗️ Architecture

**Modular Monolith** with Clean Architecture per module — designed for team scalability without microservice complexity.

```
┌─────────────────────────────────────────────────────────┐
│                    Angular 20 SPA / PWA                  │
│              Tailwind v4 + DaisyUI + SSR                │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│              ASP.NET Core Host (API Gateway)             │
│     JWT Auth │ CORS │ Rate Limiting │ Health Checks     │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                  MediatR CQRS Pipeline                  │
│   Logging → Validation → Tenant → Performance → Tx     │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                   16 Domain Modules                      │
│  Identity │ Clubs │ Members │ Memberships │ Sessions    │
│  Events │ Competitions │ Payments │ Facilities          │
│  Equipment │ Programs │ Communications │ Analytics      │
│  Shop │ Documents │ Subscriptions                       │
└───────┬─────────────┬───────────────────┬───────────────┘
        │             │                   │
   ┌────▼────┐   ┌───▼────┐   ┌─────────▼──────────┐
   │SQL Server│   │ Redis  │   │Azure Blob Storage  │
   │(shared DB│   │(cache +│   │(files, documents)  │
   │per-module│   │SignalR  │   │                    │
   │DbContext)│   │backplane│   │                    │
   └──────────┘   └────────┘   └────────────────────┘
```

### Design Principles

| Principle | Implementation |
|-----------|---------------|
| **CQRS** | Commands and queries separated via MediatR handlers |
| **Module Boundaries** | Each module owns its DbContext, communicates via integration events |
| **Multi-Tenancy** | Shared DB with ClubId discriminator + EF Core global query filters |
| **Clean Architecture** | Domain → Application → Infrastructure → API (per module) |
| **Event-Driven** | In-process integration event bus with retry and dead-letter |
| **Resilience** | Circuit breakers (Polly), Redis fallback, exponential backoff |

---

## 🛠️ Tech Stack

### Backend
| Component | Technology |
|-----------|-----------|
| Runtime | .NET 10 |
| Framework | ASP.NET Core 10 |
| Language | C# 13 |
| ORM | Entity Framework Core 10 |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Database | SQL Server 2022 |
| Cache | Redis (StackExchange.Redis) |
| Background Jobs | Hangfire |
| Real-Time | SignalR |
| File Storage | Azure Blob Storage |
| Auth | ASP.NET Core Identity + JWT |
| Resilience | Polly 8 |
| Logging | Serilog |
| API Docs | Swashbuckle (OpenAPI 3.0) |
| Testing | xUnit + FsCheck (property-based) + NetArchTest |

### Frontend
| Component | Technology |
|-----------|-----------|
| Framework | Angular 20 |
| Language | TypeScript |
| CSS | Tailwind CSS v4 |
| UI Components | DaisyUI 5 |
| State | Angular Signals |
| Charts | Chart.js + ng2-charts |
| E2E Testing | Playwright |
| PWA | Service Worker + IndexedDB |
| SSR | Angular SSR (public pages) |

---

## 📁 Solution Structure

```
TheLeague/
├── src/
│   ├── TheLeague.Host/                    # API entry point
│   ├── TheLeague.Shared/
│   │   ├── TheLeague.Shared.Domain/       # Entities, Value Objects, Enums
│   │   ├── TheLeague.Shared.Contracts/    # Module interfaces, Integration Events
│   │   └── TheLeague.Shared.Infrastructure/ # Tenancy, Caching, Messaging, Auth
│   └── Modules/
│       ├── TheLeague.Modules.Identity/    # Auth, JWT, Sessions, 2FA
│       ├── TheLeague.Modules.Clubs/       # Club management, settings
│       ├── TheLeague.Modules.Members/     # Member CRUD, family, custom fields
│       ├── TheLeague.Modules.Memberships/ # Types, billing, discounts, waitlists
│       ├── TheLeague.Modules.Sessions/    # Scheduling, bookings, attendance
│       ├── TheLeague.Modules.Events/      # Events, tickets, RSVP
│       ├── TheLeague.Modules.Competitions/# Seasons, fixtures, standings, live scoring
│       ├── TheLeague.Modules.Payments/    # Stripe, invoices, refunds, accounting
│       ├── TheLeague.Modules.Facilities/  # Venue booking, maintenance
│       ├── TheLeague.Modules.Equipment/   # Inventory, loans, reservations
│       ├── TheLeague.Modules.Programs/    # Courses, enrollment, certificates
│       ├── TheLeague.Modules.Communications/ # Email, SMS, campaigns
│       ├── TheLeague.Modules.Analytics/   # Health scores, churn, forecasting
│       ├── TheLeague.Modules.Shop/        # Merchandise, orders
│       ├── TheLeague.Modules.Documents/   # File storage, malware scanning
│       └── TheLeague.Modules.Subscriptions/ # Tiers, billing, feature gates
├── tests/
│   ├── TheLeague.Tests.Unit/
│   ├── TheLeague.Tests.Properties/        # FsCheck property-based tests
│   ├── TheLeague.Tests.Integration/
│   └── TheLeague.Tests.Architecture/      # NetArchTest boundary enforcement
├── docker-compose.yml
└── .github/workflows/                     # CI/CD pipelines
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- SQL Server (LocalDB for development)
- Redis (optional — falls back to in-memory cache)

### Backend
```bash
dotnet restore
dotnet build
cd src/TheLeague.Host
dotnet run    # → http://localhost:7000
```

### Frontend (coming in Phase 4)
```bash
cd src/the-league-client
npm install
npm start     # → http://localhost:4200
```

### Swagger
Open `http://localhost:7000/swagger` for interactive API documentation.

---

## 🧪 Testing Strategy

| Type | Tool | Purpose |
|------|------|---------|
| Unit Tests | xUnit | Handler logic, domain rules |
| Property Tests | FsCheck | Universal correctness properties (35 defined) |
| Architecture Tests | NetArchTest | Module boundaries, dependency rules |
| Integration Tests | xUnit + TestContainers | Cross-module flows, DB operations |
| E2E Tests | Playwright | Critical user journeys |

### Correctness Properties (examples)
- Tenant data isolation: queries never return cross-tenant data
- Session capacity invariant: confirmed bookings never exceed capacity
- League standings: points always equal 3×wins + 1×draws
- Invoice lifecycle: only valid state transitions permitted
- Member balance: always equals sum(credits) - sum(debits)

---

## 📋 Implementation Progress

- [x] **Phase 1** — Foundation & Infrastructure (solution, shared kernel, auth, RBAC, middleware)
- [ ] **Phase 2** — Core Domain (Clubs, Subscriptions, Members, Memberships)
- [ ] **Phase 3** — Core Domain (Sessions, Events, Competitions, Payments)
- [ ] **Phase 4** — Supporting Modules (Facilities, Equipment, Programs, Comms, Analytics, Shop)
- [ ] **Phase 5** — Frontend Foundation (Angular 20, component library, layouts, theming)
- [ ] **Phase 6** — Frontend Features (Admin, Club Manager, Member portals)
- [ ] **Phase 7** — Production Features (PWA, SSR, Live Scoring, Integrations, CI/CD)

---

## 🏛️ Multi-Tenancy

Every club operates in complete data isolation within a shared database:

1. User authenticates → JWT contains `clubId` claim
2. Tenant middleware extracts ClubId from JWT
3. EF Core global query filters enforce `WHERE ClubId = @TenantId` on every query
4. SuperAdmin bypasses filters for platform-wide operations
5. No club ever sees another club's data

---

## 🔐 Security

- JWT access tokens (15-min expiry) + refresh tokens (7-day, one-time use)
- Account lockout (5 failed attempts → 15-min lock)
- 2FA (TOTP) for SuperAdmin and ClubManager roles
- API rate limiting (100 req/min authenticated, 20 req/min public)
- Security headers (CSP, HSTS, X-Frame-Options, etc.)
- GDPR compliance (data export, erasure, retention policies)
- Immutable audit trail on every data mutation

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

---

## 🤝 Contributing

This is currently a private project. Contribution guidelines will be added when the repository goes public.

---

<p align="center">
  <strong>Built with ❤️ for sports clubs everywhere.</strong>
</p>
