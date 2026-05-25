# 04 — CQRS with MediatR Pipeline

## 📖 Feature Overview

The League uses the CQRS (Command Query Responsibility Segregation) pattern powered by MediatR. Every operation is either a **Command** (writes data) or a **Query** (reads data), routed through a pipeline of cross-cutting behaviours that handle logging, validation, tenancy, performance monitoring, and transactions automatically.

### Why It Exists
- **Separation of concerns** — Read and write paths have different optimization needs
- **Pipeline behaviours** — Cross-cutting concerns (validation, logging, tenancy) applied once, not repeated in every handler
- **Testability** — Each handler is a single-purpose class, easy to unit test in isolation
- **Consistency** — Every request follows the same flow regardless of which module it belongs to

### How It Works (High Level)
```
1. Controller receives HTTP request
2. Controller creates Command/Query object
3. MediatR.Send(command) dispatches to pipeline
4. Pipeline behaviours execute in order:
   Logging → Validation → Tenant → Performance → Transaction → Handler
5. Handler executes business logic
6. Result<T> returned through pipeline back to controller
7. Controller maps Result<T> to HTTP response
```

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| MediatR (not direct service calls) | Decouples controllers from business logic; enables pipeline |
| Commands return `Result<T>` | Consistent success/failure handling without exceptions for business rules |
| Queries return DTOs directly | Queries are read-only, no side effects, simpler return type |
| FluentValidation (not DataAnnotations) | More expressive, testable, composable validation rules |
| Pipeline order matters | Logging first (always), Validation before Handler (fail fast), Tenant before Handler (security) |
| One handler per file | Easy to find, easy to test, clear responsibility |

### Commands vs Queries

| Aspect | Command | Query |
|--------|---------|-------|
| Purpose | Change state (create, update, delete) | Read state (get, list, search) |
| Return type | `Result<T>` or `Result` | DTO or `PagedResult<T>` |
| Side effects | Yes (DB writes, events, notifications) | No (read-only) |
| Validation | Always (FluentValidation) | Optional (parameter validation) |
| Transaction | Optional (`ITransactionalRequest`) | Never |
| Naming | `CreateMemberCommand`, `UpdateSessionCommand` | `GetMemberQuery`, `ListSessionsQuery` |

---

## 📊 Pipeline Behaviours (Execution Order)

### 1. LoggingBehaviour
**File**: `TheLeague.Shared.Infrastructure/Behaviours/LoggingBehaviour.cs`

Logs the start and completion of every request with elapsed time.

```csharp
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
        return response;
    }
}
```

### 2. ValidationBehaviour
**File**: `TheLeague.Shared.Infrastructure/Behaviours/ValidationBehaviour.cs`

Runs all registered `IValidator<TRequest>` validators before the handler executes. If any fail, throws `ValidationException` with field-level errors.

```csharp
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
{
    if (!_validators.Any()) return await next();

    var context = new ValidationContext<TRequest>(request);
    var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));
    var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();

    if (failures.Count != 0)
    {
        var errors = failures.Select(f => new FieldError(f.PropertyName, f.ErrorMessage)).ToList();
        throw new ValidationException(errors);
    }

    return await next();
}
```

### 3. TenantBehaviour
**File**: `TheLeague.Shared.Infrastructure/Behaviours/TenantBehaviour.cs`

Validates tenant context for requests implementing `ITenantRequest`. Prevents cross-tenant access.

```csharp
public interface ITenantRequest
{
    Guid ClubId { get; }
}

// In the behaviour:
if (request is ITenantRequest tenantRequest)
{
    if (tenantRequest.ClubId == Guid.Empty)
        throw new ForbiddenException("Missing or invalid tenant context.");

    if (_tenantService.CurrentTenantId.HasValue && _tenantService.CurrentTenantId.Value != tenantRequest.ClubId)
        throw new ForbiddenException("Insufficient tenant access.");
}
```

### 4. PerformanceBehaviour
**File**: `TheLeague.Shared.Infrastructure/Behaviours/PerformanceBehaviour.cs`

Logs a warning if any request takes longer than 500ms — helps identify slow queries or handlers.

```csharp
_timer.Start();
var response = await next();
_timer.Stop();

if (_timer.ElapsedMilliseconds > 500)
    _logger.LogWarning("Long running request: {RequestName} ({ElapsedMs}ms)", requestName, elapsed);
```

### 5. TransactionBehaviour
**File**: `TheLeague.Shared.Infrastructure/Behaviours/TransactionBehaviour.cs`

Wraps commands marked with `ITransactionalRequest` in a database transaction. Rolls back on exception.

