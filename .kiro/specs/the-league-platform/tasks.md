# Implementation Plan: The League Platform

## Overview

Full production-grade rebuild of The League — a multi-tenant SaaS platform for sports club management. Built as a Modular Monolith with Clean Architecture per module, CQRS via MediatR, Angular 20 frontend with DaisyUI, and comprehensive testing. Implementation is organized in 7 phases progressing from infrastructure through to production features.

## Tasks

- [x] 1. Foundation and Infrastructure

  - [x] 1.1 Create solution structure and project scaffolding
    - Create `TheLeague.sln` with folder structure: `src/`, `tests/`
    - Create projects: `TheLeague.Host`, `TheLeague.Shared.Contracts`, `TheLeague.Shared.Domain`, `TheLeague.Shared.Infrastructure`
    - Create module project stubs for all 16 modules under `src/Modules/`
    - Create test projects: `TheLeague.Tests.Unit`, `TheLeague.Tests.Properties`, `TheLeague.Tests.Integration`, `TheLeague.Tests.Architecture`
    - Add NuGet references: MediatR, FluentValidation, EF Core, Serilog, Polly, Hangfire, StackExchange.Redis, FsCheck.xUnit, NetArchTest
    - Configure `Directory.Build.props` for shared package versions
    - _Requirements: 24.1, 24.3, 24.4_

  - [x] 1.2 Implement Shared Domain layer (base entities, value objects, enums)
    - Create `BaseEntity` (Id, CreatedAt, UpdatedAt), `TenantEntity` (ClubId), `AuditableEntity`
    - Create value objects: `Address`, `EmergencyContact`, `MedicalInfo`, `Money`
    - Create shared enums: `ClubType`, `MemberStatus`, `PaymentMethod`, `PaymentStatus`, `Gender`
    - Create `Result<T>` and `PagedResult<T>` response wrappers
    - _Requirements: 1.1, 6.3, 24.1_

  - [x] 1.3 Implement Shared Contracts (module interfaces, integration events)
    - Create `IModule` interface with `RegisterModule` and `UseModule` methods
    - Create `IntegrationEvent` base record with Id, OccurredAt, TenantId
    - Create all integration events: `MemberCreatedEvent`, `PaymentCompletedEvent`, `MembershipExpiredEvent`, `BookingConfirmedEvent`, `EventPublishedEvent`, `MatchCompletedEvent`, `SubscriptionChangedEvent`, etc.
    - Create `IIntegrationEventBus` interface
    - _Requirements: 24.3, 24.5_

  - [x] 1.4 Implement Shared Infrastructure (multi-tenancy, caching, messaging)
    - Implement `ITenantService` with scoped ClubId resolution from JWT claims
    - Implement in-process `IntegrationEventBus` using `Channel<T>` with retry (3x exponential backoff) and dead-letter
    - Implement Redis caching service with `IMemoryCache` fallback when Redis unavailable
    - Implement `ICacheService` interface with configurable TTL (default 5 min)
    - _Requirements: 1.2, 1.3, 18.4, 18.7, 35.4_

  - [ ]* 1.5 Write property tests for tenant isolation (Properties 1-2)
    - **Property 1: Tenant Data Isolation** — verify all query results contain only entities matching the authenticated ClubId
    - **Property 2: ClubId Extraction and Validation** — verify JWT ClubId parsing and 403 on missing/malformed
    - **Validates: Requirements 1.2, 1.3, 1.4, 1.6**

  - [x] 1.6 Implement ASP.NET Core Host with middleware pipeline
    - Configure `Program.cs` with module registration via assembly scanning
    - Add middleware pipeline: ExceptionHandling, Correlation ID, Authentication, Tenant Resolution, Rate Limiting, CORS
    - Configure Serilog structured logging with correlation ID enrichment
    - Configure health checks (SQL Server, Redis, Blob Storage, Hangfire)
    - Configure Swagger/OpenAPI documentation
    - Add security headers middleware (CSP, X-Frame-Options, HSTS, etc.)
    - _Requirements: 16.9, 16.10, 16.11, 18.2, 18.3, 26.6, 35.1, 35.7_

  - [x] 1.7 Implement global exception handling and error responses
    - Create exception types: `ValidationException`, `NotFoundException`, `ForbiddenException`, `ConflictException`, `RateLimitException`
    - Implement `ExceptionHandlingMiddleware` mapping exceptions to RFC 7807 Problem Details
    - Implement rate limiting: 100 req/min authenticated, 20 req/min unauthenticated
    - _Requirements: 16.6, 26.4, 35.1, 35.5, 35.6, 35.7_

  - [x] 1.8 Implement MediatR pipeline behaviours
    - Create `LoggingBehaviour` with correlation ID
    - Create `ValidationBehaviour` with FluentValidation integration
    - Create `TenantBehaviour` for tenant context injection/validation
    - Create `PerformanceBehaviour` logging slow requests (>500ms)
    - Create `TransactionBehaviour` wrapping commands in DB transactions
    - _Requirements: 24.2, 18.3_

  - [x] 1.9 Implement Identity Module (authentication, JWT, sessions)
    - Create `IdentityDbContext` with `ApplicationUser`, `Role`, `RefreshToken`, `UserSession` entities
    - Implement `LoginCommand` issuing JWT (15-min) + refresh token (7-day) with claims: sub, email, name, role, clubId, memberId, jti
    - Implement `RegisterCommand` with email verification (24-hour link)
    - Implement `RefreshTokenCommand` with token rotation (invalidate consumed token)
    - Implement account lockout (5 failed attempts → 15-min lock)
    - Implement password reset (60-min single-use token)
    - Implement session management (view/revoke sessions with device, IP, last active)
    - Implement 2FA (TOTP) for SuperAdmin and ClubManager roles
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10, 2.11, 2.12, 16.4_

  - [ ]* 1.10 Write property tests for authentication (Properties 3-5)
    - **Property 3: JWT Claim Completeness** — verify all required claims present with non-null values
    - **Property 4: Credential Error Message Opacity** — verify identical error regardless of which credential failed
    - **Property 5: Refresh Token Rotation** — verify consumed token invalidated, new tokens issued
    - **Validates: Requirements 2.2, 2.3, 2.5**

  - [x] 1.11 Implement Role-Based Access Control
    - Define 5 roles: SuperAdmin, ClubManager, Member, Coach, Staff
    - Implement permission matrix with attribute-based authorization
    - Implement `[RequireRole]` and `[RequirePermission]` attributes
    - Implement SuperAdmin tenant filter bypass for cross-tenant queries
    - Enforce role change takes effect on next request
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 1.7_

  - [ ]* 1.12 Write property test for authorization (Property 6)
    - **Property 6: Role-Permission Matrix Enforcement** — verify access decisions match defined permission matrix for all role/endpoint combinations
    - **Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7**

