# 03 — Role-Based Access Control (RBAC)

## 📖 Feature Overview

The League enforces a 5-role permission system that controls what each user can see and do. Rather than simple role checks, we use a **permission-based** approach where roles map to granular permissions, and endpoints are protected by specific permission requirements.

### The 5 Roles
| Role | Scope | Access Level |
|------|-------|-------------|
| **SuperAdmin** | Platform-wide | Everything — all clubs, system config, user management |
| **ClubManager** | Single club | Full CRUD on their assigned club's data |
| **Member** | Self only | Self-service: profile, bookings, payments, events |
| **Coach** | Single club (limited) | Session management + read-only member access |
| **Staff** | Single club (limited) | Read-only access to all club data |

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Permission-based (not role-based) attributes | More granular — can add new roles without changing controllers |
| Static permission mapping (not DB) | Fast (no DB lookup per request), simple to reason about |
| Custom `IAuthorizationPolicyProvider` | Dynamically creates policies for any permission string |
| Role stored in JWT claim | No DB lookup needed for authorization |
| Permissions checked per-request from role claim | Role changes take effect on next request (when new JWT issued) |

---

## 📊 Data Model

No database tables — the permission matrix is defined in code:

```
Roles.cs → Defines role constants
Permissions.cs → Defines permission constants
RolePermissionMapping.cs → Maps roles to permissions
```

---

## 🔧 Class-by-Class Breakdown

### 1. `Shared.Infrastructure/Authorization/Roles.cs`
```csharp
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ClubManager = "ClubManager";
    public const string Member = "Member";
    public const string Coach = "Coach";
    public const string Staff = "Staff";
    public static readonly string[] All = { SuperAdmin, ClubManager, Member, Coach, Staff };
}
```

### 2. `Shared.Infrastructure/Authorization/Permissions.cs`
Defines ~25 granular permissions grouped by domain:
```csharp
public static class Permissions
{
    public const string ManagePlatform = "Platform.Manage";
    public const string ManageMembers = "Members.Manage";
    public const string ViewMembers = "Members.View";
    public const string BookSessions = "Sessions.Book";
    public const string ManagePayments = "Payments.Manage";
    // ... etc
}
```

### 3. `Shared.Infrastructure/Authorization/RolePermissionMapping.cs`
Maps each role to its allowed permissions:
```csharp
public static class RolePermissionMapping
{
    private static readonly Dictionary<string, HashSet<string>> _rolePermissions = new()
    {
        [Roles.SuperAdmin] = new(new[] { /* ALL permissions */ }),
        [Roles.ClubManager] = new(new[] { /* Club-level permissions */ }),
        [Roles.Member] = new(new[] { /* Self-service permissions */ }),
        [Roles.Coach] = new(new[] { /* Session + read member */ }),
        [Roles.Staff] = new(new[] { /* Read-only */ }),
    };

    public static bool HasPermission(string role, string permission)
        => _rolePermissions.TryGetValue(role, out var perms) && perms.Contains(permission);
}
```

### 4. `Shared.Infrastructure/Authorization/PermissionAuthorizationHandler.cs`
Handles the actual permission check:
```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var role = context.User.FindFirst("role")?.Value;
        if (role != null && RolePermissionMapping.HasPermission(role, requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

### 5. `Shared.Infrastructure/Authorization/PermissionPolicyProvider.cs`
Dynamically creates authorization policies for permission strings:
```csharp
public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
{
    if (policyName.Contains('.')) // Permission format: "Members.Manage"
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
    return _fallbackPolicyProvider.GetPolicyAsync(policyName);
}
```

### 6. Usage in Controllers
```csharp
[HttpGet]
[RequirePermission(Permissions.ManageMembers)]  // Permission-based
public async Task<IActionResult> GetMembers() { ... }

[HttpPost]
[RequireRole(Roles.SuperAdmin)]  // Role-based (simpler)
public async Task<IActionResult> CreateClub() { ... }
```

---

## ⚙️ Method-Level Detail

### Authorization Flow (per request)
```
1. Request arrives with JWT
2. ASP.NET Core extracts claims (role, clubId, etc.)
3. Controller has [RequirePermission("Members.Manage")]
4. Framework calls PermissionPolicyProvider.GetPolicyAsync("Members.Manage")
5. Provider creates policy with PermissionRequirement
6. Framework calls PermissionAuthorizationHandler.HandleRequirementAsync
7. Handler reads "role" claim from JWT → "ClubManager"
8. Handler calls RolePermissionMapping.HasPermission("ClubManager", "Members.Manage")
9. Returns true → request proceeds
```

---

## 🎨 Frontend Integration

### Route Guards
```typescript
export const roleGuard: CanActivateFn = (route) => {
  const requiredRoles = route.data['roles'] as string[];
  const userRole = authService.userRole();
  return requiredRoles.includes(userRole) || router.createUrlTree(['/unauthorized']);
};
```

### Route Configuration
```typescript
{
  path: 'admin',
  canActivate: [authGuard, roleGuard],
  data: { roles: ['SuperAdmin'] },
  loadComponent: () => import('./layouts/admin-layout/...')
}
```

### Conditional UI Elements
```html
@if (authService.userRole() === 'ClubManager') {
  <button class="btn btn-primary">Edit Member</button>
}
```

---

## 🧪 Testing Approach

### Property Test
```
Property 6: Role-Permission Matrix Enforcement
  For ANY authenticated user with a given role,
  and FOR ANY resource endpoint,
  the access decision (grant/deny) SHALL match the defined permission matrix.
```

### Unit Tests
- Each role can access its permitted endpoints
- Each role is denied access to non-permitted endpoints
- Unknown role gets denied everything
- Role change takes effect on next request

---

## 🚀 How to Extend

### Adding a new permission:
1. Add constant to `Permissions.cs`
2. Add to appropriate roles in `RolePermissionMapping.cs`
3. Use `[RequirePermission(Permissions.NewPermission)]` on controller

### Adding a new role:
1. Add constant to `Roles.cs`
2. Add permission set to `RolePermissionMapping.cs`
3. Add to `Roles.All` array
4. Create role in Identity seeder
5. Add route guard data in Angular routes
