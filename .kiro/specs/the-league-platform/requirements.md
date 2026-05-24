# Requirements Document

## Introduction

The League is a multi-tenant SaaS platform for managing sports club memberships, sessions, events, payments, and competitions. It serves Cricket, Football, Hockey, Rugby, Tennis, Swimming, Athletics, Golf clubs and more. This rebuild modernises the platform with production-grade enhancements including platform subscription billing, self-service onboarding, real-time notifications, audit/compliance, revenue features, operational resilience, analytics, and an integration ecosystem.

### Target Technology Stack
- **Backend:** ASP.NET Core (latest), EF Core, C# — Modular Monolith with Clean Architecture per module, CQRS pattern
- **Frontend:** Angular 20, Tailwind CSS v4, DaisyUI, advanced reusable component system
- **Infrastructure:** SQL Server, Redis, Azure Blob Storage, SignalR, background job processing (Hangfire/Quartz)

## Glossary

- **Platform**: The League SaaS application as a whole
- **Tenant**: A single sports club operating within the Platform
- **Club**: Synonym for Tenant; the organisational unit of data isolation
- **ClubId**: The GUID discriminator column used for multi-tenant data isolation
- **SuperAdmin**: Platform-wide administrator with access to all clubs and system configuration
- **ClubManager**: Administrator of a single club with full CRUD on that club's data
- **Member**: End-user belonging to a club who accesses self-service features
- **Coach**: Club staff with limited access to session management
- **Staff**: Club personnel with read-only access
- **Subscription_Tier**: The pricing plan a club subscribes to (Free, Starter, Pro, Enterprise)
- **Feature_Gate**: A mechanism that enables or disables platform features based on Subscription_Tier
- **Onboarding_Wizard**: A guided multi-step flow for new clubs to configure their account
- **Dunning**: The automated process of retrying failed subscription payments and notifying clubs
- **Audit_Trail**: An immutable log of all data mutations for compliance purposes
- **CQRS**: Command Query Responsibility Segregation — separating read and write operations
- **Module**: A self-contained vertical slice of the application with its own domain, data access, and API
- **SignalR_Hub**: A real-time communication endpoint using WebSockets
- **Correlation_ID**: A unique identifier that traces a request across all system layers
- **Health_Check**: An endpoint that reports the operational status of system components
- **Match_Centre**: The real-time match viewing interface for live scoring and commentary
- **Document_Service**: The service responsible for file upload, storage, and retrieval
- **Shop_Service**: The service managing merchandise listings, orders, and fulfilment
- **Circuit_Breaker**: A resilience pattern that stops calling a failing service after repeated failures
- **PWA**: Progressive Web App — a web application installable on devices with offline capabilities
- **SSR**: Server-Side Rendering — pre-rendering pages on the server for SEO and initial load performance

## Requirements

### Requirement 1: Multi-Tenancy and Data Isolation

**User Story:** As a ClubManager, I want my club's data to be completely isolated from other clubs, so that no cross-tenant data leakage occurs.

#### Acceptance Criteria

1. THE Platform SHALL store all tenant-scoped entities with a ClubId discriminator column of type GUID
2. WHEN an authenticated request is received from a user whose JWT contains a ClubId claim, THE Platform SHALL extract and validate the ClubId as a well-formed GUID before processing the request
3. WHILE a tenant-scoped request is being processed, THE Platform SHALL apply global query filters to all read and write operations ensuring only data matching the current ClubId is accessible
4. IF a request attempts to access or modify data belonging to a different Tenant, THEN THE Platform SHALL reject the request with a 403 Forbidden response and return an error message indicating insufficient tenant access
5. WHEN a new Club is created, THE Platform SHALL generate a unique ClubId (GUID) for that Tenant and persist it as the discriminator value for all subsequent data created under that Club
6. IF an authenticated user's JWT does not contain a ClubId claim or contains a malformed ClubId value, THEN THE Platform SHALL reject tenant-scoped requests with a 403 Forbidden response and return an error message indicating missing or invalid tenant context
7. WHILE a SuperAdmin user is authenticated, THE Platform SHALL bypass tenant query filters to allow cross-tenant data access for platform administration purposes

### Requirement 2: Authentication and Token Management

**User Story:** As a user, I want to securely authenticate and maintain my session, so that my account is protected and I can access the platform without repeated logins.

#### Acceptance Criteria

1. WHEN a user submits valid credentials, THE Auth_Service SHALL issue a JWT access token (15-minute expiry) and a refresh token (7-day expiry)
2. WHEN a user submits invalid credentials, THE Auth_Service SHALL return a 401 Unauthorized response without revealing which credential was incorrect
3. WHEN an access token expires and the user presents a valid, non-revoked refresh token, THE Auth_Service SHALL issue a new access token and a new refresh token, invalidating the consumed refresh token
4. IF a user presents an expired, already-used, or otherwise invalid refresh token, THEN THE Auth_Service SHALL return a 401 Unauthorized response and revoke all refresh tokens for that user
5. THE Auth_Service SHALL include the following claims in the JWT: sub, email, name, role, clubId, memberId, jti
6. WHEN a user requests a password reset, THE Auth_Service SHALL send a single-use, time-limited reset token (valid for 60 minutes) to the registered email address
7. WHEN a user registers, THE Auth_Service SHALL send a verification email containing a verification link valid for 24 hours
8. IF an unverified user attempts to authenticate, THEN THE Auth_Service SHALL return a 403 Forbidden response indicating that email verification is required
9. WHEN a user fails authentication 5 consecutive times, THE Auth_Service SHALL lock the account for 15 minutes and send a notification email to the account owner
10. WHEN a user's account is locked, THE Auth_Service SHALL reject all authentication attempts with a 403 Forbidden response and include the remaining lockout duration in seconds in the response body
11. THE Auth_Service SHALL track active sessions per user and allow users to view session metadata (device identifier, last active timestamp, and IP address) and revoke any individual session
12. WHEN a user revokes a session, THE Auth_Service SHALL invalidate all tokens associated with that session so that subsequent requests using those tokens return a 401 Unauthorized response within 30 seconds

### Requirement 3: Role-Based Access Control

**User Story:** As a platform operator, I want users to have role-appropriate access, so that sensitive operations are restricted to authorised personnel.

#### Acceptance Criteria

1. THE Platform SHALL enforce exactly five user roles: SuperAdmin, ClubManager, Member, Coach, Staff
2. IF a user is authenticated with the SuperAdmin role, THEN THE Platform SHALL grant access to all clubs' data, system configuration, user management, and platform-wide reporting
3. IF a user is authenticated with the ClubManager role, THEN THE Platform SHALL grant full create, read, update, and delete access only to data belonging to their assigned club
4. IF a user is authenticated with the Member role, THEN THE Platform SHALL grant access only to their own profile, session bookings, payment history, event registration, and family member management within their assigned club
5. IF a user is authenticated with the Coach role, THEN THE Platform SHALL grant create, read, and update access to sessions and attendance records within their assigned club, and read-only access to member profiles within that club
6. IF a user is authenticated with the Staff role, THEN THE Platform SHALL grant read-only access to all data within their assigned club
7. IF an authenticated user attempts to access a resource outside their role permissions, THEN THE Platform SHALL deny the request and return an access-denied error response within 500 milliseconds
8. IF a user authenticates without a recognised role assignment, THEN THE Platform SHALL deny access to all protected resources and return an access-denied error response
9. WHEN a user's role is changed by an administrator, THEN THE Platform SHALL enforce the updated permissions on the user's next request after the role change is saved

### Requirement 4: Platform Subscription and Billing

**User Story:** As a platform operator, I want clubs to subscribe to tiered pricing plans, so that the platform generates recurring revenue and features are gated appropriately.

#### Acceptance Criteria