- [ ] 2. Checkpoint - Foundation complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Core Domain Modules — Clubs, Members, Memberships

  - [x] 3.1 Implement Clubs Module
    - Create `ClubsDbContext` with `Club`, `ClubSettings`, `SportConfiguration` entities
    - Implement `CreateClubCommand` generating unique ClubId and slug
    - Implement `UpdateClubCommand` for profile, branding (colors, logo), contact info
    - Implement sport-specific defaults pre-population on club creation
    - Implement custom terminology storage and retrieval per club
    - Implement club settings CRUD (payment provider config, notification settings, locale)
    - _Requirements: 1.5, 5.5, 28.1, 28.2, 28.3, 28.4, 28.5, 28.6_

  - [x] 3.2 Implement Subscriptions Module
    - Create `SubscriptionsDbContext` with `SubscriptionTier`, `ClubSubscription`, `Trial`, `UsageRecord`, `AddOn` entities
    - Implement 4 tiers: Free (£0), Starter (£29), Pro (£79), Enterprise (£199) with feature gates
    - Implement 14-day free trial with Pro-level access
    - Implement upgrade with proration: (PB - PA) × (L - D) / L
    - Implement downgrade scheduling at end of billing period
    - Implement usage limit enforcement (members, storage, SMS credits per tier)
    - Implement dunning: retry at days 1, 3, 7 → downgrade to Free on final failure
    - Implement trial expiry with conversion reminders at days 14, 21, 28
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9, 4.10, 4.11_

  - [ ]* 3.3 Write property tests for subscriptions (Properties 7-8)
    - **Property 7: Subscription Proration Calculation** — verify prorated charge = (PB - PA) × (L - D) / L rounded to 2dp
    - **Property 8: Usage Limit Enforcement** — verify actions blocked when tier limits exceeded
    - **Validates: Requirements 4.5, 4.7, 4.8**

  - [x] 3.4 Implement Members Module
    - Create `MembersDbContext` with `Member`, `FamilyMember`, `CustomFieldDefinition`, `MemberNote`, `MemberDocument` entities
    - Implement CRUD with search (name/email), filter (status), pagination (default 20, max 100)
    - Implement auto-generated member number: "MBR-{N}" zero-padded (min 3 digits), unique per club
    - Implement member status state machine: Pending→Active, Active→Expired/Suspended/Cancelled, Suspended→Active, Expired→Active
    - Implement status transition audit (previous, new, timestamp, actor)
    - Implement family accounts (up to 10 dependents, relationship types)
    - Implement custom fields (JSON storage, type validation: Text, Number, Date, Boolean, Select, MultiSelect, TextArea)
    - Implement CSV/Excel import (max 5MB, 2000 rows, validate required fields, report errors per row)
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7, 6.8, 6.9, 5.6, 5.7, 5.8_

  - [ ]* 3.5 Write property tests for members (Properties 9-12)
    - **Property 9: CSV Import Correctness** — verify valid rows imported, invalid rejected, counts sum to total
    - **Property 10: Member Number Uniqueness and Format** — verify unique per club, matches "MBR-{N}" format
    - **Property 11: Member Status State Machine** — verify only permitted transitions succeed
    - **Property 12: Custom Field Type Validation** — verify type-conforming values accepted, non-conforming rejected
    - **Validates: Requirements 5.6, 5.7, 6.2, 6.5, 6.6, 6.8, 6.9**

  - [x] 3.6 Implement Memberships Module
    - Create `MembershipsDbContext` with `MembershipType`, `Membership`, `MembershipDiscount`, `MembershipFreeze`, `MembershipWaitlist`, `GuestPass` entities
    - Implement membership type CRUD: name, description, pricing, billing cycle, age limits, capacity
    - Implement billing cycles: Weekly, Fortnightly, Monthly, Quarterly, Biannual, Annual, Lifetime, OneTime, PayAsYouGo
    - Implement enrolment with age validation (min/max inclusive range)
    - Implement auto-renewal with payment trigger and PendingPayment on failure
    - Implement membership freeze (1-365 days, configurable fee)
    - Implement discounts: EarlyBird, Loyalty, Family, Corporate, PromoCode, Referral (percentage or fixed)
    - Implement capacity-based waitlist (position-ordered, email notification on slot available)
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8, 7.9_

  - [ ]* 3.7 Write property tests for memberships (Properties 13-14)
    - **Property 13: Membership Age Limit Enforcement** — verify enrolment accepted iff age within [min, max]
    - **Property 14: Discount Calculation Correctness** — verify percentage and fixed discounts calculated correctly, never below zero, 2dp
    - **Validates: Requirements 7.3, 7.4, 7.8**

