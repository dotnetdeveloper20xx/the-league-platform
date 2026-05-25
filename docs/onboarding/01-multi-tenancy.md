# 01 — Multi-Tenancy & Data Isolation

## 📖 Feature Overview

Multi-tenancy is the foundation of The League Platform. Every sports club (tenant) operates in complete data isolation within a single shared database. A cricket club in Teddington never sees data from a football club in Highbury — guaranteed at the database query level.

### Why It Exists
- **Cost efficiency** — One database serves hundreds of clubs (no per-tenant DB provisioning)
- **Simplified deployment** — Single application instance, single connection string
- **Data isolation** — EF Core global query filters enforce `WHERE ClubId = @TenantId` on every query automatically

### How It Works (High Level)
```
1. User logs in → JWT contains `clubId` claim
2. Request arrives → TenantMiddleware extracts ClubId from JWT
3. TenantService stores CurrentTenantId (scoped per request)
4. DbContext applies global query filter → only matching data returned
5. SuperAdmin bypasses filters for platform-wide operations
```

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Shared database with discriminator column | Simpler than database-per-tenant; no connection string management |
| `ClubId` as GUID on every tenant-scoped entity | Universally unique, no collision risk across tenants |
| EF Core Global Query Filters | Automatic enforcement — developers can't accidentally forget the WHERE clause |
| Scoped `ITenantService` | Per-request lifetime ensures tenant context is isolated between concurrent requests |
| JWT claim extraction (not header-only) | Cryptographically verified — can't be spoofed by the client |
| SuperAdmin bypass | Platform admins need cross-tenant access for support and reporting |

### Alternative Approaches Considered
- **Database-per-tenant**: Too expensive at scale (hundreds of DBs), complex migrations
- **Schema-per-tenant**: SQL Server schema switching is complex and error-prone
- **Row-level security (RLS)**: Database-level enforcement but harder to debug and test

---

## 📊 Data Model

### Base Entity Hierarchy
```csharp
BaseEntity          → Id (Guid), CreatedAt, UpdatedAt
  └── TenantEntity  → ClubId (Guid)  ← THIS IS THE DISCRIMINATOR
       └── AuditableEntity → CreatedBy, UpdatedBy
```

### Every tenant-scoped table has:
```sql
CREATE TABLE members.Members (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClubId UNIQUEIDENTIFIER NOT NULL,  -- ← Tenant discriminator
    FirstName NVARCHAR(100),
    ...
    INDEX IX_Members_ClubId_Email (ClubId, Email) -- Composite index for tenant queries
);
```

### Key Indexes (always include ClubId first)
- `(ClubId, Email)` — Member lookup within a club
- `(ClubId, Status)` — Filtered queries within a club
- `(ClubId, PaymentDate)` — Date-range queries within a club

---

## 🔧 Class-by-Class Breakdown

### 1. `TheLeague.Shared.Domain/Entities/TenantEntity.cs`
**Purpose**: Base class for all tenant-scoped entities.

```csharp
public abstract class TenantEntity : BaseEntity
{
    public Guid ClubId { get; protected set; }
}
```

- `protected set` prevents external code from changing the tenant after creation
- Every domain entity that belongs to a club extends this class

### 2. `TheLeague.Shared.Contracts/Services/ITenantService.cs`
**Purpose**: Interface for accessing the current tenant context.

```csharp
public interface ITenantService
{
    Guid? CurrentTenantId { get; }
    void SetCurrentTenant(Guid? tenantId);
}
```

- `Guid?` (nullable) — null means no tenant context (SuperAdmin or unauthenticated)
- Registered as **Scoped** — one instance per HTTP request

### 3. `TheLeague.Shared.Infrastructure/Tenancy/TenantService.cs`
**Purpose**: Implementation that stores the tenant ID for the current request.

```csharp
public class TenantService : ITenantService
{
    public Guid? CurrentTenantId { get; private set; }

    public void SetCurrentTenant(Guid? tenantId)
    {
        CurrentTenantId = tenantId;
    }
}
```

- Simple in-memory storage — the middleware sets it, DbContexts read it
- Thread-safe because it's scoped (one instance per request, not shared)

### 4. `TheLeague.Shared.Infrastructure/Middleware/TenantMiddleware.cs`
**Purpose**: Extracts ClubId from the authenticated user's JWT and sets the tenant context.