1. THE Subscription_Service SHALL support four tiers: Free (£0/month), Starter (£29/month), Pro (£79/month), Enterprise (£199/month)
2. WHEN a Club subscribes to a tier, THE Subscription_Service SHALL activate the Feature_Gates corresponding to that tier within 30 seconds of subscription confirmation
3. WHEN a Club's subscription payment fails, THE Dunning system SHALL retry the payment at 1, 3, and 7 days after failure
4. IF all Dunning retry attempts fail, THEN THE Subscription_Service SHALL downgrade the Club to the Free tier and notify the ClubManager via email within 24 hours of the final failed retry
5. WHEN a ClubManager upgrades their subscription tier, THE Subscription_Service SHALL apply the new tier's Feature_Gates within 30 seconds and charge a prorated amount calculated as the price difference between the new and old tier multiplied by the remaining days in the current billing period divided by the total days in the billing period
6. WHEN a ClubManager downgrades their subscription tier, THE Subscription_Service SHALL apply the downgrade at the end of the current billing period and confirm the scheduled downgrade date to the ClubManager via email
7. THE Subscription_Service SHALL enforce the following usage limits per tier: Free (50 members, 1 GB storage, 0 SMS credits per month), Starter (200 members, 5 GB storage, 500 SMS credits per month), Pro (1000 members, 25 GB storage, 2000 SMS credits per month), Enterprise (unlimited members, 100 GB storage, 10000 SMS credits per month)
8. IF a Club reaches a usage limit for their current tier, THEN THE Subscription_Service SHALL block the action that would exceed the limit and display an error message indicating the limit reached and the option to upgrade
9. WHEN a new Club signs up for any paid tier, THE Subscription_Service SHALL provide a 14-day free trial with Pro tier Feature_Gates and usage limits active, regardless of the selected paid tier, before requiring payment
10. WHEN a trial period expires without payment details being provided, THE Subscription_Service SHALL downgrade the Club to the Free tier and send a conversion reminder email at day 14, day 21, and day 28 after trial expiry
11. WHEN a ClubManager provides valid payment details during the trial period, THE Subscription_Service SHALL activate the originally selected paid tier's Feature_Gates and begin billing on the day after the 14-day trial ends

### Requirement 5: Self-Service Club Onboarding

**User Story:** As a new club administrator, I want to sign up and configure my club without manual intervention, so that I can start using the platform immediately.

#### Acceptance Criteria

1. WHEN a visitor accesses the public landing page, THE Platform SHALL display subscription tiers with feature comparisons and a sign-up call-to-action
2. WHEN a visitor selects a paid tier and completes Stripe checkout, THE Onboarding_Wizard SHALL create the Club, assign the ClubManager role, and activate the subscription within 10 seconds of successful payment confirmation
3. WHEN a visitor selects the Free tier, THE Onboarding_Wizard SHALL create the Club and assign the ClubManager role without requiring payment details
4. IF Stripe checkout fails or the visitor abandons the checkout session, THEN THE Onboarding_Wizard SHALL display an error message indicating the payment was not completed and allow the visitor to retry or select a different tier without losing previously entered information
5. WHEN a new Club is created, THE Onboarding_Wizard SHALL present the ClubManager with sequential steps: club profile setup, membership type configuration, first member import, and payment provider connection, where each step can be completed or skipped and the ClubManager can return to any skipped step later from the club settings
6. WHEN a ClubManager uploads a CSV or Excel file during onboarding, THE Import_Service SHALL accept files up to 5 MB containing a maximum of 2000 rows, validate each row against required Member fields (FirstName, LastName, Email), parse valid rows, create Member records, and return a summary indicating the count of successfully imported rows and the count of rejected rows
7. IF the CSV/Excel file contains rows with missing required fields, duplicate email addresses within the file or existing in the Club, or values exceeding field length limits, THEN THE Import_Service SHALL reject those rows, import all valid rows, and return an error report listing each rejected row number, the field that failed validation, and the reason for rejection
8. IF the uploaded file exceeds 5 MB, contains more than 2000 rows, or is not in CSV or Excel format, THEN THE Import_Service SHALL reject the entire file and display an error message indicating the specific constraint that was violated

### Requirement 6: Member Management

**User Story:** As a ClubManager, I want to manage all aspects of club membership, so that I can maintain accurate member records and streamline administration.

#### Acceptance Criteria

1. THE Member_Service SHALL support Create, Read, Update, and Delete operations on Member records with search by name or email, filtering by status, and pagination with a default page size of 20 and a maximum page size of 100
2. WHEN a new Member is created, THE Member_Service SHALL auto-generate a unique member number in the format "MBR-{sequential padded to 3 digits minimum}" (e.g., MBR-001, MBR-012, MBR-999, MBR-1000) scoped to the Club
3. THE Member_Service SHALL store member profiles including: first name, last name, email, phone, date of birth, gender, address (line 1, line 2, city, county, postcode, country), emergency contact (name, phone, relation), medical information (conditions, allergies, doctor name, doctor phone, blood type), social links (Facebook, Twitter, Instagram, LinkedIn), and custom field values
4. WHEN a ClubManager creates a family account, THE Member_Service SHALL link up to 10 dependent members to a primary member with a relationship type of Spouse, Child, Sibling, or Parent
5. THE Member_Service SHALL enforce the member status lifecycle permitting only the following transitions: Pending → Active, Active → Expired, Active → Suspended, Active → Cancelled, Suspended → Active, and Expired → Active
6. IF a status transition is attempted that is not in the permitted set, THEN THE Member_Service SHALL reject the request and return an error message indicating the current status and the invalid target status
7. WHEN a Member's status changes, THE Member_Service SHALL record the transition including the previous status, new status, timestamp, and the identifier of the actor who performed the change
8. WHEN a ClubManager defines custom fields for their Club, THE Member_Service SHALL store and validate custom field values as JSON on each Member record, enforcing the declared field type (Text, Number, Date, Boolean, Select, MultiSelect, TextArea) and rejecting values that do not conform to the field type definition
9. IF a required custom field has no value provided when creating or updating a Member, THEN THE Member_Service SHALL reject the request and return an error message indicating which required field is missing

### Requirement 7: Membership Management

**User Story:** As a ClubManager, I want to configure and manage membership types with flexible billing, so that I can offer appropriate plans to different member segments.

#### Acceptance Criteria

1. THE Membership_Service SHALL allow ClubManagers to create membership types with: name (1–100 characters), description (0–500 characters), pricing (0.00 to 999,999.99), billing cycle, minimum age (0–120), maximum age (0–120 where maximum >= minimum), and capacity (1–10,000 members)
2. THE Membership_Service SHALL support billing cycles: Weekly, Fortnightly, Monthly, Quarterly, Biannual, Annual, Lifetime, OneTime, PayAsYouGo
3. WHEN a Member enrols in a membership, THE Membership_Service SHALL validate that the Member's age falls within the membership type's configured age limits, record the start date, calculate the end date based on the billing cycle, and record the auto-renewal preference
4. IF a Member attempts to enrol in a membership type and their age is outside the configured age limits, THEN THE Membership_Service SHALL reject the enrolment and return an error message indicating the age restriction
5. WHEN a membership reaches its end date with auto-renewal enabled, THE Membership_Service SHALL automatically renew the membership for the next billing cycle and trigger a payment request
6. IF a payment fails during auto-renewal, THEN THE Membership_Service SHALL set the membership status to PendingPayment, retain the membership record, and notify the Member via email that payment has failed
7. WHEN a Member requests a membership freeze, THE Membership_Service SHALL pause the membership for the specified duration (1–365 days) and apply the freeze fee configured on the membership type (0.00 if no fee is configured)
8. THE Membership_Service SHALL support discounts: EarlyBird, Loyalty, Family, Corporate, PromoCode, Referral — each with a configurable percentage (0.01–100.00%) or fixed amount (0.01–999,999.99), a validity start date, and a validity end date
9. WHEN a membership type reaches its configured capacity, THE Membership_Service SHALL place new applicants on a position-based waitlist ordered by request time and send an email notification to the applicant when a slot becomes available

### Requirement 8: Session and Booking Management

**User Story:** As a ClubManager, I want to schedule sessions and manage bookings, so that members can attend activities and capacity is controlled.

#### Acceptance Criteria

1. THE Session_Service SHALL allow creation of sessions with: title (1–100 characters), category (from SessionCategory enum), venue (existing Venue reference), date/time, duration (15–480 minutes), capacity (1–500 participants), and fee (0.00–9999.99 in club currency)
2. THE Session_Service SHALL support recurring schedule templates that auto-generate sessions based on day-of-week and time patterns for a configurable horizon of up to 12 weeks ahead
3. WHEN a Member books a session and remaining capacity is greater than zero, THE Booking_Service SHALL confirm the booking with status Confirmed and decrement the remaining capacity by one
4. IF a session is at full capacity when a Member attempts to book, THEN THE Booking_Service SHALL place the Member on a position-based waitlist (maximum 50 entries per session) and return the Member's waitlist position
5. WHEN a booking is cancelled and a waitlist exists for that session, THE Booking_Service SHALL automatically offer the slot to the next member on the waitlist and allow 24 hours for the member to accept before offering to the subsequent member
6. WHEN a Member attempts to cancel a booking after the cancellation deadline (configurable per session, default 24 hours before session start time), THE Booking_Service SHALL reject the cancellation and return a message indicating the deadline has passed
7. THE Session_Service SHALL support attendance tracking with statuses: Confirmed, Attended, NoShow, Cancelled
8. IF a waitlist offer expires without acceptance within the 24-hour window, THEN THE Booking_Service SHALL remove the member from the waitlist position and offer the slot to the next member in queue

