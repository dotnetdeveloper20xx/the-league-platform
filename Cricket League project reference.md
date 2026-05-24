# The League - Complete Codebase Reference Document

> **Purpose:** This document captures every feature, technology, pattern, and implementation detail of "The League" so it can be cloned or rebuilt in a different stack without needing the original codebase.

---

## 1. PROJECT OVERVIEW

**The League** is a multi-tenant SaaS platform for managing sports club memberships, sessions, events, payments, and competitions. It serves Cricket, Football, Hockey, Rugby, Tennis, Swimming, Athletics, Golf clubs and more.

### Business Problem
Sports clubs rely on spreadsheets and disconnected systems. The League provides a unified solution that centralises member data, streamlines session booking, simplifies event management, automates payment tracking, and enables member self-service.

### Multi-Tenancy Model
- **Strategy:** Shared database with `ClubId` discriminator column on every tenant-scoped table
- **Isolation:** EF Core global query filters automatically apply `WHERE ClubId = @TenantId`
- **Resolution:** ClubId extracted from JWT claims via TenantMiddleware
- Each club operates in complete data isolation within a single database

---

## 2. TECHNOLOGY STACK

### Backend
| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 8.0 LTS |
| Framework | ASP.NET Core | 8.0 |
| Language | C# | 12 |
| ORM | Entity Framework Core | 8.0 |
| Database | SQL Server | 2022 (LocalDB for dev) |
| Identity | ASP.NET Core Identity | 8.0 |
| Auth | JWT Bearer Tokens | - |
| API Docs | Swagger/Swashbuckle | 6.5.0 |

### Frontend
| Component | Technology | Version |
|-----------|------------|---------|
| Framework | Angular | 19.2 |
| Language | TypeScript | 5.7 |
| CSS | Tailwind CSS | 3.4 |
| Reactive | RxJS | 7.8 |
| Charts | Chart.js + ng2-charts | 4.5 / 6.0 |
| CDK | @angular/cdk | 19.2 |
| E2E Testing | Playwright | 1.57 |
| Unit Testing | Karma + Jasmine | - |

### External Integrations (via Factory/Provider Pattern)
| Provider | Purpose | Status |
|----------|---------|--------|
| Stripe | Payment processing | Mock + real implementation |
| PayPal | Payment processing | Mock + structure ready |
| SendGrid | Email delivery | Mock + real implementation |

### NuGet Packages
```xml
Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0
Microsoft.EntityFrameworkCore.SqlServer 8.0.0
Microsoft.EntityFrameworkCore.Design 8.0.0
Swashbuckle.AspNetCore 6.5.0
```

### NPM Dependencies
```json
"@angular/cdk": "^19.2.19", "@angular/core": "^19.2.0",
"@angular/forms": "^19.2.0", "@angular/router": "^19.2.0",
"chart.js": "^4.5.1", "ng2-charts": "^6.0.1",
"rxjs": "~7.8.0", "tailwindcss": "^3.4.19",
"@playwright/test": "^1.57.0", "typescript": "~5.7.2"
```

---

## 3. ARCHITECTURE

### Solution Structure
```
TheLeague/
├── TheLeague.Api/              # ASP.NET Core 8 Web API (entry point)
│   ├── Controllers/ (20)       # API endpoints
│   ├── Services/ (16)          # Business logic
│   ├── DTOs/                   # Request/Response models
│   ├── Middleware/             # ExceptionHandling, Tenant
│   ├── Providers/Payment/      # IPaymentProvider, Stripe, PayPal, Mock
│   ├── Providers/Email/        # IEmailProvider, SendGrid, Mock
│   └── Program.cs             # Startup, DI, pipeline config
├── TheLeague.Core/             # Domain layer (ZERO dependencies)
│   ├── Entities/ (48)          # Domain models
│   └── Enums/Enums.cs          # All enumerations
├── TheLeague.Infrastructure/   # Data access layer
│   └── Data/
│       ├── ApplicationDbContext.cs  # 100+ DbSets, entity config
│       ├── Migrations/ (11 phases)
│       ├── TenantService.cs
│       └── ITenantService.cs
├── TheLeague.Tests/            # xUnit tests
├── the-league-client/          # Angular 19 SPA
│   └── src/app/
│       ├── core/services/ (17) # API services
│       ├── core/guards/        # auth, role guards
│       ├── core/interceptors/  # auth, error interceptors
│       ├── core/models/ (8)    # TypeScript interfaces
│       ├── features/auth/      # Login, Register, Forgot/Reset Password
│       ├── features/admin/     # SuperAdmin portal (6 sections)
│       ├── features/club/      # Club Manager portal (12 sections)
│       ├── features/portal/    # Member portal (7 sections)
│       ├── shared/components/  # 6 reusable components
│       ├── shared/pipes/       # 3 pipes
│       └── layouts/            # AdminLayout, PortalLayout, Unauthorized
└── seedData.json               # Demo data (4 clubs, 10+ managers, 15+ members)
```