```csharp
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Primary: Extract from JWT claims
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var clubIdClaim = context.User.FindFirst("clubId")?.Value;
            if (Guid.TryParse(clubIdClaim, out var clubId))
                tenantService.SetCurrentTenant(clubId);
        }

        // Fallback: X-Tenant-Id header (for admin scenarios)
        if (!tenantService.CurrentTenantId.HasValue)
        {
            var headerValue = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (Guid.TryParse(headerValue, out var headerClubId))
                tenantService.SetCurrentTenant(headerClubId);
        }

        await _next(context);
    }
}
```

**Key decisions:**
- JWT is the primary source (cryptographically verified)
- Header fallback allows SuperAdmin to impersonate a club for debugging
- Runs AFTER authentication middleware (so `context.User` is populated)

### 5. DbContext with Global Query Filters (Example: MembersDbContext)
**Purpose**: Automatically filters all queries to the current tenant.

```csharp
public class MembersDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        builder.Entity<Member>(e =>
        {
            e.HasQueryFilter(x => x.ClubId == tenantId);
            // ... other configuration
        });
    }
}
```

**How it works:**
- `HasQueryFilter` adds a WHERE clause to EVERY query on that entity
- If `tenantId` is `Guid.Empty` (no tenant set), no data is returned (safe default)
- SuperAdmin sets `CurrentTenantId = null` and uses `.IgnoreQueryFilters()` when needed

---

## ⚙️ Method-Level Detail

### TenantMiddleware.InvokeAsync
```
Input: HttpContext (with authenticated user)
Process:
  1. Check if user is authenticated
  2. Find "clubId" claim in JWT
  3. Parse as GUID
  4. Call tenantService.SetCurrentTenant(clubId)
  5. If no JWT claim, check X-Tenant-Id header (admin fallback)
  6. Call next middleware
Output: TenantService.CurrentTenantId is set for this request
```

### TenantBehaviour (MediatR Pipeline)
```
Input: Any MediatR request implementing ITenantRequest
Process:
  1. Check if request has ITenantRequest interface
  2. Validate ClubId is not Guid.Empty
  3. If TenantService has a CurrentTenantId, verify it matches the request's ClubId
  4. If mismatch → throw ForbiddenException (cross-tenant access attempt)
Output: Request proceeds to handler (or 403 thrown)
```

---

## 🌐 API Endpoints

Tenancy is transparent to API consumers. They never explicitly pass a ClubId — it's extracted from their JWT:

```http
GET /api/v1/members
Authorization: Bearer <jwt-with-clubId-claim>

→ Returns only members for the authenticated user's club
```

SuperAdmin can override by passing the header:
```http
GET /api/v1/members
Authorization: Bearer <superadmin-jwt>
X-Tenant-Id: 10000000-0000-0000-0000-000000000001

→ Returns members for the specified club
```

---

## 🎨 Frontend Integration

The frontend never needs to think about tenancy. The JWT contains the `clubId` claim, and the `authInterceptor` attaches it to every request automatically.

```typescript
// auth.interceptor.ts
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = authService.getAccessToken();
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
```

The backend handles all tenant filtering — the frontend just calls endpoints and gets back tenant-scoped data.

---

## 🧪 Testing Approach

### Property-Based Tests (FsCheck)
```
Property 1: Tenant Data Isolation
  For ANY set of entities across multiple clubs,
  and FOR ANY authenticated request with a specific ClubId,
  ALL query results SHALL contain ONLY entities matching that ClubId.

Property 2: ClubId Extraction and Validation
  For ANY JWT payload:
  - If ClubId is present and valid GUID → tenant context is set
  - If ClubId is missing or malformed → request is rejected with 403
```

### Unit Tests
- Test TenantMiddleware with various JWT claim scenarios
- Test TenantBehaviour with matching/mismatching ClubIds
- Test DbContext query filters return only tenant-scoped data

### Integration Tests
- Create data for Club A and Club B
- Authenticate as Club A user
- Verify Club B data is never returned
- Verify SuperAdmin can access both

---

## 🚀 How to Extend

### Adding a new tenant-scoped entity:
1. Extend `TenantEntity` (not `BaseEntity`)
2. Add `HasQueryFilter(x => x.ClubId == tenantId)` in your DbContext
3. Add composite index starting with `ClubId`
4. The rest is automatic — middleware + filters handle isolation

### Adding a non-tenant entity (platform-wide):
- Extend `BaseEntity` directly (no ClubId)
- No query filter needed
- Example: `SubscriptionTierConfig`, `AuditLogEntry`

### Bypassing tenant filters (SuperAdmin queries):
```csharp
var allMembers = await _db.Members
    .IgnoreQueryFilters()
    .ToListAsync();
```
Use sparingly — only for platform-wide reporting.