### Requirement 9: Event Management

**User Story:** As a ClubManager, I want to create and manage events with ticketing and RSVP, so that members can participate in club activities beyond regular sessions.

#### Acceptance Criteria

1. THE Event_Service SHALL support event types: Social, Tournament, AGM, Training, Fundraiser, Competition, Meeting, Presentation, Other
2. THE Event_Service SHALL support both ticketed events (with standard pricing and member pricing, each between 0.01 and 999,999.99) and RSVP events (with responses: Attending, NotAttending, Maybe)
3. WHEN a Member purchases an event ticket, THE Event_Service SHALL generate a unique QR code associated with the ticket for check-in and return the QR code data in the purchase confirmation response
4. IF a Member attempts to register for an event that has reached its capacity limit, THEN THE Event_Service SHALL reject the registration and return a response indicating the event is full
5. THE Event_Service SHALL support event series (recurring events with a maximum of 52 occurrences per series) and multi-session events (with a maximum of 20 sessions per event)
6. WHEN an event is cancelled, THE Event_Service SHALL notify all registered attendees within 60 seconds and initiate refund processing for all ticketed registrations associated with the cancelled event
7. THE Event_Service SHALL manage the event lifecycle through the following states: Draft, Published, RegistrationOpen, RegistrationClosed, InProgress, Completed, Cancelled, Postponed — where only valid transitions are permitted (Draft → Published → RegistrationOpen → RegistrationClosed → InProgress → Completed, and any active state → Cancelled or Postponed)
8. WHEN a ClubManager creates an event, THE Event_Service SHALL require at minimum: event title (1 to 200 characters), event type, start date-time, and end date-time (which must be after start date-time)
9. IF a Member cancels their event registration more than 48 hours before the event start time, THEN THE Event_Service SHALL remove the registration and initiate a refund for ticketed events; IF cancellation is within 48 hours, THEN THE Event_Service SHALL reject the cancellation request

### Requirement 10: Competition and Tournament Management

**User Story:** As a ClubManager, I want to organise competitions with teams, fixtures, and standings, so that members can participate in structured competitive play.

#### Acceptance Criteria

1. THE Competition_Service SHALL support competition types: League, Tournament, Cup, Friendly, RoundRobin, Knockout, Championship
2. THE Competition_Service SHALL manage seasons with start/end dates where the end date is after the start date, season name is at most 100 characters, and associate competitions to seasons
3. WHEN a competition is created, THE Competition_Service SHALL allow team registration with a squad of 11 to 30 players per team, captain assignment (exactly one captain per team), team name of at most 100 characters, and home venue selection from existing club venues
4. WHEN a ClubManager requests fixture generation for a competition with at least 2 registered teams, THE Competition_Service SHALL generate fixtures based on competition type (round-robin: each team plays every other team; knockout: single-elimination bracket with byes assigned if team count is not a power of 2)
5. IF fixture generation is requested for a competition with fewer than 2 registered teams, THEN THE Competition_Service SHALL reject the request with an error message indicating insufficient teams
6. WHEN a match result is recorded, THE Competition_Service SHALL automatically update league standings awarding 3 points for a win, 1 point for a draw, and 0 points for a loss, calculating goal difference (goals scored minus goals conceded), and tracking form as results of the last 5 completed matches
7. WHEN a match status is set to Walkover, THE Competition_Service SHALL award the non-forfeiting team a win (3 points) and record the match with a default score as configured for the competition
8. THE Competition_Service SHALL support match events: goals, cards (yellow and red), substitutions (maximum 5 per team per match), and player lineups (exactly 11 starting players per team)
9. THE Competition_Service SHALL manage match statuses: Scheduled, Confirmed, InProgress, Completed, Postponed, Cancelled, Walkover, Abandoned, where valid transitions are Scheduled to Confirmed or Cancelled, Confirmed to InProgress or Postponed or Cancelled, InProgress to Completed or Abandoned, and Postponed to Confirmed or Cancelled

### Requirement 11: Payment and Financial Management

**User Story:** As a ClubManager, I want to process payments, generate invoices, and track financial data, so that the club's finances are managed accurately.

#### Acceptance Criteria

1. THE Payment_Service SHALL accept and record payments using the following methods: Stripe, PayPal, GoCardless Direct Debit, BankTransfer, Cash, and Cheque
2. WHEN a payment is processed via Stripe, THE Payment_Service SHALL use Stripe Connect so the Platform collects a transaction fee configurable by the SuperAdmin within the range of 1% to 2% of the transaction amount
3. IF a payment processing attempt fails, THEN THE Payment_Service SHALL set the payment status to Failed, record the failure reason, and display an error message indicating the cause of failure to the ClubManager
4. THE Invoice_Service SHALL generate invoices containing between 1 and 50 line items and manage the invoice lifecycle through the following statuses: Draft → Sent → Viewed → PartiallyPaid → Paid → Overdue → Voided
5. WHEN an invoice's due date has passed and the invoice status is not Paid or Voided, THE Payment_Service SHALL transition the invoice status to Overdue and send automated payment reminders at intervals configurable between 1 and 90 days, with a maximum of 10 reminders per invoice
6. THE Payment_Service SHALL support payment plans with scheduled installments, tracking each installment through statuses of Pending, Processing, Completed, Failed, Refunded, and Cancelled
7. THE Payment_Service SHALL maintain a member balance ledger tracking credits, debits, and outstanding amounts with DECIMAL(18,2) precision
8. WHEN a refund is requested, THE Payment_Service SHALL process full or partial refunds, deduct the refunded amount from the member's credit balance, and add the refunded amount to the member's outstanding balance
9. IF a refund processing attempt fails, THEN THE Payment_Service SHALL set the refund status to Failed, retain the original member balance unchanged, and display an error message indicating the failure reason
10. THE Financial_Service SHALL support double-entry bookkeeping with chart of accounts, journal entries, fiscal years, and tax rates, ensuring every journal entry has balanced debit and credit totals before it can be saved

### Requirement 12: Facility Management

**User Story:** As a ClubManager, I want to manage club facilities and bookings, so that resources are allocated efficiently and members can reserve spaces.

#### Acceptance Criteria

1. THE Facility_Service SHALL support facility types: Court, Pool, Field, Track, Gym, Studio, MeetingRoom, ChangingRoom, ClubHouse
2. THE Facility_Service SHALL allow a ClubManager to create, update, and delete operating schedules (defining available days and time ranges), holidays, and blockout periods per facility
3. WHEN a Member submits a facility booking request with a valid facility, date, start time, and duration between 30 minutes and 4 hours in increments of 30 minutes, THE Facility_Service SHALL confirm the booking and return a booking reference if the requested slot falls within operating hours and does not overlap with existing bookings, maintenance windows, or blockout periods
4. THE Facility_Service SHALL support peak and off-peak pricing per facility, where peak and off-peak time ranges are configured per operating schedule, with separate member and non-member rates applied based on the booker's membership status
5. THE Facility_Service SHALL track maintenance schedules per facility and automatically block new bookings that overlap with a maintenance window
6. IF a facility booking conflicts with an existing booking, maintenance window, or blockout period, THEN THE Facility_Service SHALL reject the booking and suggest up to 3 alternative available time slots of the same duration within the same day
7. WHEN a ClubManager schedules a maintenance window that overlaps with existing confirmed bookings, THE Facility_Service SHALL cancel the affected bookings and notify the affected Members
8. THE Facility_Service SHALL restrict advance booking to no more than 30 days in the future from the current date

### Requirement 13: Equipment Management

**User Story:** As a ClubManager, I want to track equipment inventory and loans, so that club assets are accounted for and members can borrow items.

#### Acceptance Criteria