### Request Pipeline (order matters)
```
HTTP Request
  → ExceptionHandlingMiddleware (catches all errors)
  → CORS ("AllowAngular" policy for localhost:4200)
  → Authentication (JWT Bearer validation)
  → Authorization ([Authorize] attributes)
  → TenantMiddleware (extracts ClubId from JWT)
  → Controller Action
  → Service Layer (business logic)
  → EF Core (with global query filters)
  → SQL Server
```

### Ports
| Service | Port | URL |
|---------|------|-----|
| Backend API | 7000 | http://localhost:7000 |
| Swagger | 7000 | http://localhost:7000/swagger |
| Frontend | 4200 | http://localhost:4200 |

---

## 4. USER ROLES & ACCESS CONTROL

| Role | Route Prefix | Access Level |
|------|-------------|--------------|
| SuperAdmin | `/admin` | Platform-wide: all clubs, system config, users |
| ClubManager | `/club` | Full CRUD on their assigned club's data |
| Member | `/portal` | Self-service: profile, bookings, payments |
| Coach | `/club` (limited) | Session management only |
| Staff | `/club` (limited) | Read-only views |

---

## 5. AUTHENTICATION & SECURITY

### JWT Configuration
```json
{
  "Jwt": {
    "Secret": "min-32-character-secret-key",
    "Issuer": "TheLeague",
    "Audience": "TheLeagueApp",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Token Claims
| Claim | Value |
|-------|-------|
| sub (NameIdentifier) | User ID (GUID) |
| email | User email |
| name | Full name |
| role | "SuperAdmin" / "ClubManager" / "Member" |
| clubId | Tenant GUID |
| memberId | Member record GUID (if member) |
| jti | Unique token ID |

### Password Policy
- Min 6 chars, requires digit, lowercase, uppercase
- Non-alphanumeric NOT required
- Unique email required

### Auth Endpoints
| Method | Route | Purpose |
|--------|-------|---------|
| POST | /api/auth/login | Login, returns JWT + refresh token |
| POST | /api/auth/register | Register new user + member record |
| POST | /api/auth/refresh | Refresh expired access token |
| POST | /api/auth/forgot-password | Send reset email |
| POST | /api/auth/reset-password | Reset with token |
| PUT | /api/auth/change-password | Change (authenticated) |
| POST | /api/auth/verify-email | Verify email token |
| GET | /api/auth/me | Get current user profile |

### Frontend Auth Flow
1. Login → store access_token, refresh_token, current_user in localStorage
2. authInterceptor attaches `Authorization: Bearer <token>` to all requests
3. errorInterceptor handles 401 (logout), 403 (unauthorized page), 500 (toast)
4. Route guards check `authService.isAuthenticated` and role
5. On 401, attempt token refresh; if fails, redirect to login

---

## 6. MULTI-TENANCY IMPLEMENTATION

### How It Works
1. User logs in → JWT contains `clubId` claim
2. TenantMiddleware extracts clubId from JWT (fallback: X-Tenant-Id header)
3. TenantService stores `Guid? CurrentTenantId` (scoped per request)
4. ApplicationDbContext applies global query filters on ALL tenant-scoped entities
5. Controllers call `GetClubId()` from BaseApiController and pass to services
6. Services ALWAYS filter by ClubId — double protection

### Key Code
```csharp
// ITenantService
public interface ITenantService {
    Guid? CurrentTenantId { get; }
    void SetCurrentTenant(Guid? tenantId);
}