- [x] 4. Core Domain Modules — Sessions, Events, Competitions

  - [x] 4.1 Implement Sessions Module
    - Create `SessionsDbContext` with `Session`, `RecurringSchedule`, `SessionBooking`, `RecurringBooking`, `Waitlist`, `Attendance` entities
    - Implement session CRUD: title, category, venue, date/time, duration (15-480 min), capacity (1-500), fee
    - Implement recurring schedule templates (day-of-week patterns, up to 12 weeks horizon)
    - Implement booking with capacity check (confirm if available, waitlist if full, max 50 waitlist)
    - Implement waitlist promotion: auto-offer to next member, 24-hour acceptance window
    - Implement cancellation deadline enforcement (configurable, default 24h before start)
    - Implement attendance tracking: Confirmed, Attended, NoShow, Cancelled
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8_

  - [ ]* 4.2 Write property tests for sessions (Properties 15-17)
    - **Property 15: Session Booking Capacity Invariant** — verify confirmed bookings never exceed capacity N
    - **Property 16: Waitlist Ordering Preservation** — verify FIFO ordering, earliest member offered first
    - **Property 17: Cancellation Deadline Enforcement** — verify accepted iff request time ≤ (T - D hours)
    - **Validates: Requirements 8.3, 8.4, 8.5, 8.6**

  - [x] 4.3 Implement Events Module
    - Create `EventsDbContext` with `Event`, `EventSeries`, `EventSession`, `EventTicket`, `EventRSVP`, `EventRegistration` entities
    - Implement event CRUD: title (1-200 chars), type, start/end datetime, capacity
    - Implement event types: Social, Tournament, AGM, Training, Fundraiser, Competition, Meeting, Presentation, Other
    - Implement ticketed events (standard/member pricing) and RSVP events (Attending/NotAttending/Maybe)
    - Implement QR code generation for tickets
    - Implement event series (max 52 occurrences) and multi-session events (max 20 sessions)
    - Implement event lifecycle state machine: Draft→Published→RegistrationOpen→RegistrationClosed→InProgress→Completed, any→Cancelled/Postponed
    - Implement cancellation with attendee notification (within 60s) and refund initiation
    - Implement registration cancellation (>48h: refund, ≤48h: reject)
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7, 9.8, 9.9_

  - [ ]* 4.4 Write property test for events (Property 18)
    - **Property 18: Event Lifecycle State Machine** — verify only valid transitions succeed per defined paths
    - **Validates: Requirements 9.7**

  - [x] 4.5 Implement Competitions Module
    - Create `CompetitionsDbContext` with `Season`, `Competition`, `CompetitionTeam`, `CompetitionParticipant`, `Match`, `MatchEvent`, `MatchLineup`, `CompetitionStanding` entities
    - Implement competition types: League, Tournament, Cup, Friendly, RoundRobin, Knockout, Championship
    - Implement season management with start/end dates
    - Implement team registration: squad (11-30 players), captain, team name, home venue
    - Implement round-robin fixture generation: N×(N-1)/2 matches, each team plays every other
    - Implement knockout bracket generation with byes for non-power-of-2 team counts
    - Implement standings calculation: 3pts win, 1pt draw, 0pts loss, goal difference, form (last 5)
    - Implement walkover handling (3pts to non-forfeiting team, default score)
    - Implement match events: goals, cards (yellow/red), substitutions (max 5/team), lineups (exactly 11 starters)
    - Implement match status state machine: Scheduled→Confirmed→InProgress→Completed/Abandoned, Confirmed→Postponed→Confirmed/Cancelled
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 10.8, 10.9_

  - [ ]* 4.6 Write property tests for competitions (Properties 19-22)
    - **Property 19: Round-Robin Fixture Completeness** — verify N×(N-1)/2 matches, each team plays every other exactly once
    - **Property 20: League Standings Calculation** — verify points = 3×wins + 1×draws, goal difference correct
    - **Property 21: Match Event Constraints** — verify max 5 subs, exactly 11 starters, valid player references
    - **Property 22: Match Status State Machine** — verify only valid transitions per defined set
    - **Validates: Requirements 10.4, 10.6, 10.8, 10.9**

- [x] 5. Core Domain Modules — Payments

  - [x] 5.1 Implement Payments Module
    - Create `PaymentsDbContext` with `Payment`, `Invoice`, `InvoiceLineItem`, `PaymentPlan`, `PaymentInstallment`, `MemberBalance`, `BalanceTransaction`, `Refund`, `Fee`, `ChartOfAccount`, `JournalEntry` entities
    - Implement payment processing: Stripe, PayPal, GoCardless, BankTransfer, Cash, Cheque
    - Implement Stripe Connect with configurable platform fee (1-2%, min £0.30)
    - Implement payment failure handling (status=Failed, record reason)
    - Implement invoice lifecycle: Draft→Sent→Viewed→PartiallyPaid→Paid, Sent→Overdue, any→Voided
    - Implement invoice generation (1-50 line items) with auto-overdue transition and reminders (1-90 day intervals, max 10)
    - Implement payment plans with installment tracking (Pending, Processing, Completed, Failed, Refunded, Cancelled)
    - Implement member balance ledger (credits, debits, DECIMAL(18,2) precision)
    - Implement full/partial refunds with balance adjustment
    - Implement double-entry bookkeeping: chart of accounts, journal entries (balanced debits = credits), fiscal years, tax rates
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7, 11.8, 11.9, 11.10, 17.1, 17.2_

  - [ ]* 5.2 Write property tests for payments (Properties 23-25)
    - **Property 23: Invoice Lifecycle State Machine** — verify only valid transitions per defined paths
    - **Property 24: Member Balance Ledger Invariant** — verify balance = sum(credits) - sum(debits) at DECIMAL(18,2)
    - **Property 25: Double-Entry Bookkeeping Balance** — verify sum(debits) = sum(credits) for every journal entry
    - **Validates: Requirements 11.4, 11.7, 11.10**

- [ ] 6. Checkpoint - Core domain modules complete
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 7. Supporting Modules — Facilities, Equipment, Programs

  - [ ] 7.1 Implement Facilities Module
    - Create `FacilitiesDbContext` with `Facility`, `FacilityBooking`, `FacilityAvailability`, `FacilityPricing`, `FacilityMaintenance`, `FacilityBlockout`, `VenueOperatingSchedule` entities
    - Implement facility types: Court, Pool, Field, Track, Gym, Studio, MeetingRoom, ChangingRoom, ClubHouse
    - Implement operating schedules, holidays, and blockout periods
    - Implement booking with conflict detection (no overlap with bookings, maintenance, blockouts)
    - Implement duration validation (30 min - 4 hours, 30-min increments)
    - Implement peak/off-peak pricing with member/non-member rates
    - Implement maintenance scheduling with auto-cancel of conflicting bookings + notification
    - Implement advance booking limit (max 30 days)
    - Implement alternative slot suggestion (up to 3 same-day alternatives on conflict)
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6, 12.7, 12.8_

  - [ ]* 7.2 Write property test for facilities (Property 26)
    - **Property 26: Facility Booking Conflict Detection** — verify no two confirmed bookings overlap for same facility
    - **Validates: Requirements 12.3, 12.6**

  - [ ] 7.3 Implement Equipment Module
    - Create `EquipmentDbContext` with `Equipment`, `EquipmentLoan`, `EquipmentReservation`, `EquipmentMaintenance` entities
    - Implement inventory CRUD: name, category, condition, location, purchase date, value, depreciation rate
    - Implement loan management with condition check (reject if NeedsRepair/Damaged/Decommissioned)
    - Implement overdue loan detection (auto-update status, notify member, flag for review)
    - Implement reservations (up to 365 days advance, conflict detection with loans/reservations)
    - Implement maintenance history tracking with condition transitions
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.6_

  - [ ]* 7.4 Write property test for equipment (Property 27)
    - **Property 27: Equipment Loan Availability** — verify loan approved iff condition loanable AND no overlapping loan/reservation
    - **Validates: Requirements 13.2, 13.3**

  - [ ] 7.5 Implement Programs Module
    - Create `ProgramsDbContext` with `Program`, `ProgramSession`, `ProgramEnrollment`, `ProgramAttendance`, `MemberCertificate` entities
    - Implement program types: Course, Camp, Class, Workshop, Clinic, Academy, Squad, PrivateLesson, GroupLesson
    - Implement skill levels: Beginner, Elementary, Intermediate, UpperIntermediate, Advanced, Expert
    - Implement program session CRUD (instructor, datetime, venue, capacity 1-200)
    - Implement enrolment with capacity check and waitlist (max 50)
    - Implement attendance tracking and completion rate calculation
    - Implement certificate issuance (≥80% attendance AND program end date passed)
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5, 14.6, 14.7, 14.8_

  - [ ]* 7.6 Write property tests for programs (Properties 28-29)
    - **Property 28: Program Enrolment Capacity** — verify enrolments never exceed capacity, waitlist up to 50, then reject
    - **Property 29: Certificate Issuance Threshold** — verify certificate iff attendance ≥80% AND program ended
    - **Validates: Requirements 14.3, 14.4, 14.5, 14.7**