1. THE Equipment_Service SHALL maintain an inventory with: name (maximum 200 characters), category (from EquipmentCategory enum), condition (from EquipmentCondition enum), location (maximum 200 characters), purchase date, value (DECIMAL 0.00 to 999,999,999.99), and annual depreciation rate (percentage from 0 to 100)
2. WHEN a Member requests an equipment loan, THE Equipment_Service SHALL verify the equipment condition is not NeedsRepair, Damaged, or Decommissioned and that no overlapping active loan or approved reservation exists, and record the loan with LoanStatus set to Requested, expected return date (no more than 90 days from loan start), fee, and deposit (each DECIMAL 0.00 to 999,999,999.99)
3. IF a Member requests a loan for equipment that is currently loaned, reserved for the requested period, or in a non-loanable condition (NeedsRepair, Damaged, Decommissioned), THEN THE Equipment_Service SHALL reject the request and return an error message indicating the reason for unavailability
4. WHEN the current date exceeds the expected return date of an active loan, THE Equipment_Service SHALL update the LoanStatus to Overdue, send a notification to the Member, and flag the loan for ClubManager review by marking it as requiring action
5. THE Equipment_Service SHALL support equipment reservations for future dates up to 365 days in advance, and IF a reservation overlaps with an existing approved reservation or active loan for the same equipment, THEN THE Equipment_Service SHALL reject the reservation and return an error message indicating the scheduling conflict
6. THE Equipment_Service SHALL track maintenance history including maintenance date, description (maximum 1000 characters), and resulting condition change, recording each condition transition over the equipment lifecycle

### Requirement 14: Program and Activity Management

**User Story:** As a ClubManager, I want to create structured programs (courses, camps, academies), so that members can enrol in progressive learning activities.

#### Acceptance Criteria

1. THE Program_Service SHALL allow creation, retrieval, update, and deletion of programs with a required program type from: Course, Camp, Class, Workshop, Clinic, Academy, Squad, PrivateLesson, GroupLesson
2. THE Program_Service SHALL allow creation, retrieval, update, and deletion of program sessions, each requiring an assigned instructor, a start date and time, an end date and time, a venue, and a maximum capacity between 1 and 200 participants
3. WHEN a Member enrols in a program and the current enrolment count is below the program capacity, THE Program_Service SHALL confirm the enrolment and record the enrolment date
4. IF a Member enrols in a program and the current enrolment count has reached the program capacity, THEN THE Program_Service SHALL place the Member on a waitlist in first-come-first-served order, up to a maximum of 50 waitlisted members
5. IF a Member enrols in a program and the waitlist has reached its maximum of 50 members, THEN THE Program_Service SHALL reject the enrolment and return an error message indicating the program is full
6. THE Program_Service SHALL track attendance per program session and calculate the completion rate as the percentage of attended sessions divided by total scheduled sessions for that program
7. WHEN a Member has attended at least 80% of a program's total scheduled sessions and the program end date has passed, THE Program_Service SHALL issue a certificate containing the program name, completion date, and the skill level assigned to the program
8. THE Program_Service SHALL require each program to be assigned a skill level from: Beginner, Elementary, Intermediate, UpperIntermediate, Advanced, Expert

### Requirement 15: Real-Time Notifications

**User Story:** As a Member, I want to receive timely notifications through my preferred channels, so that I stay informed about bookings, events, and club updates.

#### Acceptance Criteria

1. THE Notification_Service SHALL support channels: in-app (SignalR WebSocket), push notification (PWA), email (SendGrid), SMS (Twilio)
2. WHEN a notification-triggering event occurs (booking confirmation, booking cancellation, event registration, event update, event cancellation, payment receipt, membership renewal reminder, membership status change, session waitlist promotion, or club announcement), THE Notification_Service SHALL deliver the notification through all channels enabled in the Member's preferences within 30 seconds for asynchronous channels (email, SMS, push) and within 2 seconds for the in-app channel
3. THE Notification_Service SHALL allow each Member to configure per-channel preferences (enable/disable per notification type), with all channels enabled by default for new Members
4. WHEN a real-time notification is sent via SignalR, THE SignalR_Hub SHALL deliver the message within 2 seconds of the triggering event
5. THE Notification_Service SHALL support webhook integrations allowing clubs to receive event notifications at configured URLs, with a maximum of 10 webhook endpoints per club
6. IF a notification delivery fails on a channel, THEN THE Notification_Service SHALL retry delivery up to 3 times with exponential backoff starting at 5 seconds (5s, 25s, 125s)
7. IF all retry attempts for a notification delivery are exhausted on a channel, THEN THE Notification_Service SHALL log the failure, mark the notification as failed for that channel, and continue delivery on remaining enabled channels without blocking

### Requirement 16: Audit, Compliance, and Security

**User Story:** As a platform operator, I want comprehensive audit trails and compliance features, so that the platform meets regulatory requirements and data is protected.

#### Acceptance Criteria

1. THE Audit_Service SHALL record an immutable log entry for every data mutation including: entity type, entity ID, action (Create/Update/Delete), actor, timestamp, and before/after values, where the before/after payload is capped at 64 KB per entry and the log entries cannot be modified or deleted through any application-level operation
2. WHEN a Member requests GDPR data export, THE Compliance_Service SHALL generate a machine-readable export containing all personal data associated with the Member (profile, bookings, payments, communications, documents) and deliver it within 72 hours of the request
3. WHEN a Member requests GDPR data erasure, THE Compliance_Service SHALL anonymise all personal data fields (name, email, phone, address, medical information) within 30 days while preserving aggregate financial records for the configured retention period (default 7 years)
4. THE Platform SHALL enforce two-factor authentication (TOTP-based) for SuperAdmin and ClubManager roles
5. WHEN a login attempt originates from an IP address not used by the account in the previous 90 days, THE Security_Service SHALL send a verification email containing a single-use link valid for 15 minutes to the account owner before granting access
6. THE Platform SHALL enforce API rate limiting: 100 requests per minute for authenticated users, 20 requests per minute for unauthenticated endpoints
7. THE Platform SHALL enforce data retention policies: delete member data after a configurable period (default 7 years) of inactivity, where inactivity is defined as no login and no booking or payment transaction, with a notification sent to the member's registered email 30 days before deletion
8. THE Audit_Service SHALL support field-level visibility controls allowing ClubManagers to restrict which fields within audit log entries are visible to Member, Coach, and Staff roles
9. THE Platform SHALL enforce CORS policies restricting API access to configured origins only
10. THE Platform SHALL set security headers on all responses: Content-Security-Policy, X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security
11. THE Platform SHALL enforce HTTPS for all connections and redirect HTTP requests to HTTPS
12. IF the verification email sent for an unrecognised IP login is not confirmed within 15 minutes, THEN THE Security_Service SHALL reject the login attempt and log the event in the audit trail

### Requirement 17: Revenue Features

**User Story:** As a platform operator, I want to maximise revenue through transaction fees, add-ons, and value-added services, so that the platform is commercially sustainable.

#### Acceptance Criteria

1. WHEN a payment is processed through the Platform, THE Payment_Service SHALL deduct a platform transaction fee (configurable between 1% and 2% in 0.1% increments, with a minimum fee of £0.30) from the club's payout via Stripe Connect and record the fee amount against the transaction
2. IF platform fee collection fails during payment processing, THEN THE Payment_Service SHALL complete the member's payment, log the fee collection failure, and retry fee collection within 24 hours
3. THE Platform SHALL display a purchasable add-ons catalog to ClubManagers listing: SMS credit packs, additional storage, white-label branding, and custom domain, each showing name, description, and price
4. WHEN a Club purchases a white-label add-on, THE Platform SHALL serve the club's portal under their custom domain with their configured branding (club logo, primary colour, secondary colour, and favicon as defined in ClubSettings)
5. THE Platform SHALL generate a public-facing club website accessible at the club's slug URL displaying: club name, description, contact email, contact phone, address, upcoming events within the next 30 days, available membership types with pricing, and a membership sign-up link
6. THE Platform SHALL support sponsor management allowing ClubManagers to add sponsors (name, logo URL, website URL, sponsorship tier) and track sponsorship agreements (start date, end date, agreed amount, payment status) with a maximum of 50 sponsors per club
7. WHEN a member presents a QR code or NFC tag at a session or event, THE Platform SHALL validate the member's identity, verify their booking or registration, record an attendance entry with a timestamp, and display a confirmation status (success or failure with reason) within 3 seconds
8. IF a member's QR code or NFC tag cannot be validated during check-in, THEN THE Platform SHALL display an error indication specifying the failure reason (expired membership, no booking found, or invalid code) and allow manual override by a ClubManager or Coach

### Requirement 18: Operational Resilience

