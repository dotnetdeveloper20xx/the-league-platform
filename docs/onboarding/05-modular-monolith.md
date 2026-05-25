# 05 — Modular Monolith Architecture

## 📖 Feature Overview

The League is built as a **modular monolith** — a single deployable application composed of independent, loosely-coupled modules. Each module owns its domain, data access, and API surface. Modules communicate through integration events, never by directly referencing each other's internals.

### Why Not Microservices
- **Operational simplicity** — One deployment, one database, one CI pipeline
- **No network overhead** — In-process communication, no HTTP/gRPC between services
- **Transactional consistency** — Shared database means no distributed transactions (no Saga complexity)
- **Team size** — A small team doesn't benefit from service boundaries; they add coordination cost
- **Future-proof** — Modules can be extracted to microservices later if needed (clean boundaries already exist)

### How It Works (High Level)
```
TheLeague.Host (ASP.NET Core Web API)
  ├── Discovers all IModule implementations via reflection
  ├── Calls RegisterModule() on each → DI registration
  ├── Calls UseModule() on each → Middleware/endpoints
  └── Shared Infrastructure (MediatR, EF Core, Auth, Tenancy)

Modules:
  Members, Memberships, Sessions, Events, Competitions,
  Clubs, Identity, Payments, Communications, Analytics,
  Documents, Equipment, Facilities, Programs, Shop, Subscriptions
```

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| `IModule` interface for auto-discovery | No manual registration — add a module, it's automatically loaded |
| Separate DbContext per module | Each module owns its schema; no cross-module entity references |
| Shared database (single connection string) | Simplicity; modules use different schemas/table prefixes |
| Integration events (not direct calls) | Loose coupling — modules don't import each other's namespaces |
| Shared.Domain for base entities/enums | Common types (TenantEntity, Result, enums) live in shared layer |
| Shared.Infrastructure for cross-cutting | Pipeline behaviours, middleware, auth — used by all modules |
| Shared.Contracts for interfaces/events | The "contract" between modules — only this is referenced cross-module |

---

## 📊 Module Structure

Every module follows the same internal layout:

```
TheLeague.Modules.{Name}/
├── {Name}Module.cs              ← IModule implementation (entry point)
├── Api/
│   └── {Name}Controller.cs      ← HTTP endpoints
├── Application/
│   ├── Commands/                 ← Write operations (CQRS)
│   ├── Queries/                  ← Read operations (CQRS)
│   └── Dtos/                     ← Data transfer objects
├── Domain/
│   ├── {Entity}.cs               ← Domain entities (rich models)
│   └── ...
├── Infrastructure/
│   ├── Persistence/
│   │   └── {Name}DbContext.cs    ← Module's own DbContext
│   └── Services/                 ← External integrations
└── TheLeague.Modules.{Name}.csproj
```

---

## 🔧 Class-by-Class Breakdown

### 1. `TheLeague.Shared.Contracts/IModule.cs`
**Purpose**: The contract every module must implement for auto-discovery.

```csharp
public interface IModule
{
    string Name { get; }
    void RegisterModule(IServiceCollection services, IConfiguration configuration);
    void UseModule(IApplicationBuilder app);
}
```

- `Name` — Human-readable identifier (e.g., "Members", "Events")
- `RegisterModule` — DI registration (DbContext, services, validators, MediatR handlers)
- `UseModule` — Middleware or endpoint mapping (rarely needed beyond controllers)

### 2. Module Implementation (Example: MembersModule)
```csharp
public class MembersModule : IModule
{
    public string Name => "Members";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<MembersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MembersModule).Assembly));

        // FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(MembersModule).Assembly);

        // Module-specific services
        services.AddScoped<IMemberNumberGenerator, MemberNumberGenerator>();
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No custom middleware needed — controllers are auto-discovered
    }
}
```

### 3. Host Auto-Discovery (Program.cs / Startup)
```csharp
// Discover all IModule implementations across loaded assemblies
var modules = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
    .Select(t => (IModule)Activator.CreateInstance(t)!)
    .ToList();

// Register each module
foreach (var module in modules)
    module.RegisterModule(services, configuration);

// Use each module
foreach (var module in modules)
    module.UseModule(app);
```

---

## ⚙️ Module Boundaries & Dependency Rules