- [ ] 8. Supporting Modules — Communications, Analytics, Shop, Documents

  - [ ] 8.1 Implement Communications Module
    - Create `CommunicationsDbContext` with `CommunicationTemplate`, `EmailLog`, `BulkEmailCampaign`, `SmsLog` entities
    - Implement email templates per club: Welcome, PasswordReset, PaymentReminder, BookingConfirmation, EventNotification, MembershipRenewal
    - Implement placeholder variable substitution (first name, last name, email, member number, club name)
    - Implement bulk email campaigns (max 5000 recipients, respect opt-out preferences)
    - Implement SMS via Twilio for time-sensitive events (within 5 min)
    - Implement delivery logging: Sent, Delivered, Bounced, Failed
    - Implement retry (3x exponential backoff) on provider failure
    - Implement opt-in/opt-out enforcement (suppress non-transactional for opted-out, always deliver transactional)
    - _Requirements: 21.1, 21.2, 21.3, 21.4, 21.5, 21.6, 21.7_

  - [ ] 8.2 Implement Notifications Service (SignalR + multi-channel)
    - Implement `NotificationHub` with club-group and user-group membership
    - Implement in-app notifications via SignalR (within 2 seconds)
    - Implement push notifications (PWA)
    - Implement per-member channel preferences (enable/disable per notification type)
    - Implement webhook integrations (max 10 endpoints per club)
    - Implement retry (3x exponential backoff: 5s, 25s, 125s) on delivery failure
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5, 15.6, 15.7_

  - [ ] 8.3 Implement Analytics Module
    - Create `AnalyticsDbContext` with `ClubAnalyticsSnapshot`, `MemberEngagement`, `ChurnPrediction` entities
    - Implement club health score: weighted average (25% each: member growth, payment collection, session attendance, event participation)
    - Implement churn prediction: flag at-risk if attendance drops ≥50%, ≥2 missed payments, or login drops ≥50% (90-day window)
    - Implement member engagement tracking (sessions/month, events/month, payment timeliness, logins/month)
    - Implement revenue forecasting (next 3 months based on active memberships + historical data)
    - Implement platform benchmarking (anonymised averages across all clubs)
    - Implement weekly snapshot capture (retain 24 months)
    - Handle insufficient data (<30 days: show notification, omit predictions)
    - Exclude members with no bookings/registrations from churn prediction
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6, 19.7, 19.8_

  - [ ]* 8.4 Write property tests for analytics (Properties 30-31)
    - **Property 30: Club Health Score Calculation** — verify weighted average produces value 0-100
    - **Property 31: Churn Prediction Logic** — verify at-risk flag iff criteria met, exclude inactive members
    - **Validates: Requirements 19.1, 19.2, 19.8**

  - [ ] 8.5 Implement Shop Module
    - Create `ShopDbContext` with `Product`, `ProductVariant`, `ProductCategory`, `Order`, `OrderItem` entities
    - Implement product CRUD: name (150 chars), description (2000 chars), images (max 8), price, variants (size/colour), stock
    - Implement categories (max 50 per club) with paginated catalogue
    - Implement purchase flow: payment → decrement stock → create order (Pending)
    - Implement stock management: zero-stock prevention, out-of-stock marking, restock notifications
    - Implement order status state machine: Pending→Confirmed→Dispatched→Delivered, {Pending,Confirmed,Dispatched}→Refunded
    - Implement order confirmation email on Confirmed status
    - _Requirements: 38.1, 38.2, 38.3, 38.4, 38.5, 38.6, 38.7, 38.8, 38.9_

  - [ ]* 8.6 Write property tests for shop (Properties 34-35)
    - **Property 34: Stock Decrement Invariant** — verify stock = Q-1 after purchase, reject if Q=0
    - **Property 35: Order Status State Machine** — verify only valid transitions per defined paths
    - **Validates: Requirements 38.2, 38.4, 38.8**

  - [ ] 8.7 Implement Documents Module
    - Create `DocumentsDbContext` with `Document`, `DocumentMetadata` entities
    - Implement file upload with type validation (jpg/png/webp, pdf/docx, csv/xlsx) and size limit (10MB)
    - Implement Azure Blob Storage integration with container structure per club
    - Implement secure download URLs (SAS tokens, 1-hour expiry)
    - Implement storage quota enforcement per subscription tier
    - Implement malware scanning (reject + discard on threat detection)
    - Implement image optimization: profile photos (300×300), thumbnails (150×150), 80% quality
    - Implement access control (ClubManager, subject Member, SuperAdmin only)
    - _Requirements: 32.1, 32.2, 32.3, 32.4, 32.5, 32.6, 32.7, 32.8, 32.9_