**User Story:** As a platform operator, I want the system to be observable, resilient, and maintainable, so that issues are detected early and the platform remains available.

#### Acceptance Criteria

1. THE Platform SHALL process background jobs (email sending, payment retries, report generation, data imports) via a job processing framework (Hangfire or Quartz)
2. THE Platform SHALL expose health check endpoints that return a status of "Healthy", "Degraded", or "Unhealthy" for each monitored component: database connectivity, Redis connectivity, external service availability, and background job processing, with the health check response completing within 5 seconds
3. THE Platform SHALL implement structured logging with a Correlation_ID attached to every log entry within a request lifecycle
4. THE Platform SHALL use Redis for caching frequently accessed data (membership types, club settings, session availability) with a configurable TTL defaulting to 5 minutes and supporting values from 1 second to 24 hours
5. THE Platform SHALL store file uploads (documents, photos, exports) in Azure Blob Storage or S3-compatible storage, enforcing a maximum file size of 50 MB per upload
6. IF a background job fails, THEN THE Platform SHALL retry the job up to 3 times with exponential backoff starting at 30 seconds, and notify the operations team via the configured alerting channel (email or webhook) within 60 seconds of final failure
7. IF Redis becomes unavailable, THEN THE Platform SHALL bypass the cache and serve requests directly from the database without returning an error to the end user
8. WHEN cached data is modified through the Platform (membership types, club settings, or session availability), THE Platform SHALL invalidate the corresponding cache entry within 5 seconds of the change being persisted

### Requirement 19: Analytics and Retention

**User Story:** As a ClubManager, I want insights into club health and member engagement, so that I can make data-driven decisions to reduce churn and grow the club.

#### Acceptance Criteria

1. THE Analytics_Service SHALL calculate a club health score as a numeric value from 0 to 100, derived as a weighted average of: member growth rate (25%), payment collection rate (25%), session attendance rate (25%), and event participation rate (25%), each individually scored from 0 to 100
2. THE Analytics_Service SHALL flag a member as at-risk of churn WHEN, within a rolling 90-day window, the member meets one or more of: session attendance drops by 50% or more compared to the prior 90-day window, 2 or more consecutive payments are missed, or portal login frequency drops by 50% or more compared to the prior 90-day window
3. THE Analytics_Service SHALL track member engagement metrics per member: number of sessions attended per calendar month, number of events attended per calendar month, payment timeliness measured as average days between invoice due date and payment date, and number of portal logins per calendar month
4. THE Analytics_Service SHALL provide revenue forecasting for the next 3 calendar months based on active memberships, historical renewal rates from the prior 12 months, and historical payment data from the prior 12 months
5. WHEN a ClubManager views the analytics dashboard, THE Analytics_Service SHALL display benchmarking data comparing their club's metrics against anonymised platform averages calculated from all active clubs on the platform
6. THE Analytics_Service SHALL capture a club analytics snapshot once every 7 days and retain snapshots for a minimum of 24 months for historical trend analysis
7. IF fewer than 30 days of historical data exist for a club, THEN THE Analytics_Service SHALL display a notification indicating that analytics data is insufficient and omit the health score, churn predictions, and revenue forecast until sufficient data is available
8. IF a member has no session bookings or event registrations within the 90-day churn evaluation window, THEN THE Analytics_Service SHALL exclude that member from churn prediction calculations rather than flagging them as at-risk

### Requirement 20: Integration Ecosystem

**User Story:** As a ClubManager, I want to integrate with external services, so that data flows seamlessly between the platform and tools I already use.

#### Acceptance Criteria

1. THE Integration_Service SHALL support GoCardless Direct Debit for recurring membership payments, including mandate creation, mandate cancellation, payment collection, and payment status tracking
2. THE Integration_Service SHALL push payments, invoices, and refunds to Xero and QuickBooks at a configurable sync interval (default: every 60 minutes), mapping each transaction to the corresponding account in the external system
3. THE Integration_Service SHALL push created, updated, and cancelled sessions and events to Google Calendar and Outlook Calendar within 5 minutes of the change occurring in the platform
4. THE Integration_Service SHALL support auto-posting of published events and completed match results to configured club Facebook and X (Twitter) accounts within 10 minutes of the triggering action
5. THE Platform SHALL expose a public REST API with API key authentication for third-party integrations, enforcing a rate limit of 1000 requests per API key per hour
6. THE Platform SHALL provide OpenAPI (Swagger) documentation for all public API endpoints
7. WHEN an integration sync fails, THE Integration_Service SHALL log the failure with the integration name and error detail, notify the ClubManager via in-app notification, and retry at the next scheduled sync interval up to a maximum of 5 consecutive retry attempts per failed record
8. IF an integration exceeds the maximum retry attempts for a failed record, THEN THE Integration_Service SHALL mark the record as requiring manual intervention and notify the ClubManager with the record identifier and failure reason

### Requirement 21: Communication and Messaging

**User Story:** As a ClubManager, I want to communicate with members through templates and bulk campaigns, so that club communications are professional and efficient.

#### Acceptance Criteria

1. THE Communication_Service SHALL support email templates per Club for: Welcome, PasswordReset, PaymentReminder, BookingConfirmation, EventNotification, MembershipRenewal, where each template allows the ClubManager to edit the subject line (maximum 200 characters) and body content (maximum 10,000 characters) including placeholder variables for member first name, last name, email, member number, and club name
2. WHEN a ClubManager creates a bulk email campaign, THE Communication_Service SHALL send emails to the selected member segment with placeholder variables replaced by each recipient's corresponding member data, targeting a maximum of 5,000 recipients per campaign
3. IF a bulk email campaign targets members who have opted out of the relevant communication channel (MarketingOptIn, SmsOptIn, or EmailOptIn set to false), THEN THE Communication_Service SHALL exclude those members from the send list and include the count of excluded members in the campaign summary
4. THE Communication_Service SHALL log all sent communications with delivery status (Sent, Delivered, Bounced, Failed), recipient identifier, channel (Email or SMS), timestamp, and template type used
5. WHEN a time-sensitive event occurs (session cancellation or payment reminder due), THE Communication_Service SHALL send an SMS notification via Twilio to affected members who have SmsOptIn set to true, within 5 minutes of the triggering event
6. IF the SMS or email delivery provider is unavailable, THEN THE Communication_Service SHALL retry delivery up to 3 times with exponential backoff and log the communication with a Failed status if all retries are exhausted
7. IF a member has EmailOptIn set to false, THEN THE Communication_Service SHALL suppress all non-transactional email communications to that member while still delivering transactional messages (PasswordReset, BookingConfirmation, PaymentReminder)

### Requirement 22: Member Self-Service Portal

**User Story:** As a Member, I want a self-service portal where I can manage my profile, bookings, and payments, so that I can interact with my club without contacting administrators.

#### Acceptance Criteria

1. WHEN a Member logs in, THE Portal SHALL display a personalised dashboard showing: membership status, upcoming bookings (next 7 days), upcoming events (next 30 days), and outstanding balance
2. WHEN a Member selects an available session with remaining capacity, THE Portal SHALL allow the Member to book a place and display a booking confirmation with session date, time, and venue
3. IF a session has reached maximum capacity, THEN THE Portal SHALL prevent booking and offer the Member the option to join the waitlist
4. WHEN a Member requests to cancel a booking at least 24 hours before the session start time, THE Portal SHALL cancel the booking and confirm the cancellation
5. IF a Member attempts to cancel a booking less than 24 hours before the session start time, THEN THE Portal SHALL reject the cancellation and display a message indicating the cancellation deadline has passed
6. THE Portal SHALL allow Members to view upcoming events and register for ticketed events (by purchasing tickets) or RSVP events (by submitting attendance response)
7. THE Portal SHALL display the Member's payment history and outstanding invoices, and allow the Member to pay an invoice online via the club's configured payment provider
8. IF an online payment attempt fails, THEN THE Portal SHALL display an error message indicating the failure reason and retain the invoice in its unpaid state
9. THE Portal SHALL allow Members to add, edit, and remove family member profiles linked to their account, up to a maximum of 10 family members
10. WHEN a Member edits their profile, THE Portal SHALL allow updates to: personal details (first name, last name, phone, date of birth), address fields, emergency contacts (name, phone, relationship), medical information (conditions, allergies, doctor details), and notification preferences (email opt-in, SMS opt-in, marketing opt-in)

### Requirement 23: Reporting

**User Story:** As a ClubManager, I want comprehensive reports on membership, finances, and attendance, so that I can monitor club performance and make informed decisions.

