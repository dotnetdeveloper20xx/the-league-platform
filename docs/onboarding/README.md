# 📚 The League Platform — Developer Onboarding Guides

Welcome to The League Platform developer documentation. These guides are designed to take a new developer from zero to productive on any feature of the platform.

## How to Use These Guides

Each document is a **self-contained mentor guide** for one feature. Read them in order if you're new to the project, or jump to a specific feature if you're working on it.

## Document Index

### Foundation Features (Start Here)
| # | Feature | File | Difficulty |
|---|---------|------|-----------|
| 1 | [Multi-Tenancy & Data Isolation](./01-multi-tenancy.md) | `01-multi-tenancy.md` | ⭐⭐ |
| 2 | [Authentication & JWT Tokens](./02-authentication.md) | `02-authentication.md` | ⭐⭐⭐ |
| 3 | [Role-Based Access Control](./03-rbac.md) | `03-rbac.md` | ⭐⭐ |
| 4 | [CQRS with MediatR Pipeline](./04-cqrs-mediatr.md) | `04-cqrs-mediatr.md` | ⭐⭐⭐ |
| 5 | [Modular Monolith Architecture](./05-modular-monolith.md) | `05-modular-monolith.md` | ⭐⭐⭐⭐ |

### Core Domain Features
| # | Feature | File | Difficulty |
|---|---------|------|-----------|
| 6 | [Member Management](./06-member-management.md) | `06-member-management.md` | ⭐⭐ |
| 7 | [Membership & Billing](./07-membership-billing.md) | `07-membership-billing.md` | ⭐⭐⭐ |
| 8 | [Session & Booking Management](./08-session-booking.md) | `08-session-booking.md` | ⭐⭐⭐ |
| 9 | [Event Management](./09-event-management.md) | `09-event-management.md` | ⭐⭐⭐ |
| 10 | [Competition & Tournament Management](./10-competitions.md) | `10-competitions.md` | ⭐⭐⭐⭐ |

### Financial Features
| # | Feature | File | Difficulty |
|---|---------|------|-----------|
| 11 | [Payment Processing & Stripe Connect](./11-payments.md) | `11-payments.md` | ⭐⭐⭐⭐ |
| 12 | [Double-Entry Bookkeeping](./12-bookkeeping.md) | `12-bookkeeping.md` | ⭐⭐⭐⭐⭐ |

### Supporting Features
| # | Feature | File | Difficulty |
|---|---------|------|-----------|
| 13 | [Facility Booking & Conflict Detection](./13-facilities.md) | `13-facilities.md` | ⭐⭐⭐ |
| 14 | [Equipment & Loan Management](./14-equipment.md) | `14-equipment.md` | ⭐⭐ |
| 15 | [Programs & Certificates](./15-programs.md) | `15-programs.md` | ⭐⭐ |
| 16 | [Communications & Campaigns](./16-communications.md) | `16-communications.md` | ⭐⭐⭐ |

### Platform & Intelligence
| # | Feature | File | Difficulty |
|---|---------|------|-----------|
| 17 | [Subscription Tiers & Feature Gating](./17-subscriptions.md) | `17-subscriptions.md` | ⭐⭐⭐ |
| 18 | [Analytics & Churn Prediction](./18-analytics.md) | `18-analytics.md` | ⭐⭐⭐⭐ |
| 19 | [Real-Time Notifications & SignalR](./19-signalr-notifications.md) | `19-signalr-notifications.md` | ⭐⭐⭐ |
| 20 | [Database Seeding & Demo Data](./20-database-seeding.md) | `20-database-seeding.md` | ⭐⭐ |

## Prerequisites

Before diving in, ensure you understand:
- C# 13 / .NET 10 fundamentals
- Entity Framework Core basics
- Angular 21 with Signals
- RESTful API design
- SQL Server basics

## Architecture Overview

```
TheLeague.Host (API Entry Point)
  → Shared.Infrastructure (Cross-cutting: Auth, Cache, Events, Middleware)
  → Shared.Contracts (Module interfaces, Integration Events)
  → Shared.Domain (Base entities, Value Objects, Enums)
  → Modules.* (16 independent domain modules)
```

Each module follows Clean Architecture: **Domain → Application → Infrastructure → API**