### What a module CAN reference:
- `TheLeague.Shared.Domain` — Base entities, enums, value objects, Result<T>
- `TheLeague.Shared.Contracts` — IModule, integration events, shared service interfaces
- `TheLeague.Shared.Infrastructure` — Pipeline behaviours, middleware, auth

### What a module CANNOT reference:
- Another module's `Domain` namespace (no cross-module entity access)
- Another module's `Infrastructure` namespace (no cross-module DbContext access)
- Another module's `Application` namespace (no cross-module handler calls)

### Cross-Module Communication: Integration Events
When Module A needs to notify Module B about something, it publishes an integration event through `Shared.Contracts`:

```csharp
// In Shared.Contracts/Events/
public record MemberCreatedEvent(Guid MemberId, Guid ClubId, string Email) : IIntegrationEvent;

// In Members module (publisher):
await _eventBus.Publish(new MemberCreatedEvent(member.Id, member.ClubId, member.Email));

// In Identity module (subscriber):
public class MemberCreatedEventHandler : IIntegrationEventHandler<MemberCreatedEvent>
{
    public async Task Handle(MemberCreatedEvent @event, CancellationToken ct)
    {
        // Create user account for the new member
    }
}
```

---

## 🗄️ Separate DbContexts, Shared Database

Each module has its own `DbContext` that maps only its own entities:

```csharp
public class MembersDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Member> Members => Set<Member>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<CustomFieldDefinition> CustomFields => Set<CustomFieldDefinition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("members"); // Schema isolation

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;
        builder.Entity<Member>(e =>
        {
            e.HasQueryFilter(x => x.ClubId == tenantId);
            e.HasIndex(x => new { x.ClubId, x.Email }).IsUnique();
        });
    }
}
```

**Key points:**
- Each module uses a different schema (`members`, `memberships`, `sessions`, `events`, `competitions`)
- All schemas live in the same physical database
- EF Core migrations are per-module (each DbContext has its own migration history)
- No foreign keys across module boundaries (use GUIDs as loose references)

---

## 🌐 API Surface

Each module exposes its own controller(s) under a shared API prefix:

```
/api/v1/members/...       → MembersController
/api/v1/memberships/...   → MembershipsController
/api/v1/sessions/...      → SessionsController
/api/v1/events/...        → EventsController
/api/v1/competitions/...  → CompetitionsController
```

Controllers are auto-discovered by ASP.NET Core's `AddControllers()` — no manual route registration needed.

---

## 🧪 Testing Approach

### Unit Tests (per module)
- Test handlers in isolation with in-memory DbContext
- Test domain entities and their business rules
- Test validators with various inputs

### Integration Tests
- Test full pipeline (controller → MediatR → handler → DB)
- Use `WebApplicationFactory<Program>` for realistic HTTP testing
- Each module's tests are independent — no cross-module test dependencies

### Property Tests
```
Property 7: Module Isolation
  For ANY module M and ANY request handled by M,
  the request SHALL only read/write entities owned by M's DbContext.
  No cross-module entity access shall occur.
```

---

## 🚀 How to Add a New Module

### Step 1: Create the project
```bash
dotnet new classlib -n TheLeague.Modules.NewFeature -o src/Modules/TheLeague.Modules.NewFeature
```

### Step 2: Add project references
```xml
<ItemGroup>
  <ProjectReference Include="..\..\TheLeague.Shared\TheLeague.Shared.Domain\TheLeague.Shared.Domain.csproj" />
  <ProjectReference Include="..\..\TheLeague.Shared\TheLeague.Shared.Contracts\TheLeague.Shared.Contracts.csproj" />
  <ProjectReference Include="..\..\TheLeague.Shared\TheLeague.Shared.Infrastructure\TheLeague.Shared.Infrastructure.csproj" />
</ItemGroup>
```

### Step 3: Implement IModule
```csharp
public class NewFeatureModule : IModule
{
    public string Name => "NewFeature";
    public void RegisterModule(IServiceCollection services, IConfiguration configuration) { /* ... */ }
    public void UseModule(IApplicationBuilder app) { }
}
```

### Step 4: Create the folder structure
```
Domain/ → Entities
Application/Commands/ → Write handlers
Application/Queries/ → Read handlers
Application/Dtos/ → Response models
Api/ → Controller
Infrastructure/Persistence/ → DbContext + migrations
```

### Step 5: Reference from Host
Add the project reference to `TheLeague.Host.csproj` — auto-discovery handles the rest.

The module is now live. No other configuration needed.