#### Acceptance Criteria

1. WHEN a ClubManager requests a membership report, THE Report_Service SHALL generate membership data including: active/expired/pending counts grouped by membership type, member age distribution in 5-year brackets, month-over-month growth trends for the selected date range, and retention rate calculated as (renewed memberships / expiring memberships) × 100 for each period
2. WHEN a ClubManager requests a financial report, THE Report_Service SHALL generate financial data including: revenue totalled by PaymentType and PaymentMethod, outstanding balances per member, monthly and quarterly revenue trends for the selected date range, and budget vs actual variance per BudgetLine
3. WHEN a ClubManager requests an attendance report, THE Report_Service SHALL generate attendance data including: session utilisation rate calculated as (confirmed bookings / session capacity) × 100, no-show percentage calculated as (NoShow bookings / total bookings) × 100, and per-member attendance frequency as total sessions attended within the selected date range
4. THE Report_Service SHALL support date range filtering with a maximum selectable range of 365 days, export to CSV and PDF formats with a maximum export size of 10,000 rows per report, and scheduled report delivery via email at configurable intervals of daily, weekly, or monthly
5. WHEN a SuperAdmin requests a platform-wide report, THE Report_Service SHALL aggregate data across all clubs with member-level data anonymised by replacing personally identifiable fields (name, email, phone, address) with opaque identifiers while retaining club-level and statistical aggregates
6. IF a report request returns no matching data for the specified date range, THEN THE Report_Service SHALL return a success response containing an empty dataset and a message indicating no records matched the filter criteria
7. IF report generation exceeds 30 seconds, THEN THE Report_Service SHALL cancel the operation and return an error response indicating a timeout, without partial data

### Requirement 24: Modular Architecture

**User Story:** As a development team, I want the platform built as a modular monolith with clean architecture, so that modules can be developed, tested, and deployed independently.

#### Acceptance Criteria

1. THE Platform SHALL be structured as a modular monolith where each domain area (Members, Memberships, Sessions, Events, Competitions, Payments, Facilities, Equipment, Programs, Communications, Analytics) is implemented as a separate .NET project containing its own commands, queries, domain entities, and data access with no compile-time references between module projects except through a shared contracts project
2. THE Platform SHALL implement CQRS within each module by separating command handlers (write operations that modify state) and query handlers (read operations that return data) into distinct classes, where no single class performs both a state-modifying operation and a data-retrieval operation
3. THE Platform SHALL enforce module boundaries such that modules communicate only through defined contracts (interfaces in a shared contracts project or integration events via an in-process message bus), and no module directly references another module's DbContext, entities, or internal services
4. THE Platform SHALL use EF Core with a separate DbContext per module, each configured with only the entity mappings owned by that module, all targeting the same shared SQL Server database
5. THE Platform SHALL support the addition of new modules without modifying existing module code by using a module registration mechanism where new modules are discovered and loaded through assembly scanning or explicit registration at startup
6. THE Platform SHALL enforce that no circular dependencies exist between modules: if Module A depends on a contract defined by Module B, then Module B SHALL NOT depend on any contract defined by Module A
7. WHEN a module requires data owned by another module, THE Platform SHALL provide that data exclusively through the owning module's public query interface or integration events, not through direct database queries against the other module's tables

### Requirement 25: Frontend Architecture

**User Story:** As a development team, I want a modern Angular frontend with reusable components and consistent design, so that the UI is maintainable, accessible, and performant.

#### Acceptance Criteria

1. THE Frontend SHALL be built with Angular 20 using standalone components, signals for state management, and the inject() pattern for dependency injection
2. THE Frontend SHALL use Tailwind CSS v4 with DaisyUI as the component library for consistent theming and rapid UI development
3. THE Frontend SHALL implement three portal layouts: Admin (SuperAdmin), Club Manager, and Member — each with role-appropriate navigation, and each portal SHALL be lazy-loaded as a separate route chunk to minimize initial bundle size
4. THE Frontend SHALL include a reusable component library with at minimum: data tables (with sorting and filtering), form controls (text input, select, date picker, checkbox, radio, textarea), modals, toast notifications, status badges, pagination, skeleton loading states, and empty states
5. THE Frontend SHALL include a component showcase/demo page documenting all reusable components with usage examples and accepted input/output contracts
6. THE Frontend SHALL implement responsive design supporting desktop (1024px and above), tablet (768px to 1023px), and mobile (below 768px) viewports, where all interactive elements have a minimum touch target size of 44x44 CSS pixels on mobile and tablet
7. THE Frontend SHALL meet WCAG 2.1 Level AA accessibility standards including keyboard navigation for all interactive elements, ARIA labels for screen reader support, and a minimum colour contrast ratio of 4.5:1 for normal text and 3:1 for large text
8. THE Frontend SHALL support system, light, and dark theme switching where the default theme follows the operating system preference, user-selected theme preference is persisted in browser localStorage, and per-club brand colour customisation applies to primary accent colour, navigation header background, and action button colours
9. THE Frontend SHALL implement skeleton loading states for all views that fetch data from the API, displaying the skeleton within 100 milliseconds of navigation and replacing it with content once data arrives or an error state after a maximum loading timeout of 10 seconds
10. WHEN a user performs an optimistic UI action (booking a session, submitting an RSVP, or changing a status), THE Frontend SHALL immediately reflect the expected success state in the UI, and IF the server returns an error, THEN THE Frontend SHALL roll back the UI to the previous state and display a toast notification indicating the action failed within 1 second of receiving the error response
11. THE Frontend SHALL use Angular SSR (Server-Side Rendering) for public-facing pages including the platform landing page and public club profile pages, and these pages SHALL achieve a Largest Contentful Paint of 2.5 seconds or less on a simulated 4G connection
12. THE Frontend SHALL achieve an initial bundle size of no more than 500 KB gzipped for any single lazy-loaded route chunk, and a total initial load (main bundle before lazy routes) of no more than 250 KB gzipped

### Requirement 26: API Design and Documentation

**User Story:** As a developer integrating with the platform, I want well-documented RESTful APIs, so that I can build integrations efficiently.

#### Acceptance Criteria

1. THE Platform SHALL expose RESTful APIs following consistent naming conventions: plural nouns for resources, HTTP verbs for actions
2. THE Platform SHALL return standardised API responses: `{ success: boolean, message: string | null, data: object | null }` for single items and `{ items: array, totalCount: number, page: number, pageSize: number, totalPages: number }` for collections
3. THE Platform SHALL support pagination on all collection endpoints with configurable page size (minimum 1, default 20, maximum 100) and page number (minimum 1), and SHALL default to page 1 when the requested page exceeds the total number of pages
4. IF a request contains invalid input, THEN THE Platform SHALL return a 400 Bad Request response with a body containing `{ success: false, message: string, errors: [{ field: string, message: string }] }` identifying each field that failed validation and the reason for failure
5. THE Platform SHALL version the API using URL path versioning (/api/v1/) and SHALL NOT remove or rename existing fields, endpoints, or change response status codes within the same version number
6. THE Platform SHALL generate OpenAPI 3.0 documentation automatically from controller metadata, accessible at the /swagger endpoint without authentication
7. IF a request is made to a non-existent endpoint, THEN THE Platform SHALL return a 404 Not Found response with the standard error body `{ success: false, message: string, data: null }`

### Requirement 27: Data Seeding and Demo Environment

**User Story:** As a developer or sales team member, I want a pre-populated demo environment with realistic data volumes, so that dashboards look impressive and platform capabilities are immediately demonstrable.

#### Acceptance Criteria

1. THE Platform SHALL include a database seeder that creates demo data: 4 clubs across at least 3 different sport types (from the ClubType enum), 10 managers (at least 2 per club), and 50 members per club distributed across all MemberStatus values (at least 70% Active, 10% Pending, 10% Expired, and 10% Suspended or Cancelled), with at least 3 membership types per club, sessions, events, competitions, and payments
2. WHEN the seeder runs, THE Platform SHALL create one demo user account for each role (SuperAdmin, ClubManager, Member) per club, using email addresses and passwords listed in a seeder documentation section within the project README or a dedicated seed-credentials file
3. WHEN the seeder runs against a database that already contains demo data (identified by matching demo user email addresses), THE Platform SHALL skip creation of existing records and only create missing records, resulting in no duplicate entries
4. THE Seeder SHALL generate 12 months of historical payment data ending at the current month, covering at least 3 PaymentMethod types, with 5–10% of payments in Failed status and at least 20% higher payment volume during September–October and January (seasonal peaks) compared to the monthly average
5. THE Seeder SHALL generate 6 months of session attendance data with at least 70% attendance rate during in-season months, no more than 40% attendance rate during off-season months, and 5–15% of bookings marked as NoShow
6. THE Seeder SHALL generate at least 1 active competition with partial results (at least 50% of scheduled matches completed), at least 2 upcoming fixtures with Scheduled status, and at least 1 completed season with full standings for all registered teams
7. THE Seeder SHALL generate a minimum of 30 data points per chart series across all dashboard and analytics views, ensuring every dashboard widget (membership distribution, revenue trends, attendance trends, payment method breakdown, and competition standings) has at least one corresponding data record to render