```csharp
public interface ITransactionalRequest { }

// Only applies to requests implementing the marker interface:
if (request is not ITransactionalRequest) return await next();

try
{
    var response = await next();
    _logger.LogInformation("Transaction committed for {RequestName}", requestName);
    return response;
}
catch
{
    _logger.LogWarning("Transaction rolled back for {RequestName}", requestName);
    throw;
}
```

---

## 🔧 Result<T> Pattern

All commands return `Result<T>` for consistent success/failure handling:

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public string? Message { get; }
    public List<string> Errors { get; } = new();

    public static Result Success(string? message = null) => new(true, message);
    public static Result Failure(string message) => new(false, message);
    public static Result<T> Success<T>(T data, string? message = null) => new(data, true, message);
    public static Result<T> Failure<T>(string message) => new(default, false, message);
}

public class Result<T> : Result
{
    public T? Data { get; }
}
```

**Usage in handlers:**
```csharp
// Success
return Result.Success(new MemberDto { Id = member.Id, ... }, "Member created successfully");

// Failure (business rule violation — NOT an exception)
return Result.Failure<MemberDto>("Member with this email already exists.");
```

**Usage in controllers:**
```csharp
var result = await _mediator.Send(command);
return result.IsSuccess
    ? Ok(result)
    : BadRequest(result);
```

---

## ⚙️ How to Create a New Command

### Step 1: Define the Command
```csharp
// Application/Commands/CreateMemberCommand.cs
public record CreateMemberCommand(
    Guid ClubId,
    string FirstName,
    string LastName,
    string Email
) : IRequest<Result<MemberDto>>, ITenantRequest;
```

### Step 2: Create the Validator
```csharp
// Application/Commands/CreateMemberCommandValidator.cs
public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ClubId).NotEqual(Guid.Empty);
    }
}
```

### Step 3: Implement the Handler
```csharp
// Application/Commands/CreateMemberCommandHandler.cs
public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<MemberDto>>
{
    private readonly MembersDbContext _db;

    public CreateMemberCommandHandler(MembersDbContext db) => _db = db;

    public async Task<Result<MemberDto>> Handle(CreateMemberCommand request, CancellationToken ct)
    {
        var exists = await _db.Members.AnyAsync(m => m.Email == request.Email, ct);
        if (exists)
            return Result.Failure<MemberDto>("Member with this email already exists.");

        var member = Member.Create(request.ClubId, request.FirstName, request.LastName, request.Email);
        _db.Members.Add(member);
        await _db.SaveChangesAsync(ct);

        return Result.Success(member.ToDto(), "Member created successfully");
    }
}
```

### Step 4: Wire up in Controller
```csharp
[HttpPost]
[RequirePermission(Permissions.ManageMembers)]
public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
{
    var command = new CreateMemberCommand(CurrentClubId, request.FirstName, request.LastName, request.Email);
    var result = await _mediator.Send(command);
    return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
}
```

---

## 🌐 FluentValidation Integration

Validators are auto-discovered via assembly scanning in each module's registration:

```csharp
// In module registration:
services.AddValidatorsFromAssembly(typeof(MembersModule).Assembly);
```

**Validation error response (400):**
```json
{
  "type": "ValidationException",
  "errors": [
    { "field": "Email", "message": "'Email' is not a valid email address." },
    { "field": "FirstName", "message": "'First Name' must not be empty." }
  ]
}
```

---

## 🧪 Testing Approach

### Unit Testing a Handler
```csharp
[Fact]
public async Task CreateMember_WithDuplicateEmail_ReturnsFailure()
{
    // Arrange
    var db = CreateInMemoryDb();
    db.Members.Add(Member.Create(clubId, "John", "Doe", "john@test.com"));
    await db.SaveChangesAsync();

    var handler = new CreateMemberCommandHandler(db);
    var command = new CreateMemberCommand(clubId, "Jane", "Doe", "john@test.com");

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Message.Should().Contain("already exists");
}
```

### Testing Validators
```csharp
[Fact]
public void Validate_EmptyEmail_HasError()
{
    var validator = new CreateMemberCommandValidator();
    var command = new CreateMemberCommand(Guid.NewGuid(), "John", "Doe", "");
    var result = validator.Validate(command);
    result.Errors.Should().Contain(e => e.PropertyName == "Email");
}
```

---

## 🚀 How to Extend

### Adding a new pipeline behaviour:
1. Create class implementing `IPipelineBehavior<TRequest, TResponse>`
2. Register in `ServiceCollectionExtensions.cs`:
   ```csharp
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(YourNewBehaviour<,>));
   ```
3. Order matters — add it in the correct position relative to existing behaviours

### Adding a query with pagination:
```csharp
public record ListMembersQuery(Guid ClubId, int Page, int PageSize) : IRequest<PagedResult<MemberDto>>, ITenantRequest;
```