// BaseApiController helper
protected Guid GetClubId() {
    var claim = User.FindFirst("clubId");
    return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
}
```

### Entities with Global Query Filters (50+)
Member, FamilyMember, MembershipType, Membership, Payment, Session,
RecurringSchedule, SessionBooking, RecurringBooking, Waitlist, Event,
EventTicket, EventRSVP, Venue, MemberDocument, MemberNote,
CustomFieldDefinition, CommunicationTemplate, EmailLog, BulkEmailCampaign,
ClubAnalyticsSnapshot, MembershipDiscount, MembershipFreeze, GuestPass,
MembershipWaitlist, Fee, Invoice, PaymentPlan, PaymentInstallment,
MemberBalance, BalanceTransaction, Refund, PaymentReminder, ChartOfAccount,
JournalEntry, FiscalYear, FiscalPeriod, TaxRate, BankReconciliation,
Budget, FinancialAuditLog, SavedFinancialReport, Vendor, Expense,
PurchaseOrder, Facility, FacilityBooking, Equipment, EquipmentLoan,
EquipmentReservation, Program, ProgramEnrollment, MemberCertificate,
EventSeries, EventRegistration, Season, Competition, CompetitionTeam

---

## 7. API CONTROLLERS & SERVICES

### Controllers (20 total)
| Controller | Route | Role Required | Purpose |
|-----------|-------|---------------|---------|
| AuthController | /api/auth | Public/Any | Login, register, password reset |
| AdminController | /api/admin | SuperAdmin | System dashboard, user management |
| ClubsController | /api/clubs | SuperAdmin | CRUD all clubs |
| ClubController | /api/club | ClubManager | Club profile, settings, dashboard |
| MembersController | /api/members | ClubManager | Member CRUD, family members |
| MembershipsController | /api/memberships | ClubManager | Membership enrollment/renewal |
| MembershipTypesController | /api/membership-types | ClubManager | Configure membership categories |
| SessionsController | /api/sessions | ClubManager/Member | Session CRUD, booking, attendance |
| RecurringSchedulesController | /api/recurring-schedules | ClubManager | Recurring session templates |
| EventsController | /api/events | ClubManager/Member | Event CRUD, RSVP, tickets |
| CompetitionController | /api/competitions | ClubManager | Competitions, teams, matches |
| PaymentsController | /api/payments | ClubManager | Record/list payments, refunds |
| InvoicesController | /api/invoices | ClubManager | Invoice CRUD, send, void |
| FeesController | /api/fees | ClubManager | Fee configuration |
| VenuesController | /api/venues | ClubManager/Member | Venue management |
| ReportsController | /api/reports | ClubManager | Financial/membership reports |
| PortalController | /api/portal | Member | Member self-service dashboard |
| SystemConfigurationController | /api/system-configuration | SuperAdmin | Payment/email/feature config |

### Services (16 total)
AuthService, ClubService, MemberService, MembershipService,
SessionService, EventService, CompetitionService, PaymentService,
InvoiceService, FeeService, VenueService, EmailService,
ReportService, SystemConfigurationService, DatabaseSeeder

### API Response Pattern
```json
{ "success": true, "message": null, "data": { ... } }
{ "success": false, "message": "Error description", "data": null }
```

### Pagination Pattern
```json
{ "items": [...], "totalCount": 150, "page": 1, "pageSize": 20, "totalPages": 8 }
```
Query params: `page` (default 1), `pageSize` (default 20, max 100)

---

## 8. DATABASE SCHEMA

### Migration Phases (11 total)
| # | Migration | Entities Added |
|---|-----------|---------------|
| 1 | InitialCreate | Club, ClubSettings, Member, FamilyMember, MembershipType, Membership, Payment, Session, RecurringSchedule, SessionBooking, RecurringBooking, Waitlist, Event, EventTicket, EventRSVP, Venue |
| 2 | EnhancedMemberManagement | MemberDocument, MemberNote, CustomFieldDefinition, CommunicationTemplate, EmailLog, BulkEmailCampaign, ClubAnalyticsSnapshot |
| 3 | MembershipPlansAndPricing | MembershipDiscount, MembershipFreeze, GuestPass, MembershipWaitlist |
| 4 | Phase4FeesAndPayments | Fee, Invoice, InvoiceLineItem, PaymentPlan, PaymentInstallment, MemberBalance, BalanceTransaction, Refund, PaymentReminder |
| 5 | Phase5FinancialManagement | ChartOfAccount, JournalEntry, JournalEntryLine, FiscalYear, FiscalPeriod, TaxRate, BankReconciliation, BankReconciliationLine, Budget, BudgetLine, FinancialAuditLog, SavedFinancialReport |
| 6 | Phase6ExpenseManagement | Vendor, Expense, ExpenseLineItem, ExpenseApproval, ExpenseAttachment, ExpensePayment, PurchaseOrder, PurchaseOrderLine, PurchaseOrderReceipt, PurchaseOrderReceiptLine |
| 7 | Phase7FacilityManagement | VenueOperatingSchedule, VenueHoliday, Facility, FacilityAvailability, FacilityPricing, FacilityBooking, FacilityMaintenance, FacilityBlockout |
| 8 | Phase8EquipmentManagement | Equipment, EquipmentLoan, EquipmentMaintenance, EquipmentReservation |
| 9 | Phase9ProgramManagement | Program, ProgramSession, ProgramEnrollment, ProgramInstructor, ProgramAttendance, MemberCertificate |
| 10 | Phase10EventsAndCompetitions | EventSeries, EventSession, EventRegistration, Season, Competition, CompetitionRound, CompetitionTeam, CompetitionParticipant, Match, MatchEvent, MatchLineup, CompetitionStanding |
| 11 | AddSystemConfiguration | SystemConfiguration, ConfigurationAuditLog |

### Core Entity: Club (Tenant Root)
```
Id (GUID PK), Name (200), Slug (100 UNIQUE), Description, LogoUrl,
PrimaryColor (#1E40AF), SecondaryColor (#3B82F6), ContactEmail, ContactPhone,
Address, Website, ClubType (enum), IsActive, CreatedAt, RenewalDate,
PreferredPaymentProvider, StripeAccountId, PayPalClientId,
SendGridApiKey, FromEmail, FromName
```

### Core Entity: Member
```
Id, ClubId (FK), UserId (FK nullable), MemberNumber (unique per club),
QRCodeData, FirstName, LastName, Email, Phone, DateOfBirth, Gender,
Address, AddressLine2, City, County, PostCode, Country, ProfilePhotoUrl,
EmergencyContactName/Phone/Relation, SecondaryEmergencyContact...,
MedicalConditions, Allergies, DoctorName, DoctorPhone, BloodType,
FacebookUrl, TwitterHandle, InstagramHandle, LinkedInUrl,
CustomFieldValues (JSON), MarketingOptIn, SmsOptIn, EmailOptIn,
IsFamilyAccount, PrimaryMemberId (self-ref FK),
ApplicationStatus, WaiverAccepted, TermsAccepted,
Status (enum), JoinedDate, LastLoginDate, IsActive, EmailVerified,
ReferredByMemberId, ReferralSource,
StripeCustomerId, PayPalPayerId, GoCardlessCustomerId
```

### Key Indexes
| Table | Columns | Type |
|-------|---------|------|
| Club | Slug | UNIQUE |
| Member | (ClubId, Email) | Composite |
| Member | (ClubId, MemberNumber) | UNIQUE |
| Membership | (ClubId, Status) | Composite |
| Session | (ClubId, StartTime) | Composite |
| Payment | (ClubId, PaymentDate) | Composite |
| Invoice | (ClubId, InvoiceNumber) | UNIQUE |
| Fee | (ClubId, Code) | UNIQUE |

### Cascade Delete Rules
| Parent → Child | Behavior |
|---------------|----------|
| Club → ClubSettings | CASCADE |
| Club → Member | RESTRICT |
| Member → FamilyMember | CASCADE |
| Member → SessionBooking | RESTRICT |
| Member → Payment | RESTRICT |
| Session → SessionBooking | CASCADE |
| Event → EventTicket | CASCADE |
| MembershipType → Membership | RESTRICT |

### All Decimal Fields: DECIMAL(18,2) precision

---

## 9. ENUMERATIONS (Complete List)

### Core Enums
- **ClubType:** Cricket, Football, Rugby, Tennis, Golf, Hockey, Swimming, Athletics, MultiSport, CommunityGroup, YouthOrganization, Other
- **UserRole:** SuperAdmin, ClubManager, Member, Coach, Staff
- **MemberStatus:** Pending, Active, Expired, Suspended, Cancelled
- **Gender:** Male, Female, Other, PreferNotToSay
- **FamilyMemberRelation:** Spouse, Child, Sibling, Parent, Other

### Session/Booking Enums
- **SessionCategory:** AllAges, Juniors, Seniors, U7-U19, Ladies, Mens, Mixed, Beginners, Advanced, Social, Competition
- **BookingStatus:** Confirmed, Cancelled, NoShow, Attended

### Payment Enums
- **PaymentStatus:** Pending, Processing, Completed, Failed, Refunded, Cancelled
- **PaymentMethod:** Stripe, PayPal, BankTransfer, Cash, Cheque, Other
- **PaymentType:** Membership, EventTicket, SessionFee, Other
- **PaymentProvider:** Stripe, PayPal

### Membership Enums
- **MembershipPaymentType:** Annual, Monthly, PayAsYouGo
- **MembershipStatus:** Active, PendingPayment, Expired, Cancelled
- **MembershipCategory:** Individual, Family, Corporate, Student, Senior, Junior, Couple, Lifetime, Honorary, Trial, DayPass
- **BillingCycle:** Weekly, Fortnightly, Monthly, Quarterly, Biannual, Annual, Lifetime, OneTime

### Event Enums
- **EventType:** Social, Tournament, AGM, Training, Fundraiser, Competition, Meeting, Presentation, Other
- **EventStatus:** Draft, Published, RegistrationOpen/Closed, Upcoming, InProgress, Completed, Cancelled, Postponed, Archived
- **RSVPResponse:** Attending, NotAttending, Maybe

### Competition Enums
- **CompetitionType:** League, Tournament, Cup, Friendly, Championship, Qualifier, Playoff, RoundRobin, Ladder, TimeTrial
- **MatchStatus:** Scheduled, Confirmed, InProgress, Completed, Postponed, Cancelled, Walkover, Bye, Abandoned, Disputed
- **MatchResult:** NotPlayed, HomeWin, AwayWin, Draw, HomeWalkover, AwayWalkover

### Financial Enums
- **FeeType:** Membership, Registration, JoiningFee, Activity, ClassFee, EventParticipation, FacilityBooking, EquipmentRental, Coaching, LatePaymentPenalty, Insurance, GuestPass, Other
- **InvoiceStatus:** Draft, Sent, Viewed, PartiallyPaid, Paid, Overdue, Voided, Disputed, WrittenOff
- **RefundStatus:** Requested, Approved, Processing, Completed, Rejected, PartiallyRefunded, Failed
- **AccountCategory:** Asset, Liability, Equity, Revenue, Expense

### Facility/Equipment Enums
- **FacilityType:** Court, Pool, Field, Track, Gym, Studio, MeetingRoom, ChangingRoom, ClubHouse, Parking, Bar, Kitchen, Office
- **EquipmentCategory:** Sports, Training, Safety, Electronics, Furniture, Tools, Medical, Office, Audio
- **EquipmentCondition:** New, Excellent, Good, Fair, Poor, NeedsRepair, Damaged, Decommissioned
- **LoanStatus:** Requested, Approved, Active, Overdue, Returned, ReturnedDamaged, Lost

### Program Enums
- **ProgramType:** Course, Camp, Class, Workshop, Clinic, League, Academy, Squad, PrivateLesson, GroupLesson, Trial, Holiday
- **EnrollmentStatus:** Pending, Confirmed, WaitListed, Attended, Withdrawn, Transferred, Completed, Failed, Cancelled
- **SkillLevel:** Beginner, Elementary, Intermediate, UpperIntermediate, Advanced, Expert, AllLevels

---

## 10. FRONTEND ARCHITECTURE

### Angular App Configuration
```typescript
// app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideCharts(withDefaultRegisterables())
  ]
};
```

### Route Structure
**Root Routes (app.routes.ts):**
- `/` → redirect to `/auth/login`
- `/auth/*` → public (guestGuard prevents authenticated users)
- `/admin/*` → superAdminGuard + AdminLayoutComponent
- `/club/*` → clubManagerGuard + AdminLayoutComponent
- `/portal/*` → memberGuard + PortalLayoutComponent
- `/unauthorized` → UnauthorizedComponent
- `/**` → redirect to `/auth/login`

**Auth Routes:** login, register, forgot-password, reset-password

**Admin Routes:** dashboard, clubs (list/new/:id), users, reports, settings, system-config (payment/email/features/appearance/audit)

**Club Routes:** dashboard, members (list/new/:id), memberships (list/types), sessions (list/new/:id), events (list/new/:id), competitions (list/new/:id/:id/edit), payments (list/:id), venues, fees (list/new/:id), invoices (list/new/:id/:id/edit), reports, settings

**Portal Routes:** dashboard, sessions, events, payments, family, profile, settings

### Frontend Services (17)
api, auth, club, competition, event, fee, invoice, member,
membership, notification, payment, portal, report, session,
system-config, venue + index barrel

### Shared Components (6)
- **notification** — Toast notifications (success/error/warning/info)
- **loading-spinner** — Loading indicator
- **confirm-dialog** — Confirmation modal
- **pagination** — Page navigation with prev/next/page numbers
- **status-badge** — Colored badge (active=green, pending=yellow, expired=red)
- **empty-state** — "No data found" placeholder

### Shared Pipes (3)
- **date-format** — Date formatting
- **currency-format** — Currency display (GBP default)
- **truncate** — Text truncation

### Layouts (3)
- **AdminLayout** — Dark sidebar (w-64) + white header + main content area
- **PortalLayout** — Member-facing layout
- **Unauthorized** — Access denied page

### Component Patterns
- All standalone components (`standalone: true`)
- Angular 17+ control flow (`@if`, `@for`, `@else`)
- Signals for state (`signal<T[]>([])`, `computed()`)
- `inject()` for DI (not constructor injection)
- Reactive Forms with FormBuilder
- Tailwind utility classes for all styling

---

## 11. FEATURES (Complete Inventory)

### Authentication & User Management
- User registration with optional email verification
- JWT login with access + refresh tokens
- Password reset via email (forgot/reset flow)
- Change password (authenticated)
- Role-based navigation after login (SuperAdmin→admin, ClubManager→club, Member→portal)
- Token refresh on 401

### Club Management (SuperAdmin)
- Create/edit/deactivate clubs
- View club statistics and health
- Assign club managers
- System-wide dashboard (total clubs, members, revenue)
- System configuration (payment provider, email provider, feature flags, appearance, audit log)

### Member Management (ClubManager)
- Full CRUD with search, filter, pagination
- Member profiles: personal details, address, emergency contacts, medical info, social links
- Member number auto-generation (unique per club, format: "MBR-001")
- Family accounts with dependents (Spouse, Child, Sibling, Parent)
- Member status lifecycle: Pending → Active → Expired/Suspended/Cancelled
- Member notes (General, Medical, Payment, Behavior, Achievement, Communication, Internal)
- Custom fields per club (Text, Number, Date, Boolean, Select, MultiSelect, TextArea)
- Document uploads (ProfilePhoto, MedicalForm, ConsentForm, DBSCertificate)
- QR code data for member cards
- Referral tracking
- Application/onboarding workflow (Draft→Submitted→UnderReview→Approved/Rejected)

### Membership Management
- Configurable membership types per club (name, description, pricing, age limits)
- Multiple billing cycles: Weekly, Monthly, Quarterly, Annual, Lifetime, PayAsYouGo
- Joining fees, admin fees, session fees
- Membership enrollment with start/end dates
- Auto-renewal configuration
- Membership freeze/pause (with optional fee during freeze)
- Membership discounts (EarlyBird, Loyalty, Family, Corporate, PromoCode, Referral)
- Membership waitlist (position-based queue)
- Guest passes (with pass codes, pricing, conversion to member)
- Membership cancellation with reasons and fees

### Session & Booking Management
- Create individual sessions (title, category, venue, capacity, fee)
- Recurring schedule templates (day of week, time, auto-generate sessions)
- Session categories: AllAges, Juniors, Seniors, U7-U19, Ladies, Mens, Mixed, etc.
- Capacity management with real-time booking count
- Member booking (self or family member)
- Booking cancellation with deadline enforcement
- Waitlist management (position-based, auto-notify on cancellation)
- Attendance tracking and check-in
- Session cancellation with reason

### Event Management
- Event types: Social, Tournament, AGM, Training, Fundraiser, Competition, Meeting
- Ticketed events (standard + member pricing, early bird)
- RSVP events (Attending/NotAttending/Maybe with guest count)
- Event registration with emergency contacts
- Capacity management
- Event series (recurring events)
- Event sessions (multi-session events)
- QR code tickets and check-in
- Event cancellation/postponement

### Competition & Tournament Management
- Competition types: League, Tournament, Cup, Friendly, RoundRobin, Knockout
- Season management
- Team registration with squad management
- Captain assignment, team colors, home venue
- Match scheduling and fixture generation
- Match result recording (scores, events, lineups)
- Match events (goals, cards, substitutions)
- Auto-calculated league standings (points, GD, form)
- Competition rounds

### Payment & Financial Management
- Manual payment recording (Cash, BankTransfer, Cheque)
- Stripe integration (payment intents)
- PayPal integration (structure ready)
- Payment types: Membership, EventTicket, SessionFee, Other
- Refund processing (full/partial)
- Invoice generation with line items
- Invoice lifecycle: Draft → Sent → Viewed → Paid/Overdue/Voided
- Payment plans with installments
- Member balance tracking (credit/outstanding/aging)
- Payment reminders (automated, configurable frequency)
- Fee configuration (one-time, recurring, per-session)
- Late payment penalties
- Receipt generation

### Advanced Financial (Phases 5-6)
- Chart of accounts (Asset, Liability, Equity, Revenue, Expense)
- Journal entries (double-entry bookkeeping)
- Fiscal years and periods
- Tax rates and tax calculations
- Bank reconciliation
- Budget management (budgeted vs actual)
- Financial audit log
- Saved financial reports
- Vendor management
- Expense tracking with approval workflow
- Purchase orders with receipts
- Expense categories and cost centers

### Facility Management (Phase 7)
- Venue operating schedules and holidays
- Facility types: Court, Pool, Field, Track, Gym, Studio, etc.
- Facility availability and pricing rules
- Facility booking with member/non-member rates
- Peak/off-peak pricing
- Facility maintenance scheduling
- Facility blockouts

### Equipment Management (Phase 8)
- Equipment inventory (category, condition, location, value)
- Equipment loans to members
- Loan fees and deposits
- Equipment reservations
- Equipment maintenance tracking
- Depreciation tracking

### Programs & Activities (Phase 9)
- Program types: Course, Camp, Class, Workshop, Academy, etc.
- Program sessions with instructors
- Enrollment management (capacity, waitlist, pricing)
- Attendance tracking per session
- Member certificates and qualifications
- Skill levels and age groups

### Reporting & Analytics
- Membership statistics (by status, type, age, growth trends)
- Financial summary (revenue by type/method, outstanding, monthly trends)
- Attendance reports (session/event, no-show tracking)
- Club analytics snapshots (periodic KPI capture)

### Communication
- Email notifications via SendGrid (or Mock for dev)
- Email types: Welcome, PasswordReset, PaymentReminder, BookingConfirmation, etc.
- Communication templates per club
- Bulk email campaigns
- Email logging with status tracking

### Member Portal (Self-Service)
- Personal dashboard (membership status, upcoming bookings/events, balance)
- Browse and book available sessions
- View and register for events
- Payment history
- Family member management
- Profile editing (personal info, address, emergency contacts, medical)
- Notification preferences

---

## 12. DEMO/SEED DATA

### System Configuration
- Payment Provider: Mock (1500ms delay, 0% failure rate)
- Email Provider: Mock (500ms delay)
- Platform Name: "The League"
- Primary Color: #6366f1

### Demo Clubs (4)
| Club | Type | Primary Color |
|------|------|--------------|
| Teddington Cricket Club | Cricket | #1e3a5f |
| Highbury United FC | Football | #dc2626 |
| Richmond Hockey Club | Hockey | #16a34a |
| Marylebone Cricket Club | Cricket | #1d4ed8 |

### Demo Credentials
| Role | Email | Password |
|------|-------|----------|
| Super Admin | admin@theleague.com | Admin123! |
| Club Manager (Teddington) | chairman@teddingtoncc.com | Chairman123! |
| Club Manager (Highbury) | chairman@highburyunited.com | Chairman123! |
| Club Manager (Richmond) | president@richmondhockey.org.uk | President123! |
| Club Manager (Marylebone) | chairman@marylebonecc.com | Chairman123! |
| Member (Teddington) | james.anderson@email.com | Member123! |
| Member (Highbury) | marcus.rashford@email.com | Member123! |
| Member (Richmond) | sam.ward@email.com | Member123! |

### Membership Types per Club
- **Teddington CC:** Senior Playing (£350), Junior Playing (£150), Family (£650), Social (£75)
- **Highbury FC:** Senior Player (£280), Youth Player (£120), Women's Player (£200), Veterans (£180), Social (£50)
- **Richmond HC:** Senior Playing (£420), Junior Player (£180), Masters (£320), Student (£250), Social (£80)
- **Marylebone CC:** Full Playing (£400), Junior (£175), Social (£150), Corporate (£1500)

### Venues (14 total, 3-4 per club)
Each club has: Primary ground/pitch, Practice facilities, Clubhouse/Pavilion

---

## 13. DEPLOYMENT & CONFIGURATION

### Development Setup
```bash
# Backend
dotnet restore
cd TheLeague.Infrastructure && dotnet ef database update -s ../TheLeague.Api
cd TheLeague.Api && dotnet run  # → http://localhost:7000

# Frontend
cd the-league-client && npm install && npm start  # → http://localhost:4200
```

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TheLeagueDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": { "Secret": "...", "Issuer": "TheLeague", "Audience": "TheLeagueApp" },
  "Auth": { "RequireEmailVerification": false },
  "MockPayments": { "Stripe": { "PublishableKey": "pk_test_mock_key" } },
  "MockEmail": { "FromEmail": "noreply@theleague.com" },
  "Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:7000" } } }
}
```

### Production Deployment
- **Backend:** Azure App Service or Docker container
- **Frontend:** Azure Static Web Apps / CDN
- **Database:** Azure SQL
- **Environment Variables:** ConnectionStrings__DefaultConnection, Jwt__Secret, ASPNETCORE_ENVIRONMENT

### Docker Support
Multi-stage Dockerfile: SDK build → aspnet runtime → expose port 80

---

## 14. DESIGN PATTERNS & CONVENTIONS

### Backend Patterns
- **Clean Architecture:** Api → Core → Infrastructure (dependency inversion)
- **Service Layer:** Controllers are thin, services contain all business logic
- **Provider/Factory Pattern:** Swappable payment (Stripe/PayPal/Mock) and email (SendGrid/Mock)
- **DTO Pattern:** CreateXxxRequest, UpdateXxxRequest, XxxListDto, XxxDetailDto
- **Global Exception Handling:** Middleware maps exceptions to HTTP status codes
- **Scoped DI:** All services registered as Scoped (per-request)
- **Async/Await:** All I/O operations are async
- **Soft Deletes:** IsActive flag on most entities

### Frontend Patterns
- **Standalone Components:** No NgModules, direct imports
- **Lazy Loading:** All feature routes use `loadComponent()` / `loadChildren()`
- **Signals:** `signal<T>()` for component state, `computed()` for derived values
- **BehaviorSubject:** For auth state (currentUser$)
- **Functional Guards:** `CanActivateFn` with `inject()`
- **Functional Interceptors:** `HttpInterceptorFn`
- **Base API Service:** All HTTP calls go through ApiService wrapper
- **Tailwind Utility Classes:** No custom CSS files per component
- **Mobile-First Responsive:** `grid-cols-1 md:grid-cols-2 lg:grid-cols-4`

### Naming Conventions
- **Entities:** PascalCase singular (Member, Session, Payment)
- **Controllers:** PascalCase plural + "Controller" (MembersController)
- **Services:** PascalCase singular + "Service" (MemberService)
- **DTOs:** PascalCase + purpose suffix (MemberCreateRequest, MemberListDto)
- **Angular Components:** kebab-case files, PascalCase classes
- **Routes:** lowercase plural (/members, /sessions, /events)
- **Enums:** PascalCase (MemberStatus.Active)

---

## 15. KEY BUSINESS RULES

1. **Email uniqueness** — per club (same person can be in multiple clubs)
2. **Member number** — auto-generated, unique per club (format: MBR-001)
3. **Session capacity** — enforced at booking time with transaction
4. **Booking cancellation** — deadline enforcement (configurable hours before)
5. **Waitlist** — position-based, auto-notify when spot opens
6. **Membership overlap** — prevented by application logic
7. **Payment recording** — updates membership status when linked
8. **Invoice numbering** — unique per club (format: INV-YYYY-NNNN)
9. **Tenant isolation** — EVERY query filters by ClubId, no cross-tenant access
10. **Soft deletes** — IsActive flag, data preserved for history
11. **Currency** — GBP default, configurable per entity
12. **Competition standings** — auto-calculated from match results (configurable points)

---

*Document Version: 1.0 — Generated from full codebase analysis*
*This document is self-contained and sufficient to rebuild the entire application.*