### Requirement 28: Multi-Sport Support

**User Story:** As a platform operator, I want the platform to serve multiple sport types, so that clubs across different sports can use the same platform with sport-appropriate configurations.

#### Acceptance Criteria

1. THE Platform SHALL support club types: Cricket, Football, Rugby, Tennis, Golf, Hockey, Swimming, Athletics, MultiSport, CommunityGroup, YouthOrganization, and each club type SHALL be selectable during club creation
2. WHEN a Club selects a sport type during creation, THE Platform SHALL pre-populate editable sport-specific defaults for: session categories, competition types, match event types, and terminology, allowing the club manager to modify any default value after application
3. IF a club selects a sport type for which no sport-specific defaults are defined (e.g., CommunityGroup, YouthOrganization), THEN THE Platform SHALL apply a generic set of defaults and display a notification indicating that custom configuration is recommended
4. THE Platform SHALL allow clubs to customise sport-specific terminology (e.g., "match" vs "fixture", "pitch" vs "court") through club settings, with a maximum custom term length of 50 characters, and THE Platform SHALL display the customised terminology in place of default terms across all user-facing screens within that club's tenant
5. THE Platform SHALL render competition scoring input fields and statistics displays according to the club's sport type, where each sport type defines its own set of score fields (e.g., runs/wickets/overs for Cricket, goals for Football, sets/games for Tennis) and match statistics categories
6. WHEN a club manager changes the club's sport type after initial setup, THE Platform SHALL apply the new sport-specific defaults without overwriting any previously customised terminology or competition data, and SHALL prompt the club manager to review the updated defaults

### Requirement 29: Dark Mode and Theming

**User Story:** As a user, I want to switch between light and dark themes, so that I can use the platform comfortably in any lighting condition and clubs can express their brand identity.

#### Acceptance Criteria

1. THE Frontend SHALL support three theme modes: System (follows OS preference), Light, and Dark
2. WHEN a user selects a theme preference, THE Frontend SHALL persist the selection in local storage and apply it within 100 milliseconds on all subsequent page loads for that browser
3. WHILE the user's theme preference is set to System, THE Frontend SHALL switch between Light and Dark modes within 300 milliseconds of detecting an OS-level colour scheme change
4. THE Frontend SHALL apply per-club brand colours (primary, secondary, accent) to the club portal and member-facing pages, defaulting to the platform brand colours for unauthenticated pages
5. WHEN a ClubManager configures brand colours, THE Frontend SHALL generate a colour palette consisting of at least 5 shades and 5 tints per primary and secondary colour, plus corresponding contrast colours for text and icons
6. IF a generated brand colour combination does not meet WCAG 2.1 AA contrast ratios (minimum 4.5:1 for normal text, 3:1 for large text and UI components), THEN THE Frontend SHALL substitute the nearest colour that satisfies the contrast requirement while preserving the brand hue
7. THE Frontend SHALL apply theme transitions without layout shift (Cumulative Layout Shift below 0.01 during transition) and without flash of unstyled content, completing the visual transition within 300 milliseconds

### Requirement 30: Progressive Web App and Offline Support

**User Story:** As a Member, I want to install the platform as a mobile app and access key features offline, so that I can check schedules and mark attendance even without internet connectivity.

#### Acceptance Criteria

1. THE Frontend SHALL be installable as a Progressive Web App (PWA) with a service worker, web app manifest, and offline fallback page
2. WHEN a Member installs the PWA, THE Frontend SHALL cache the application shell, navigation, and the last 50 viewed items (sessions, events, and profile data) for offline access
3. WHILE offline, THE Frontend SHALL allow Members to view cached session schedules for the next 14 days, cached event details, and their profile information
4. WHILE offline, THE Frontend SHALL allow Coaches to mark session attendance, queuing up to 100 offline actions in local storage for sync when connectivity is restored
5. WHEN connectivity is restored, THE Frontend SHALL synchronise all queued offline actions with the server within 30 seconds of detecting connectivity and display a summary notification indicating the number of actions succeeded and the number failed
6. IF an offline action conflicts with a server-side change during sync, THEN THE Frontend SHALL present both the local and server versions of the data to the user and allow the user to choose to keep the local version, accept the server version, or discard the action
7. WHILE the device has no network connectivity, THE Frontend SHALL display a persistent visible indicator in the application header stating that the app is offline
8. IF sync of a queued offline action fails due to a server error, THEN THE Frontend SHALL retain the failed action in the queue and retry sync on the next connectivity restoration, up to a maximum of 3 retry attempts per action

### Requirement 31: Live Scoring and Match Centre

**User Story:** As a Member or spectator, I want to follow live match scores in real-time, so that I can stay engaged with club competitions even when not physically present.

#### Acceptance Criteria

1. WHEN a match status transitions to InProgress, THE Match_Centre SHALL establish a SignalR connection and display live scores updated within 3 seconds of each scoring change
2. THE Match_Centre SHALL support sport-specific scoring formats: cricket (runs/wickets/overs), football (goals), hockey (goals), tennis (sets/games/points), rugby (tries/conversions/penalties)
3. WHEN a match event is recorded (goal, wicket, card, substitution), THE Match_Centre SHALL push the update to all connected viewers within 2 seconds for up to 500 concurrent connections per match
4. THE Match_Centre SHALL provide a public-facing scoreboard URL that spectators can access without authentication
5. THE Match_Centre SHALL display a live commentary feed with timestamped match events in reverse-chronological order, showing the most recent 100 entries with the ability to load earlier entries
6. WHEN a match is completed, THE Match_Centre SHALL automatically transition to a match summary view displaying: final score, individual match events log, and per-player statistics relevant to the sport (e.g., runs/wickets for cricket, goals/assists for football)
7. IF the SignalR connection is lost, THEN THE Match_Centre SHALL attempt automatic reconnection up to 5 times with exponential backoff and display a connection-status indicator to the viewer
8. WHILE a match status is Scheduled or Confirmed, THE Match_Centre SHALL display the match details (teams, venue, start time) without live scoring elements

### Requirement 32: File and Document Management

**User Story:** As a ClubManager, I want to manage member documents and club files securely, so that important records are stored, organised, and accessible when needed.

#### Acceptance Criteria

1. THE Document_Service SHALL support file uploads for: profile photos, medical forms, consent forms, DBS certificates, club documents, and event media
2. IF a file upload does not match the allowed file types (images: jpg/png/webp, documents: pdf/docx, spreadsheets: csv/xlsx) or exceeds the maximum file size of 10MB per upload, THEN THE Document_Service SHALL reject the upload and return an error message indicating the specific validation failure (unsupported file type or size exceeded)
3. THE Document_Service SHALL store files in Azure Blob Storage with unique keys and maintain metadata (filename, type, size, uploader, upload date) in the database
4. THE Document_Service SHALL generate secure, time-limited (1-hour) download URLs for file access
5. IF a Club's total stored file size reaches or exceeds its Subscription_Tier quota (Free: 1GB, Starter: 5GB, Pro: 25GB, Enterprise: 100GB), THEN THE Document_Service SHALL reject new uploads and return an error message indicating the storage quota has been reached
6. WHEN a file is uploaded, THE Document_Service SHALL scan the file for malware before storing it
7. IF a malware scan detects a threat in an uploaded file, THEN THE Document_Service SHALL reject the file, discard the uploaded content, and return an error message indicating the file failed the security scan
8. THE Document_Service SHALL generate optimised versions of profile photos (maximum 300x300 pixels) and thumbnails (maximum 150x150 pixels) with a compression quality target of 80%
9. THE Document_Service SHALL restrict file access so that only ClubManagers of the owning Club, the Member who is the subject of the document, and SuperAdmins can retrieve or download files