- [ ] 9. Supporting Modules — Audit, Reporting, Integrations, Onboarding

  - [ ] 9.1 Implement Audit and Compliance Service
    - Implement immutable audit log: entity type, entity ID, action, actor, timestamp, before/after (max 64KB)
    - Implement GDPR data export (all member PII, delivered within 72 hours)
    - Implement GDPR data erasure (anonymise PII within 30 days, preserve financial aggregates 7 years)
    - Implement field-level visibility controls for audit log entries
    - Implement data retention policies (configurable inactivity period, 30-day pre-deletion notification)
    - Implement IP reputation check (90-day history, verification email for new IPs)
    - _Requirements: 16.1, 16.2, 16.3, 16.5, 16.7, 16.8, 16.12_

  - [ ] 9.2 Implement Reporting Service
    - Implement membership reports: active/expired/pending counts, age distribution, growth trends, retention rate
    - Implement financial reports: revenue by type/method, outstanding balances, trends, budget vs actual
    - Implement attendance reports: utilisation rate, no-show %, per-member frequency
    - Implement date range filtering (max 365 days), export to CSV/PDF (max 10,000 rows)
    - Implement scheduled report delivery (daily/weekly/monthly via email)
    - Implement SuperAdmin platform-wide reports (anonymised member data)
    - Implement timeout handling (30s timeout, cancel on exceed, no partial data)
    - _Requirements: 23.1, 23.2, 23.3, 23.4, 23.5, 23.6, 23.7_

  - [ ] 9.3 Implement Integration Ecosystem
    - Implement GoCardless Direct Debit (mandate creation/cancellation, payment collection, status tracking)
    - Implement Xero and QuickBooks sync (payments, invoices, refunds at configurable interval, default 60 min)
    - Implement Google Calendar and Outlook Calendar sync (sessions/events within 5 min of change)
    - Implement Facebook and X (Twitter) auto-posting (published events, match results within 10 min)
    - Implement public REST API with API key authentication (1000 req/hour per key)
    - Implement webhook handling (idempotent, signature verification, async processing)
    - Implement integration failure handling (log, notify, retry up to 5x, mark for manual intervention)
    - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.5, 20.6, 20.7, 20.8_

  - [ ] 9.4 Implement Self-Service Onboarding
    - Implement public landing page API (tier comparison, sign-up flow)
    - Implement Stripe checkout integration for paid tiers (club creation within 10s of payment)
    - Implement Free tier sign-up without payment
    - Implement checkout failure/abandonment handling (retry without data loss)
    - Implement onboarding wizard steps: club profile, membership types, member import, payment provider
    - Implement step skip/return functionality
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [ ] 9.5 Implement Background Jobs (Hangfire)
    - Configure Hangfire with SQL Server persistence and dashboard (admin-only)
    - Implement scheduled jobs: membership auto-renewal (daily 02:00), payment reminders (daily 09:00), dunning retries, analytics snapshot (weekly), overdue invoice check, equipment loan check, trial expiry, data retention cleanup
    - Implement on-demand jobs: email campaigns, report generation (120s timeout), integration sync
    - Implement retry policies (3x exponential backoff: 30s, 60s, 120s) with ops alerting on final failure
    - _Requirements: 18.1, 18.6_

  - [ ] 9.6 Implement Search and Filtering
    - Implement global search across members, events, sessions, invoices (scoped to tenant)
    - Implement debounced server-side search (2+ chars, 300ms debounce, max 20 results grouped by type)
    - Implement advanced filtering: multi-select, date range, status, free-text (AND logic)
    - Implement saved filter presets (max 10 per user, name up to 50 chars)
    - Implement search result highlighting
    - Implement performance target: results within 500ms for up to 10,000 records
    - _Requirements: 37.1, 37.2, 37.3, 37.4, 37.5, 37.6, 37.7_

  - [ ] 9.7 Implement Revenue Features
    - Implement add-ons catalog: SMS credit packs, additional storage, white-label branding, custom domain
    - Implement white-label serving (custom domain, club branding)
    - Implement public club website at slug URL (name, description, events, membership types, sign-up link)
    - Implement sponsor management (max 50 per club, agreements with dates/amounts/status)
    - Implement QR/NFC check-in (validate identity, verify booking, record attendance within 3s)
    - _Requirements: 17.3, 17.4, 17.5, 17.6, 17.7, 17.8_

- [ ] 10. Checkpoint - All backend modules complete
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 11. Frontend Foundation

  - [ ] 11.1 Create Angular 20 project with configuration
    - Initialize Angular 20 project at `src/the-league-client/`
    - Configure Tailwind CSS v4 and DaisyUI 5.x
    - Configure Angular SSR for public pages
    - Configure PWA (service worker, web app manifest, offline fallback)
    - Configure environment files (development, staging, production)
    - Configure i18n resource files (externalise all user-facing text)
    - Set up proxy configuration for API calls
    - _Requirements: 25.1, 25.2, 25.11, 30.1, 34.4_

  - [ ] 11.2 Implement Core Services
    - Implement `AuthService` (login, register, token refresh, logout, session management)
    - Implement `ApiService` (HTTP client with interceptors for JWT, correlation ID, error handling)
    - Implement `ThemeService` (system/light/dark switching, localStorage persistence, per-club brand colors)
    - Implement `NotificationService` (SignalR connection, toast display)
    - Implement `SignalRService` (connection management, auto-reconnect 5x with backoff)
    - Implement `OfflineService` (connectivity detection, action queue in IndexedDB)
    - _Requirements: 25.1, 25.8, 29.1, 29.2, 29.3, 29.4, 30.5_

  - [ ] 11.3 Implement HTTP Interceptors and Guards
    - Implement JWT interceptor (attach Bearer token, handle 401 with token refresh)
    - Implement correlation ID interceptor (generate/attach X-Correlation-Id)
    - Implement error interceptor (global error handling, toast notifications)
    - Implement loading interceptor (track pending requests)
    - Implement route guards: `AuthGuard`, `RoleGuard`, `TenantGuard`
    - _Requirements: 25.1, 3.7_

  - [ ] 11.4 Implement Shared Component Library
    - Create `DataTableComponent` (sorting, filtering, pagination, row click, skeleton loading)
    - Create form controls: `TextInputComponent`, `SelectComponent`, `DatePickerComponent`, `CheckboxComponent`, `RadioComponent`, `TextareaComponent`
    - Create `ModalComponent` and `DialogService`
    - Create `ToastComponent` and `ToastService`
    - Create `StatusBadgeComponent` (configurable colors per status)
    - Create `PaginationComponent` (page size selector, page navigation)
    - Create `SkeletonLoaderComponent` (various shapes: text, card, table row)
    - Create `EmptyStateComponent` (icon, message, action button)
    - Create `ChartComponent` wrappers (line, area, doughnut, heatmap via Chart.js)
    - All components standalone, OnPush change detection, signal-based inputs/outputs
    - _Requirements: 25.4, 25.6, 25.7, 25.9_

  - [ ]* 11.5 Write property tests for pagination and currency (Properties 32-33)
    - **Property 32: Pagination Calculation** — verify totalCount, totalPages, page defaults, items count
    - **Property 33: Currency Formatting** — verify GBP format "£{N},{NNN}.{NN}" with 2dp always
    - **Validates: Requirements 26.3, 34.1**

  - [ ] 11.6 Implement Component Showcase Page
    - Create a dedicated route `/showcase` displaying all shared components
    - Document each component with usage examples and input/output contracts
    - Include interactive demos with different states (loading, empty, error, populated)
    - _Requirements: 25.5_

  - [ ] 11.7 Implement Portal Layouts
    - Create `AdminLayoutComponent` (SuperAdmin navigation, platform-wide menu)
    - Create `ClubLayoutComponent` (ClubManager navigation, club-scoped menu)
    - Create `PortalLayoutComponent` (Member navigation, self-service menu)
    - Create `PublicLayoutComponent` (unauthenticated pages, landing)
    - Implement responsive design: desktop (≥1024px), tablet (768-1023px), mobile (<768px)
    - Implement minimum touch targets (44×44px on mobile/tablet)
    - Configure lazy-loaded route chunks per portal (max 500KB gzipped each, main bundle ≤250KB)
    - _Requirements: 25.3, 25.6, 25.12_

  - [ ] 11.8 Implement Theme System and Dark Mode
    - Implement DaisyUI theme configuration (system/light/dark)
    - Implement localStorage persistence of theme preference
    - Implement OS preference detection and auto-switch (within 300ms)
    - Implement per-club brand color application (primary, secondary, accent)
    - Implement color palette generation (5 shades + 5 tints per brand color)
    - Implement WCAG AA contrast validation with auto-substitution
    - Implement theme transitions without layout shift (CLS < 0.01, within 300ms)
    - _Requirements: 29.1, 29.2, 29.3, 29.4, 29.5, 29.6, 29.7_

