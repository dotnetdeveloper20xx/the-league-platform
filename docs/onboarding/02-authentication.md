# 02 — Authentication & JWT Token Management

## 📖 Feature Overview

The League uses JWT (JSON Web Tokens) for stateless authentication. When a user logs in, they receive a short-lived access token (15 minutes) and a long-lived refresh token (7 days). This provides security (short exposure window) with convenience (no repeated logins).

### Key Capabilities
- Email/password login with JWT issuance
- Refresh token rotation (one-time use, revoke on reuse)
- Account lockout (5 failed attempts → 15-minute lock)
- Password reset via email (60-minute token)
- Email verification required before first login
- 2FA (TOTP) for admin roles
- Session management (view/revoke active sessions)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| JWT Bearer (not cookies) | Works with SPA + mobile, no CSRF issues |
| 15-minute access token | Short exposure window if token is stolen |
| 7-day refresh token | Convenience — users don't re-login daily |
| One-time refresh tokens | Prevents token replay attacks |
| Revoke ALL tokens on suspicious reuse | If a consumed token is presented, assume compromise |
| ASP.NET Core Identity | Battle-tested password hashing, lockout, 2FA support |
| Claims in JWT (not DB lookup) | Zero database calls for authorization on every request |

### Token Flow
```
Login → Access Token (15 min) + Refresh Token (7 days)
  ↓
API calls use Access Token in Authorization header
  ↓
Access Token expires → Client sends Refresh Token
  ↓
Server validates Refresh Token → Issues NEW Access + Refresh tokens
  ↓
Old Refresh Token is invalidated (one-time use)
```

---

## 📊 Data Model

### Entities (in Identity Module)

**ApplicationUser** (extends IdentityUser):
```
Id (string/GUID), Email, FirstName, LastName, ClubId (Guid?),
MemberId (Guid?), IsActive, CreatedAt, LastLoginAt
+ All IdentityUser fields (PasswordHash, LockoutEnd, AccessFailedCount, etc.)
```

**RefreshToken**:
```
Id (Guid), UserId (string FK), Token (string, hashed),
ExpiresAt (DateTime), CreatedAt, IsRevoked (bool), RevokedReason (string?)
INDEX: Token (unique), UserId
```

**UserSession**:
```
Id (Guid), UserId (string FK), DeviceIdentifier (string),
IpAddress (string?), LastActiveAt, CreatedAt, IsRevoked (bool)
INDEX: UserId
```

---

## 🔧 Class-by-Class Breakdown

### 1. `Modules.Identity/Domain/ApplicationUser.cs`
Extends ASP.NET Core Identity's `IdentityUser` with platform-specific fields:
- `ClubId` — Links user to their tenant (null for SuperAdmin)
- `MemberId` — Links to the Member record (for Member role users)
- `IsActive` — Soft-delete flag
- `CreatedAt`, `LastLoginAt` — Audit timestamps

### 2. `Modules.Identity/Infrastructure/Services/JwtService.cs`
**Purpose**: Generates JWT access tokens and cryptographic refresh tokens.

**Key Methods:**

`GenerateAccessToken(ApplicationUser user, string role)`:
- Creates claims: sub, email, name, role, clubId, memberId, jti
- Signs with HMAC-SHA256 using the configured secret
- Sets expiry to 15 minutes
- Returns the serialized JWT string

`GenerateRefreshToken()`:
- Generates 64 random bytes using `RandomNumberGenerator`
- Returns Base64-encoded string
- NOT a JWT — just a random opaque token

### 3. `Modules.Identity/Application/Commands/LoginCommand.cs`
**Purpose**: Validates credentials and issues tokens.

**Flow:**
```
1. Find user by email (UserManager.FindByEmailAsync)
2. Check if account is locked (UserManager.IsLockedOutAsync)
   → If locked: return 403 with lockout seconds remaining
3. Check email verification (user.EmailConfirmed)
   → If not verified: return 403 "email verification required"
4. Validate password (UserManager.CheckPasswordAsync)
   → If wrong: increment failed count, check if now locked
   → Return 401 "Invalid credentials" (don't reveal which field)
5. Reset failed count on success
6. Generate access token + refresh token
7. Store refresh token in DB (hashed)
8. Create/update UserSession record
9. Update user.LastLoginAt
10. Return { accessToken, refreshToken, user }
```

### 4. `Modules.Identity/Application/Commands/RefreshTokenCommand.cs`
**Purpose**: Rotates tokens — issues new pair, invalidates old.