### Requirement 33: Dashboard and Data Visualisation

**User Story:** As a ClubManager or SuperAdmin, I want rich interactive dashboards with charts and KPIs, so that I can quickly understand club performance and identify trends.

#### Acceptance Criteria

1. THE Dashboard SHALL display role-appropriate KPI cards: SuperAdmin (total clubs, total members, platform revenue, active subscriptions), ClubManager (active members, monthly revenue, session attendance rate, outstanding balance)
2. THE Dashboard SHALL render interactive charts for: revenue trends (line chart, 12-month window), membership growth (area chart, 12-month window), attendance patterns (heatmap by day/time, 4-week window), payment method distribution (doughnut chart, current month)
3. THE Dashboard SHALL support date range selection (this week, this month, this quarter, this year, custom range) for all chart data, where custom range allows selection of start and end dates within the past 24 months up to today
4. THE Dashboard SHALL display an activity feed showing the 20 most recent actions (new members, payments received, bookings made), refreshed every 30 seconds via polling
5. WHEN a user with role ClubManager or SuperAdmin hovers over a chart data point, THE Dashboard SHALL display a tooltip within 200 milliseconds showing the data point label, value, and percentage change from the previous period
6. THE Dashboard SHALL load chart data asynchronously, displaying skeleton placeholders during loading, and complete the initial chart render within 3 seconds on a standard connection
7. IF chart data fails to load or the API returns an error, THEN THE Dashboard SHALL display an error message indicating the failure and provide a retry action, while preserving any previously loaded data on other dashboard widgets
8. THE Dashboard SHALL support full-screen mode for individual charts and export chart data as PNG image or CSV file containing a maximum of 10,000 data rows

### Requirement 34: Internationalisation and Localisation

**User Story:** As a platform serving UK-based clubs, I want consistent locale-aware formatting and the foundation for multi-language support, so that the platform feels native and professional.

#### Acceptance Criteria

1. THE Platform SHALL format all currency values in GBP (£) with 2 decimal places using en-GB locale formatting (e.g., "£1,234.56") with the thousands separator as a comma and the decimal separator as a period
2. THE Platform SHALL format all dates in UK format (DD/MM/YYYY) for absolute display, and SHALL display relative time labels ("X minutes ago", "X hours ago", "Yesterday") for timestamps within the past 48 hours, switching to absolute DD/MM/YYYY format for timestamps older than 48 hours
3. THE Platform SHALL format all times in 24-hour format (HH:mm) using the Europe/London timezone, automatically adjusting displayed times during BST/GMT transitions without requiring user action
4. THE Frontend SHALL externalise all user-facing text labels, messages, and validation prompts into Angular i18n resource files, such that no translatable string is hard-coded in component templates or TypeScript files
5. THE Platform SHALL store all timestamps in UTC in the database and convert to the club's configured timezone (defaulting to Europe/London) for display in the frontend
6. THE Platform SHALL use en-GB number formatting (comma as thousand separator, period as decimal point) as the default, and SHALL allow club administrators to override the locale for number formatting via club settings
7. IF a timestamp or date value is null or invalid, THEN THE Platform SHALL display a placeholder indicator (e.g., a dash or "N/A") instead of rendering a formatting error or blank space

### Requirement 35: Error Handling and Resilience Patterns

**User Story:** As a platform operator, I want the system to handle failures gracefully, so that users experience minimal disruption when components fail.

#### Acceptance Criteria

1. THE Platform SHALL implement a global exception handling middleware that catches all unhandled exceptions, logs them with Correlation_ID, and returns a standardised error response without exposing internal details
2. THE Platform SHALL implement circuit breaker patterns for all external service calls (Stripe, SendGrid, Twilio, GoCardless) with configurable failure thresholds (5 failures in 30 seconds triggers open state)
3. WHILE a circuit breaker is in the open state, THE Platform SHALL return a graceful degradation response and queue the operation for retry when the circuit closes
4. IF Redis is unavailable, THEN THE Platform SHALL fall back to in-memory caching and continue serving requests with degraded performance
5. IF the database connection pool is exhausted, THEN THE Platform SHALL queue incoming requests with a 30-second timeout and return 503 Service Unavailable if the timeout expires
6. THE Platform SHALL implement request timeout policies: 30 seconds for standard API calls, 120 seconds for report generation, 300 seconds for data imports
7. THE Platform SHALL return problem details (RFC 7807) format for all error responses including: type, title, status, detail, and instance (Correlation_ID)

### Requirement 36: CI/CD and Deployment

**User Story:** As a development team, I want automated build, test, and deployment pipelines, so that code changes are validated and deployed consistently.

#### Acceptance Criteria

1. THE Platform SHALL include Docker containerisation with multi-stage builds for both the API and Angular frontend
2. THE Platform SHALL include a docker-compose configuration for local development with: SQL Server, Redis, Azure Storage emulator (Azurite), and the application services
3. THE Platform SHALL include a CI pipeline definition (GitHub Actions) that: restores dependencies, runs unit tests, runs integration tests, builds Docker images, and publishes artifacts
4. THE Platform SHALL include environment-specific configuration (Development, Staging, Production) managed through environment variables and Azure Key Vault for secrets
5. THE Platform SHALL include database migration scripts that run automatically on deployment (EF Core migrations with idempotent execution)
6. THE Platform SHALL include a health check endpoint that the deployment pipeline uses to verify successful deployment before routing traffic

### Requirement 37: Search and Filtering

**User Story:** As a ClubManager, I want powerful search and filtering across all data, so that I can quickly find members, payments, sessions, and events without scrolling through lists.

#### Acceptance Criteria

1. THE Platform SHALL provide a global search bar that searches across members (name, email, member number), events (title), sessions (title), and invoices (invoice number), returning results scoped to the current user's club (tenant) only
2. THE Platform SHALL support advanced filtering on all list views with: multi-select filters (e.g., status, category), date range pickers, status filters, and free-text search, where each filter combination narrows results using AND logic
3. WHEN a user types at least 2 characters in a search field, THE Platform SHALL provide debounced (300ms) server-side search results displaying a maximum of 20 matching items grouped by entity type
4. THE Platform SHALL support up to 10 saved filter presets per user, each storing the filter field values, sort order, and a user-defined name of up to 50 characters
5. THE Platform SHALL highlight matching text in search results by visually distinguishing the matched substring from surrounding text
6. THE Platform SHALL return search results within 500ms for datasets up to 10,000 records
7. IF a search or filter query returns no matching results, THEN THE Platform SHALL display an empty-state message indicating no items matched the current criteria and suggest clearing or adjusting filters

### Requirement 38: Merchandise and Shop

**User Story:** As a ClubManager, I want to sell club merchandise online, so that the club can generate additional revenue and members can purchase items conveniently.

#### Acceptance Criteria

1. THE Shop_Service SHALL allow ClubManagers to create product listings with: name (maximum 150 characters), description (maximum 2000 characters), up to 8 images per product, price (0.01 to 999,999.99 in club currency), variants (size, colour), and stock quantity (0 to 99,999 units per variant)
2. WHEN a Member purchases a product, THE Shop_Service SHALL process payment via the club's configured payment provider, decrement stock for the purchased variant, and create an order with status Pending
3. IF payment processing fails during a purchase attempt, THEN THE Shop_Service SHALL not decrement stock, not create the order, and display an error message indicating the payment failure reason to the Member
4. IF a Member attempts to purchase a variant that has zero stock, THEN THE Shop_Service SHALL reject the purchase and display a message indicating the item is out of stock
5. THE Shop_Service SHALL support product categories (maximum 50 per club) and display products in a paginated catalogue on the member portal with filtering by category and sorting by price or name
6. WHEN a product variant reaches zero stock, THE Shop_Service SHALL mark it as out-of-stock and prevent further purchases of that variant
7. WHERE the ClubManager has enabled restock notifications for a product, WHEN a Member views an out-of-stock variant, THE Shop_Service SHALL allow the Member to register interest, and WHEN stock is replenished, THE Shop_Service SHALL notify all registered Members via email
8. THE Shop_Service SHALL track order status through the following valid transitions: Pending to Confirmed, Confirmed to Dispatched, Dispatched to Delivered, and Pending or Confirmed or Dispatched to Refunded
9. WHEN an order transitions to Confirmed status, THE Shop_Service SHALL send an order confirmation email to the Member containing: order reference number, items purchased, quantities, total amount paid, and expected dispatch timeframe as configured by the ClubManager