- [ ] 12. Checkpoint - Frontend foundation complete
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 13. Frontend Features — Auth and Admin Portal

  - [ ] 13.1 Implement Auth Feature
    - Create login page with email/password form
    - Create registration page with email verification flow
    - Create password reset flow (request + confirm)
    - Create 2FA setup and verification pages (TOTP for SuperAdmin/ClubManager)
    - Implement account lockout display (remaining duration)
    - Implement session management page (view/revoke active sessions)
    - _Requirements: 2.1, 2.6, 2.7, 2.9, 2.10, 2.11, 16.4_

  - [ ] 13.2 Implement SuperAdmin Portal
    - Create platform dashboard: total clubs, total members, platform revenue, active subscriptions KPIs
    - Create club management: list all clubs, view/edit club details, activate/deactivate
    - Create user management: list all users, role assignment, account actions
    - Create subscription management: view all subscriptions, override tiers, manage trials
    - Create platform-wide reporting (anonymised member data)
    - Create platform configuration: fee settings, tier limits, add-on catalog
    - Create Hangfire dashboard link (admin-only access)
    - _Requirements: 3.2, 23.5, 33.1_

- [ ] 14. Frontend Features — Club Manager Portal

  - [ ] 14.1 Implement Club Manager Dashboard
    - Create dashboard with KPI cards: active members, monthly revenue, session attendance rate, outstanding balance
    - Create interactive charts: revenue trends (line, 12-month), membership growth (area, 12-month), attendance heatmap (4-week), payment method distribution (doughnut)
    - Implement date range selection (this week/month/quarter/year/custom up to 24 months)
    - Implement activity feed (20 most recent actions, 30s polling refresh)
    - Implement chart tooltips (within 200ms), full-screen mode, export (PNG/CSV)
    - Implement async chart loading with skeleton placeholders (render within 3s)
    - _Requirements: 33.1, 33.2, 33.3, 33.4, 33.5, 33.6, 33.7, 33.8_

  - [ ] 14.2 Implement Member Management UI
    - Create member list with search (name/email), filters (status), pagination
    - Create member detail/edit form (all profile fields, custom fields, family members)
    - Create member creation form with auto-generated member number display
    - Create CSV/Excel import wizard with progress, validation results, error report
    - Create member status transition UI with audit history
    - Implement global search bar (debounced, grouped results, highlighting)
    - Implement saved filter presets
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.7, 6.8, 5.6, 37.1, 37.2, 37.3, 37.4, 37.5_

  - [ ] 14.3 Implement Membership Management UI
    - Create membership type CRUD forms (name, pricing, billing cycle, age limits, capacity)
    - Create membership enrolment flow with age validation feedback
    - Create discount management (type, percentage/fixed, validity dates)
    - Create membership freeze request handling
    - Create waitlist management view
    - Create auto-renewal status and payment failure indicators
    - _Requirements: 7.1, 7.2, 7.3, 7.7, 7.8, 7.9_

  - [ ] 14.4 Implement Session Management UI
    - Create session CRUD forms (title, category, venue, datetime, duration, capacity, fee)
    - Create recurring schedule template builder (day-of-week, time, horizon)
    - Create session calendar view (week/month)
    - Create booking management (view bookings, waitlist, attendance marking)
    - Create attendance tracking interface (Confirmed/Attended/NoShow/Cancelled)
    - _Requirements: 8.1, 8.2, 8.7_

  - [ ] 14.5 Implement Event Management UI
    - Create event CRUD forms (title, type, datetime, capacity, ticketing/RSVP config)
    - Create event series and multi-session event builder
    - Create event lifecycle management (status transitions, publish, cancel)
    - Create registration/ticket management view
    - Create event cancellation flow with refund confirmation
    - _Requirements: 9.1, 9.2, 9.5, 9.7, 9.8_

  - [ ] 14.6 Implement Competition Management UI
    - Create competition CRUD (type, season, teams)
    - Create team registration form (squad, captain, venue)
    - Create fixture generation interface (round-robin/knockout)
    - Create match result entry form with sport-specific scoring
    - Create match event recording (goals, cards, substitutions, lineups)
    - Create standings table with live updates
    - Create season management view
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.6, 10.8_

  - [ ] 14.7 Implement Payment and Financial Management UI
    - Create payment recording form (method selection, amount, member)
    - Create invoice builder (line items, send, track status)
    - Create payment plan setup and installment tracking
    - Create member balance view (credits, debits, outstanding)
    - Create refund processing interface
    - Create financial reports dashboard (revenue, outstanding, trends)
    - Create double-entry bookkeeping: chart of accounts, journal entries
    - _Requirements: 11.1, 11.4, 11.6, 11.7, 11.8, 11.10, 23.2_

  - [ ] 14.8 Implement Facility and Equipment Management UI
    - Create facility CRUD (type, operating schedules, pricing)
    - Create facility booking calendar with conflict visualization
    - Create maintenance scheduling interface
    - Create equipment inventory list with condition tracking
    - Create loan management and overdue tracking
    - Create reservation management
    - _Requirements: 12.1, 12.2, 12.4, 12.7, 13.1, 13.4, 13.5_

  - [ ] 14.9 Implement Programs, Communications, and Shop UI
    - Create program CRUD (type, skill level, sessions, capacity)
    - Create enrolment management and waitlist view
    - Create attendance tracking and certificate issuance
    - Create email template editor with placeholder preview
    - Create bulk campaign builder (segment selection, send)
    - Create shop product management (listings, variants, stock, categories)
    - Create order management and fulfilment tracking
    - _Requirements: 14.1, 14.2, 14.3, 21.1, 21.2, 38.1, 38.5, 38.8_

  - [ ] 14.10 Implement Club Settings and Configuration UI
    - Create club profile editor (name, description, logo, colors, contact)
    - Create subscription management (current tier, upgrade/downgrade, usage meters)
    - Create payment provider configuration (Stripe, PayPal, GoCardless)
    - Create integration settings (calendar sync, accounting, social media)
    - Create notification preferences management
    - Create custom field definition builder
    - Create sport-specific terminology customization
    - Create sponsor management
    - _Requirements: 5.5, 17.6, 20.2, 20.3, 28.4, 28.6_