**Flow:**
```
1. Find refresh token in DB by token value
2. Validate: not expired, not revoked
3. If already revoked (reuse detected!):
   → Revoke ALL user's refresh tokens (security measure)
   → Return 401
4. Mark current token as revoked (consumed)
5. Generate new access token + new refresh token
6. Store new refresh token in DB
7. Return new token pair
```

### 5. `Modules.Identity/Api/AuthController.cs`
**Endpoints:**
| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | /api/v1/auth/login | Public | Login |
| POST | /api/v1/auth/register | Public | Register |
| POST | /api/v1/auth/refresh | Public | Refresh tokens |
| POST | /api/v1/auth/forgot-password | Public | Request reset |
| POST | /api/v1/auth/reset-password | Public | Reset with token |
| PUT | /api/v1/auth/change-password | Authenticated | Change password |
| GET | /api/v1/auth/me | Authenticated | Get current user |
| GET | /api/v1/auth/sessions | Authenticated | List sessions |
| DELETE | /api/v1/auth/sessions/{id} | Authenticated | Revoke session |

---

## ⚙️ Method-Level Detail

### JwtService.GenerateAccessToken
```csharp
public string GenerateAccessToken(ApplicationUser user, string role)
{
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),        // Subject (user ID)
        new(JwtRegisteredClaimNames.Email, user.Email),   // Email
        new("name", $"{user.FirstName} {user.LastName}"), // Display name
        new(ClaimTypes.Role, role),                        // For [Authorize(Roles=...)]
        new("role", role),                                 // Custom claim for frontend
        new("clubId", user.ClubId?.ToString() ?? ""),     // Tenant ID
        new("memberId", user.MemberId?.ToString() ?? ""), // Member record ID
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "TheLeague",
        audience: "TheLeagueApp",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(15),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Password Validation Rules
```
- Minimum 6 characters
- Requires at least one digit
- Requires at least one lowercase letter
- Requires at least one uppercase letter
- Non-alphanumeric NOT required
- Unique email required
```

---

## 🌐 API Contracts

### Login Request/Response
```json
// POST /api/v1/auth/login
// Request:
{ "email": "james.wilson@teddingtoncc.co.uk", "password": "Demo123!" }

// Success Response (200):
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...",
  "user": {
    "id": "30000000-0000-0000-0001-000000000001",
    "email": "james.wilson@teddingtoncc.co.uk",
    "firstName": "James",
    "lastName": "Wilson",
    "role": "ClubManager",
    "clubId": "10000000-0000-0000-0000-000000000001"
  }
}

// Lockout Response (403):
{ "error": "Account locked", "lockoutSecondsRemaining": 845 }

// Invalid Credentials (401):
{ "error": "Invalid email or password." }
```

---

## 🎨 Frontend Integration

### AuthService (Angular)
```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUser = signal<AuthUser | null>(null);

  login(email: string, password: string) {
    return this.http.post<AuthResponse>('/api/v1/auth/login', { email, password });
  }

  setAuth(response: AuthResponse) {
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    this.currentUser.set(response.user);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('access_token');
  }
}
```

### Auth Interceptor (automatic token refresh)
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Attach token to every request
  // On 401: attempt refresh, retry original request
  // On refresh failure: logout
};
```

---

## 🧪 Testing Approach

### Property Tests
- **Property 3**: JWT always contains required claims (sub, email, role, clubId, jti)
- **Property 4**: Error messages are identical regardless of which credential failed
- **Property 5**: Consumed refresh token cannot be reused

### Unit Tests
- Login with valid credentials → returns tokens
- Login with wrong password → returns 401, increments failed count
- 5 failed attempts → account locked for 15 minutes
- Refresh with valid token → new tokens issued, old invalidated
- Refresh with consumed token → ALL tokens revoked (security)

---

## 🚀 How to Extend

### Adding social login (Google/Microsoft):
1. Add `Microsoft.AspNetCore.Authentication.Google` package
2. Configure in `IdentityModule.RegisterModule`
3. Add `/api/v1/auth/external/google` endpoint
4. On callback: find or create user, issue JWT as normal

### Adding API key authentication (for integrations):
1. Create `ApiKey` entity in Identity module
2. Add `ApiKeyAuthenticationHandler` middleware
3. Check `X-Api-Key` header before JWT
4. Map API key to a club (tenant context)