- [ ] 15. Frontend Features — Member Portal

  - [ ] 15.1 Implement Member Portal Dashboard
    - Create personalised dashboard: membership status, upcoming bookings (7 days), upcoming events (30 days), outstanding balance
    - Implement optimistic UI for booking/RSVP actions (immediate reflect, rollback on error within 1s)
    - _Requirements: 22.1, 25.10_

  - [ ] 15.2 Implement Member Session Booking
    - Create session browser with availability indicators
    - Implement booking flow (confirm if available, join waitlist if full)
    - Implement cancellation with deadline enforcement (24h default)
    - Display booking confirmation (date, time, venue)
    - _Requirements: 22.2, 22.3, 22.4, 22.5_

  - [ ] 15.3 Implement Member Events and Payments
    - Create event browser with registration/RSVP flow
    - Implement ticket purchase with QR code display
    - Create payment history and outstanding invoices view
    - Implement online invoice payment via configured provider
    - Handle payment failure display
    - _Requirements: 22.6, 22.7, 22.8_

  - [ ] 15.4 Implement Member Profile and Family Management
    - Create profile editor (personal details, address, emergency contacts, medical info, notification preferences)
    - Create family member management (add/edit/remove, max 10, relationship types)
    - _Requirements: 22.9, 22.10_

  - [ ] 15.5 Implement Member Shop
    - Create product catalogue with category filtering and sorting (price/name)
    - Implement purchase flow with variant selection and stock validation
    - Create order history view
    - Implement restock notification registration for out-of-stock items
    - _Requirements: 38.2, 38.4, 38.5, 38.7_

- [ ] 16. Checkpoint - All frontend features complete
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 17. Production Features — PWA, SSR, Live Scoring

  - [ ] 17.1 Implement PWA and Offline Support
    - Configure service worker caching (app shell + last 50 viewed items)
    - Implement offline viewing: cached sessions (14 days), events, profile
    - Implement offline action queue (up to 100 actions in IndexedDB)
    - Implement Coach offline attendance marking
    - Implement sync on reconnect (within 30s, summary notification)
    - Implement conflict resolution UI (local vs server version choice)
    - Implement offline indicator in app header
    - Implement retry for failed sync actions (max 3 attempts)
    - _Requirements: 30.1, 30.2, 30.3, 30.4, 30.5, 30.6, 30.7, 30.8_

  - [ ] 17.2 Implement SSR for Public Pages
    - Implement Angular SSR for platform landing page
    - Implement SSR for public club profile pages (at slug URL)
    - Implement hydration for interactive elements
    - Target LCP ≤ 2.5s on simulated 4G
    - Implement public club website content: name, description, contact, upcoming events, membership types, sign-up link
    - _Requirements: 25.11, 17.5_

  - [ ] 17.3 Implement Live Scoring and Match Centre
    - Create `MatchCentreHub` (SignalR) with match-group subscriptions (max 500 concurrent per match)
    - Implement live score display updated within 3s of scoring change
    - Implement sport-specific scoring formats (cricket, football, hockey, tennis, rugby)
    - Implement live commentary feed (timestamped, reverse-chronological, last 100 entries)
    - Implement public scoreboard URL (no auth required)
    - Implement match completion → summary view (final score, events log, player stats)
    - Implement connection loss handling (auto-reconnect 5x with backoff, status indicator)
    - Implement pre-match display (teams, venue, start time) for Scheduled/Confirmed matches
    - _Requirements: 31.1, 31.2, 31.3, 31.4, 31.5, 31.6, 31.7, 31.8_

  - [ ] 17.4 Implement Onboarding Wizard Frontend
    - Create tier selection page with feature comparison
    - Create Stripe checkout integration (redirect + callback handling)
    - Create multi-step wizard: club profile → membership types → member import → payment provider
    - Implement step skip/return navigation
    - Implement checkout failure/abandonment recovery
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [ ] 17.5 Implement Internationalisation and Locale Formatting
    - Implement GBP currency formatting (£1,234.56)
    - Implement UK date formatting (DD/MM/YYYY) with relative time for <48h
    - Implement 24-hour time format (HH:mm) with Europe/London timezone
    - Implement UTC storage → club timezone display conversion
    - Implement BST/GMT auto-adjustment
    - Implement null/invalid date placeholder ("N/A" or dash)
    - _Requirements: 34.1, 34.2, 34.3, 34.5, 34.6, 34.7_

- [ ] 18. Testing, DevOps, and Data Seeding

  - [ ] 18.1 Implement Architecture Tests
    - Create `ModuleBoundaryTests` (no cross-module direct references except shared contracts)
    - Create `DependencyRuleTests` (Domain has zero external deps, Application depends only on Domain)
    - Create `NamingConventionTests` (commands end in Command, queries end in Query, handlers end in Handler)
    - Verify no circular dependencies between modules
    - Verify CQRS separation (no class performs both read and write)
    - _Requirements: 24.1, 24.2, 24.3, 24.6_

  - [ ]* 18.2 Write integration tests for cross-module flows
    - Test member creation → membership enrolment → payment processing flow
    - Test session booking → waitlist promotion → notification delivery flow
    - Test event registration → ticket purchase → cancellation refund flow
    - Test subscription upgrade → feature gate activation flow
    - _Requirements: 24.3, 24.7_

  - [ ] 18.3 Implement Docker and Local Development Environment
    - Create multi-stage Dockerfile for API (restore → build → publish → runtime)
    - Create multi-stage Dockerfile for Angular frontend (build → nginx)
    - Create `docker-compose.yml`: API, client, SQL Server 2022, Redis 7, Azurite
    - Create `docker-compose.override.yml` for dev overrides
    - Configure health check endpoints for container orchestration
    - _Requirements: 36.1, 36.2_

  - [ ] 18.4 Implement CI/CD Pipeline
    - Create GitHub Actions workflow: restore, build, unit tests, property tests, architecture tests, integration tests
    - Create frontend pipeline: npm ci, lint, test (--run), build production
    - Create Docker image build and publish steps
    - Configure environment-specific deployment (staging on develop, production on main)
    - Configure health check verification post-deployment
    - _Requirements: 36.3, 36.4, 36.5, 36.6_

  - [ ] 18.5 Implement Database Seeder
    - Create 4 demo clubs across 3+ sport types
    - Create 10 managers (2+ per club) and 50 members per club (distributed across all statuses)
    - Create demo user accounts per role per club with documented credentials
    - Implement idempotent seeding (skip existing, no duplicates)
    - Generate 12 months historical payment data (3+ methods, 5-10% failed, seasonal peaks Sep-Oct/Jan)
    - Generate 6 months session attendance (70% in-season, 40% off-season, 5-15% no-show)
    - Generate 1 active competition (50%+ matches completed), 2 upcoming fixtures, 1 completed season
    - Generate minimum 30 data points per chart series for all dashboard widgets
    - _Requirements: 27.1, 27.2, 27.3, 27.4, 27.5, 27.6, 27.7_

  - [ ] 18.6 Implement EF Core Migrations
    - Create initial migrations for all 16 module DbContexts
    - Configure idempotent migration execution on startup
    - Create database indexes per design (unique constraints, composite indexes, filtered indexes)
    - Configure cascade delete strategy per design
    - _Requirements: 36.5, 24.4_

  - [ ] 18.7 Implement Resilience Patterns
    - Configure Polly circuit breakers for Stripe, SendGrid, Twilio, GoCardless (5 failures/30s → 30s break)
    - Implement graceful degradation responses during circuit open state
    - Implement Redis fallback to IMemoryCache
    - Implement DB connection pool exhaustion handling (30s queue → 503)
    - Implement request timeout policies (30s standard, 120s reports, 300s imports, 60s uploads)
    - _Requirements: 35.2, 35.3, 35.4, 35.5, 35.6_

- [ ] 19. Final Checkpoint - Full platform integration
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document (35 properties total)
- Unit tests validate specific examples and edge cases
- Backend uses C# (.NET 9, ASP.NET Core, EF Core, MediatR, FsCheck)
- Frontend uses TypeScript (Angular 20, Tailwind v4, DaisyUI 5.x)
- All 38 requirements and 280+ acceptance criteria are covered across the task list
- Architecture tests enforce module boundaries and Clean Architecture compliance

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2", "1.3"] },
    { "id": 2, "tasks": ["1.4", "1.6", "1.7"] },
    { "id": 3, "tasks": ["1.5", "1.8"] },
    { "id": 4, "tasks": ["1.9", "1.11"] },
    { "id": 5, "tasks": ["1.10", "1.12"] },
    { "id": 6, "tasks": ["3.1", "3.2"] },
    { "id": 7, "tasks": ["3.3", "3.4"] },
    { "id": 8, "tasks": ["3.5", "3.6"] },
    { "id": 9, "tasks": ["3.7", "4.1"] },
    { "id": 10, "tasks": ["4.2", "4.3"] },
    { "id": 11, "tasks": ["4.4", "4.5"] },
    { "id": 12, "tasks": ["4.6", "5.1"] },
    { "id": 13, "tasks": ["5.2"] },
    { "id": 14, "tasks": ["7.1", "7.3", "7.5"] },
    { "id": 15, "tasks": ["7.2", "7.4", "7.6"] },
    { "id": 16, "tasks": ["8.1", "8.2", "8.3"] },
    { "id": 17, "tasks": ["8.4", "8.5", "8.7"] },
    { "id": 18, "tasks": ["8.6", "9.1", "9.2"] },
    { "id": 19, "tasks": ["9.3", "9.4", "9.5"] },
    { "id": 20, "tasks": ["9.6", "9.7"] },
    { "id": 21, "tasks": ["11.1"] },
    { "id": 22, "tasks": ["11.2", "11.3"] },
    { "id": 23, "tasks": ["11.4", "11.5"] },
    { "id": 24, "tasks": ["11.6", "11.7", "11.8"] },
    { "id": 25, "tasks": ["13.1", "13.2"] },
    { "id": 26, "tasks": ["14.1", "14.2"] },
    { "id": 27, "tasks": ["14.3", "14.4", "14.5"] },
    { "id": 28, "tasks": ["14.6", "14.7", "14.8"] },
    { "id": 29, "tasks": ["14.9", "14.10"] },
    { "id": 30, "tasks": ["15.1", "15.2", "15.3"] },
    { "id": 31, "tasks": ["15.4", "15.5"] },
    { "id": 32, "tasks": ["17.1", "17.2", "17.3"] },
    { "id": 33, "tasks": ["17.4", "17.5"] },
    { "id": 34, "tasks": ["18.1", "18.3"] },
    { "id": 35, "tasks": ["18.2", "18.4", "18.5"] },
    { "id": 36, "tasks": ["18.6", "18.7"] }
  ]
}
```
