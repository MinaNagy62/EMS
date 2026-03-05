# EMS Project — Complete Technical Guide

Everything you need to understand this project, top to bottom. Every file, every pattern, every decision explained.

---

## 1. What Is This Project?

An **Employee Management System (EMS)** REST API. It manages **Departments**, **Employees**, and **User Authentication**. Built with .NET 10, SQL Server, Entity Framework Core, and JWT authentication. The goal is to learn and demonstrate backend patterns for interviews.

**Current status:** Milestone 1 (COMPLETED), Milestone 2 (COMPLETED), Milestone 3 (IN PROGRESS ~70%).

---

## 2. Architecture: Clean Architecture (Onion)

The project is split into 4 layers. The core rule: **inner layers never depend on outer layers**.

```
┌─────────────────────────────────────────────────┐
│                   EMS_API                       │  ← Outermost (controllers, middleware, Program.cs)
│  ┌─────────────────────────────────────────┐    │
│  │          EMS_Infrastructure             │    │  ← Database, EF Core, repositories, JWT service
│  │  ┌─────────────────────────────────┐    │    │
│  │  │       EMS_Application           │    │    │  ← Business logic, DTOs, interfaces, services, validation
│  │  │  ┌─────────────────────────┐    │    │    │
│  │  │  │      EMS_Domain         │    │    │    │  ← Entities, enums (the core)
│  │  │  └─────────────────────────┘    │    │    │
│  │  └─────────────────────────────────┘    │    │
│  └─────────────────────────────────────────┘    │
└─────────────────────────────────────────────────┘
```

**Dependency rule:**
- `EMS_Domain` → depends on **nothing**
- `EMS_Application` → depends on `EMS_Domain`
- `EMS_Infrastructure` → depends on `EMS_Application` (and transitively `EMS_Domain`)
- `EMS_API` → depends on all three

**Why this matters:** Your business logic (Application) defines *interfaces* (e.g., `IDepartmentRepository`, `IJwtTokenService`). Infrastructure *implements* them. This means you could swap SQL Server for PostgreSQL, or swap JWT for OAuth — by only changing Infrastructure. Application and Domain don't care.

---

## 3. Layer-by-Layer Breakdown

### 3.1 EMS_Domain — The Core

**Location:** `EMS_Domain/`
**NuGet packages:** None (pure C#)

This layer contains only your data models and enums. No logic, no dependencies.

#### Entities

**Department** (`Entities/Department.cs`):
| Property | Type | Purpose |
|---|---|---|
| Id | int | Primary key (auto-increment) |
| Name | string | Department name, max 100 chars |
| Code | string | Unique short code (e.g., "HR", "IT01"), max 10 |
| Description | string? | Optional, max 500 |
| IsActive | bool | Soft delete flag (default: true) |
| CreatedAt | DateTime | Set once on creation |
| UpdatedAt | DateTime? | Set on every update |
| Employees | ICollection\<Employee\> | Navigation property (one-to-many), initialized to `new List<Employee>()` |

**Employee** (`Entities/Employee.cs`):
| Property | Type | Purpose |
|---|---|---|
| Id | int | Primary key |
| FirstName | string | Max 50 |
| LastName | string | Max 50 |
| Email | string | Unique |
| Phone | string? | Optional, max 20 |
| DateOfBirth | DateTime | Must be in the past |
| HireDate | DateTime | Past or today |
| Salary | decimal | Precision 18,2 |
| Gender | Gender (enum) | Male=1, Female=2 |
| JobTitle | string | Max 100 |
| DepartmentId | int | Foreign key to Department |
| Department | Department | Navigation property |
| IsActive | bool | Soft delete flag (default: true) |
| CreatedAt | DateTime | Set once on creation |
| UpdatedAt | DateTime? | Set on every update |

**AppUser** (`Entities/AppUser.cs`) — Added in Milestone 3:
| Property | Type | Purpose |
|---|---|---|
| Id | int | Primary key |
| FirstName | string | Max 50 |
| LastName | string | Max 50 |
| Email | string | Unique (used as username) |
| PasswordHash | string | BCrypt hash of password — never store plain text |
| Role | Role (enum) | Default: Employee |
| RefreshToken | string? | Current valid refresh token (nullable — null before first login) |
| RefreshTokenExpiryDate | DateTime? | When the refresh token expires (7 days from generation) |
| IsActive | bool | Soft delete flag (default: true) |
| CreatedAt | DateTime | Set once on creation |

**Why `PasswordHash` and not `Password`?** You never store raw passwords. BCrypt creates a one-way hash that includes a random salt. Even if the database is compromised, attackers can't reverse the hash to get the original password.

**Why `RefreshToken` is stored on the entity?** When a user requests a token refresh, we need to verify the refresh token they sent matches what we have on file. Storing it on the user entity keeps it simple — one user = one active refresh token.

#### Enums

**Gender** (`Enum/Gender.cs`): `Male = 1, Female = 2`

**Role** (`Enum/Role.cs`): `Admin = 1, HR = 2, Employee = 3`
- Admin: full access to everything
- HR: can manage employees
- Employee: read-only access (future use in authorization)

---

### 3.2 EMS_Application — Business Logic

**Location:** `EMS_Application/`
**NuGet packages:**
- `FluentValidation.DependencyInjectionExtensions` (12.1.1) — validation rules + DI auto-registration
- `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.3) — IServiceCollection for DI extension methods
- `BCrypt.Net-Next` (4.1.0) — password hashing
- `Microsoft.Extensions.Options` (10.0.3) — IOptions<T> for strongly-typed config

This is the biggest layer. It contains: interfaces, DTOs, mapping, validation, services, exceptions, helpers, and the API response wrapper.

#### 3.2.1 Interfaces (`Interfaces/`)

**IGenericRepository\<T\>** — base repository with common CRUD:
- `GetByIdAsync(int id)` → find by primary key
- `GetAllAsync(filter?, includes[])` → list with optional WHERE and Include
- `FindAsync(predicate, includes[])` → single entity with optional Include
- `AddAsync(entity)` → track for insert
- `Update(entity)` → track for update
- `Remove(entity)` → track for delete

**IDepartmentRepository** / **IEmployeeRepository** / **IAppUserRepository** — extend `IGenericRepository<T>` with no extra methods (yet). They exist so you *can* add entity-specific queries later without modifying the generic one.

**IUnitOfWork** — groups repositories + `SaveChangesAsync()`. This ensures all changes in a single request go into one database transaction.
```
UnitOfWork.Departments  → IDepartmentRepository
UnitOfWork.Employees    → IEmployeeRepository
UnitOfWork.AppUsers     → IAppUserRepository      ← Added in M3
UnitOfWork.SaveChangesAsync()  → commits everything
```

**IDepartmentService** / **IEmployeeService** — define the business operations (GetAll, GetById, Create, Update, Delete). They accept DTOs and return DTOs — never entities.

**IAuthService** (`Interfaces/AppUsers/IAuthService.cs`) — Added in M3:
```csharp
Task<AuthResponse> RegisterAsync(RegisterRequest request);
Task<AuthResponse> LoginAsync(LoginRequest request);
Task<AuthResponse> RefreshTokenAsync(string refreshToken);
```
This is NOT a CRUD service — it's an auth-specific service. Register creates a user + returns tokens. Login validates credentials + returns tokens. RefreshToken rotates an expired access token.

**IJwtTokenService** (`Interfaces/AppUsers/IJwtTokenService.cs`) — Added in M3:
```csharp
(string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user);
string GenerateRefreshToken();
```
Notice the **tuple return** `(string Token, DateTime ExpiresAt)`. This is a design choice: the token's expiry time is calculated inside `GenerateAccessToken`, so it returns both the token string AND when it expires. This prevents the caller from calculating expiry separately (which could drift).

**Why IJwtTokenService is defined in Application but implemented in Infrastructure:** Application defines WHAT it needs (an interface). Infrastructure implements HOW (JWT signing with crypto libraries). This follows the Dependency Inversion Principle — the same reason repository interfaces are in Application but implementations are in Infrastructure.

#### 3.2.2 DTOs (`DTO/`)

DTOs (Data Transfer Objects) are the shapes that go in and out of the API. Entities NEVER leave the service layer.

**Department DTOs** (`DTO/Department/`):
- `CreateDepartmentRequest` — Name, Code, Description (what the client sends to POST)
- `UpdateDepartmentRequest` — Name, Code, Description (what the client sends to PUT)
- `DepartmentResponse` — Id, Name, Code, Description, IsActive, CreatedAt, UpdatedAt (what the API returns)

**Employee DTOs** (`DTO/Employee/`):
- `CreateEmployeeRequest` — all fields except Id, IsActive, CreatedAt, UpdatedAt, Department nav
- `UpdateEmployeeRequest` — same fields as Create
- `EmployeeResponse` — all fields + `DepartmentName` (denormalized from the nav property)

**Auth DTOs** (`DTO/Auth/`) — Added in M3:
- `RegisterRequest` — FirstName, LastName, Email, Password
- `LoginRequest` — Email, Password
- `AuthResponse` — Token, RefreshToken, ExpiresAt, Email, Role

**Why Create and Update are separate classes even though they're identical:** They represent different intents. If Update later becomes a PATCH (partial update with nullable fields), they'll diverge. Separate types = separate validators = easy to change one without affecting the other.

**Why RegisterRequest has `Password` but AppUser has `PasswordHash`:** The DTO carries the raw password from the client. The service hashes it with BCrypt before saving. The raw password never touches the database.

#### 3.2.3 Mapping (`Mapping/`)

Manual mapping via **extension methods** (no AutoMapper). Each mapping file has 3 methods:

| Method | Direction | Used By |
|---|---|---|
| `.ToResponse()` | Entity → Response DTO | Service (returning data) |
| `.ToEntity()` | Create Request → Entity | Service (creating) |
| `.ApplyUpdate()` | Update Request → existing Entity | Service (updating) |

There's also an `IEnumerable<T>.ToResponse()` overload for mapping lists.

**Why `ApplyUpdate` instead of `ToEntity` for updates?** When updating, you need the existing entity from the database (it has Id, CreatedAt, IsActive, etc.). You can't create a new one — you mutate the tracked entity so EF Core generates an UPDATE statement, not an INSERT.

**Why no AutoMapper?** Manual mapping is explicit — you see exactly what maps to what. AutoMapper can silently map wrong properties if names don't match perfectly. For interviews, being able to explain the trade-offs shows deeper understanding.

**Note:** Auth doesn't have a mapping file because there's no entity-to-DTO conversion pattern. Register creates an AppUser inline in the service, and the AuthResponse is built manually from the generated tokens + user data.

#### 3.2.4 Validators (`Validators/`)

FluentValidation validators — one per request DTO (6 total). Rules are defined in the constructor using a fluent chain.

**Department validators** (`CreateDepartmentValidator`, `UpdateDepartmentValidator`):
- Name: not empty, 2-100 chars
- Code: not empty, 2-10 chars, regex `^[A-Z0-9]+$` (uppercase letters and digits only — also handles null safely, unlike `.Must()`)
- Description: max 500

**Employee validators** (`CreateEmployeeValidator`, `UpdateEmployeeValidator`):
- FirstName/LastName: not empty, 2-50 chars
- Email: not empty, valid email format
- Phone: max 20 (optional, so no NotEmpty)
- DateOfBirth: must be before today (`LessThan(DateTime.Today)`)
- HireDate: today or before (`LessThanOrEqualTo(DateTime.Today)`) — notice the distinction from DOB
- Salary: greater than 0
- Gender: must be a defined enum value (`IsInEnum()`)
- JobTitle: not empty, 2-100 chars
- DepartmentId: greater than 0

**Auth validators** (`RegisterRequestValidator`, `LoginRequestValidator`) — Added in M3:
- Register: FirstName/LastName (2-50), Email (valid format), Password (min 8 chars)
- Login: Email (valid format), Password (required — no min length on login, just on registration)

**How they're registered:** `AddValidatorsFromAssembly()` in `DependencyInjection.cs` scans the assembly and auto-registers every class that extends `AbstractValidator<T>` into DI. You never manually register each validator.

**Where validation happens:** In the **service layer** (not controllers, not middleware). Services inject `IValidator<T>`, call `.ValidateAsync()`, and throw `ValidationException` if invalid. This is important for interviews — validation in the service layer means validation runs even if someone calls the service from a background job or another service, not just from an HTTP controller.

#### 3.2.5 Custom Exceptions (`Exceptions/`)

Three custom exception types that the middleware catches and translates to HTTP responses:

| Exception | Constructor | Generated Message | HTTP Status |
|---|---|---|---|
| `NotFoundException` | `(string entityName, object key)` | `"Department (5) was not found."` | 404 |
| `BadRequestException` | `(string message)` | Whatever you pass | 400 |
| `ValidationException` | `(IDictionary<string, string[]> errors)` | `"One or more validation errors occurred."` | 422 |

`ValidationException` also has an `Errors` property — an `IDictionary<string, string[]>` where keys are property names and values are arrays of error messages. This matches the ASP.NET Core ModelState error format, so frontends can display field-level errors.

**Why in Application layer, not API?** Because services throw them. If exceptions lived in API, Application would need to reference API — breaking the dependency rule. Services throw `NotFoundException`, the middleware (in API) catches it and converts to HTTP 404.

#### 3.2.6 Common Utilities (`Common/`)

**ApiResponse\<T\>** (`Common/ApiResponse.cs`) — a generic wrapper so every API response has a consistent shape:

```json
// Success
{
  "success": true,
  "message": "Request completed successfully.",
  "data": { ... },
  "errors": null
}

// Failure (validation)
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "data": null,
  "errors": {
    "Email": ["Email is required.", "A valid email address is required."],
    "Salary": ["Salary must be greater than 0."]
  }
}

// Failure (not found)
{
  "success": false,
  "message": "Department (5) was not found.",
  "data": null,
  "errors": null
}
```

Uses **static factory methods** (a design pattern):
- `ApiResponse<T>.SuccessResponse(data, message?)` — sets Success=true, default message "Request completed successfully."
- `ApiResponse<T>.FailResponse(message, errors?)` — sets Success=false

This prevents mistakes like forgetting to set `Success = true`. The `Message` property defaults to `string.Empty` to avoid null reference issues.

**ValidationExtensions** (`Common/ValidationExtensions.cs`) — Added in M3:
```csharp
public static IDictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
```
Converts FluentValidation's `ValidationResult` into the `IDictionary<string, string[]>` format that `ValidationException` expects. Groups errors by property name. Used in all 6 validation points across DepartmentService, EmployeeService, and AuthService — extracted to eliminate code duplication.

**JwtSettings** (`Common/JwtSettings.cs`) — Added in M3:
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
```
This is a **POCO class** that maps 1:1 to the `JwtSettings` section in `appsettings.json`. Used with the **Options pattern** — registered via `services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"))`, then injected as `IOptions<JwtSettings>` into services.

**Why Options pattern instead of IConfiguration?** Strongly-typed settings give you compile-time safety (no typos in string keys like `configuration["JwtSettings:SecretKey"]`), IntelliSense support, and are easier to mock in unit tests.

#### 3.2.7 Services (`Services/`)

**DepartmentService** and **EmployeeService** — the business logic layer. They:
1. Validate the incoming DTO using injected `IValidator<T>`
2. If validation fails, convert errors with `ToErrorDictionary()` and throw `ValidationException`
3. Use UnitOfWork to access repositories
4. Map between DTOs and entities
5. Throw `NotFoundException` on missing data
6. Return response DTOs

**Key patterns in services:**
- **Validation first:** Every Create/Update method validates before doing anything else
- **Soft delete:** `Delete` doesn't remove from DB — it sets `IsActive = false` and `UpdatedAt = DateTime.UtcNow`
- **All queries filter by `IsActive`:** `GetAll` uses `.GetAllAsync(d => d.IsActive)`, not `.GetAllAsync()`
- **GetById throws NotFoundException:** Services never return null for "not found" — they throw. The middleware turns it into a 404.
- **Employee queries include Department:** Because `EmployeeResponse` needs `DepartmentName`, the service passes `e => e.Department` as an include expression to avoid lazy loading issues
- **CreatedAt/UpdatedAt set in service:** `DateTime.UtcNow` is set before saving

**AuthService** (`Services/AuthService.cs`) — Added in M3. The authentication service with 3 methods:

**RegisterAsync flow:**
```
1. Validate input (FirstName, LastName, Email, Password) via FluentValidation
2. Check email doesn't already exist → BadRequestException if it does
3. Create AppUser entity with BCrypt.HashPassword(password)
4. Generate access token (JWT) + refresh token (random 64 bytes)
5. Set RefreshToken and RefreshTokenExpiryDate on user
6. Save to database
7. Return AuthResponse (Token, RefreshToken, ExpiresAt, Email, Role)
```

**LoginAsync flow:**
```
1. Validate input (Email, Password)
2. Find user by email AND IsActive=true
3. If user is null OR password doesn't verify → BadRequestException("Invalid email or password.")
   ↑ SECURITY: same error for both cases prevents user enumeration
4. Generate new access token + refresh token
5. Update user's RefreshToken and expiry date
6. Save to database
7. Return AuthResponse
```

**RefreshTokenAsync flow:**
```
1. Find user by refresh token AND IsActive=true
2. If not found → NotFoundException
3. If RefreshTokenExpiryDate < now → BadRequestException("Refresh token has expired.")
4. Generate NEW access token + NEW refresh token (rotation)
   ↑ SECURITY: old refresh token is invalidated — this is called token rotation
5. Update user with new refresh token + new expiry
6. Save to database
7. Return AuthResponse
```

**Why token rotation?** If a refresh token is stolen, it can only be used once. The next time the legitimate user tries to refresh, their old token won't match (it was replaced), and they'll be forced to re-login. This limits the damage window of a stolen token.

**Why BCrypt instead of SHA256?** BCrypt is intentionally slow (configurable work factor) and automatically generates a unique salt per hash. SHA256 is fast (bad for passwords — allows brute-force attacks) and requires you to manage salts manually. Every security expert recommends BCrypt/scrypt/Argon2 for password hashing.

#### 3.2.8 DependencyInjection (`DependencyInjection.cs`)

Extension method `AddApplication()` that registers:
- `IDepartmentService` → `DepartmentService` (Scoped)
- `IEmployeeService` → `EmployeeService` (Scoped)
- `IAuthService` → `AuthService` (Scoped) — Added in M3
- All FluentValidation validators from the assembly (auto-scan)

Called in `Program.cs` as `builder.Services.AddApplication()`.

---

### 3.3 EMS_Infrastructure — Database & External Services

**Location:** `EMS_Infrastructure/`
**NuGet packages:**
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.3) — SQL Server database provider
- `Microsoft.EntityFrameworkCore.Tools` (10.0.3) — Migrations CLI (`dotnet ef`)

#### 3.3.1 AppDbContext (`Data/AppDbContext.cs`)

The EF Core database context. Exposes three DbSets:
- `DbSet<Employee> Employees`
- `DbSet<Department> Departments`
- `DbSet<AppUser> AppUsers` — Added in M3

Applies all Fluent API configurations from the assembly automatically via `ApplyConfigurationsFromAssembly()` — you never have to manually call each configuration.

#### 3.3.2 Entity Configurations (`Data/Configurations/`)

Fluent API configuration — one per entity. This is where database schema rules are defined (column sizes, indexes, relationships, defaults).

**DepartmentConfiguration:**
- `Code` has a unique index
- `IsActive` defaults to `true` at database level
- `HasMany(Employees).WithOne(Department)` — one-to-many relationship
- `OnDelete(DeleteBehavior.Restrict)` — can't delete a department that has employees (prevents orphan records)

**EmployeeConfiguration:**
- `Email` has a unique index
- `Salary` has precision (18, 2)
- All required fields marked with `.IsRequired()`
- Max lengths match the entity properties

**AppUserConfiguration** — Added in M3:
- `Email` has a unique index (prevents duplicate accounts at DB level)
- `FirstName`/`LastName` required, max 50
- `PasswordHash` required
- No relationship configurations — AppUser is independent (not linked to Employee entity)

**Why Fluent API over Data Annotations?** Keeps entities clean (no `[Required]`, `[MaxLength]` attributes cluttering the model). Also, Fluent API can do things annotations can't (composite indexes, owned types, complex relationships). In interviews, this shows you understand that domain entities should be framework-agnostic.

#### 3.3.3 Repositories (`Repositories/`)

**GenericRepository\<T\>** — implements `IGenericRepository<T>`:
- Uses `DbSet<T>` for all operations
- `GetAllAsync` builds a query with optional WHERE and Includes, then calls `.ToListAsync()`
- `FindAsync` builds a query with Includes, then calls `.FirstOrDefaultAsync(predicate)`
- `AddAsync` uses `_dbSet.AddAsync()` (async because value generators might need DB access in HiLo scenarios — with IDENTITY, sync `Add` would also work)
- `Update` and `Remove` are sync — they only mark the entity in the Change Tracker, no DB call happens until `SaveChangesAsync()`

**DepartmentRepository** / **EmployeeRepository** / **AppUserRepository** — inherit GenericRepository with the specific entity type. No extra methods yet, but the classes exist so you can add entity-specific queries (e.g., `FindByEmailAsync`) without touching the generic repository.

#### 3.3.4 UnitOfWork (`UnitOfWork/UnitOfWork.cs`)

Implements `IUnitOfWork`. Wraps all repositories and the DbContext.

**Lazy initialization:** Repositories aren't created until first accessed:
```csharp
public IDepartmentRepository Departments =>
    _departments ??= new DepartmentRepository(_context);

public IEmployeeRepository Employees =>
    _employees ??= new EmployeeRepository(_context);

public IAppUserRepository AppUsers =>
    _appUsers ??= new AppUserRepository(_context);
```
This uses the `??=` null-coalescing assignment — if `_departments` is null, create it; otherwise reuse. This means if a request only uses Departments, the Employee and AppUser repositories are never instantiated.

**Why UnitOfWork?** Without it, each repository would call `SaveChanges` independently. With it, you can make changes across multiple repositories and commit them all in one transaction via `SaveChangesAsync()`. Example: creating a user and logging the action would both succeed or both fail.

Implements `IDisposable` to dispose the DbContext when the scope ends.

#### 3.3.5 JwtTokenService (`Services/JwtTokenService.cs`) — Added in M3

Implements `IJwtTokenService`. This is in Infrastructure because it depends on framework/crypto libraries (`System.IdentityModel.Tokens.Jwt`, `Microsoft.IdentityModel.Tokens`).

**GenerateAccessToken(AppUser user):**
1. Reads settings from `IOptions<JwtSettings>` (injected)
2. Creates JWT claims:
   - `sub` — user ID (standard JWT subject claim)
   - `email` — user's email
   - `role` — user's role as string (e.g., "Admin", "HR", "Employee")
   - `jti` — unique token ID (GUID) — prevents token replay
3. Creates signing credentials with HMAC-SHA256 using the secret key
4. Calculates expiry: `DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes)`
5. Creates a `JwtSecurityToken` with issuer, audience, claims, expiry, and credentials
6. Returns `(tokenString, expiresAt)` as a tuple

**GenerateRefreshToken():**
1. Creates 64 random bytes using `RandomNumberGenerator.Create()` (cryptographically secure)
2. Converts to base64 string
3. Returns the string

**Why RandomNumberGenerator and not `Guid.NewGuid()`?** GUIDs are unique but not cryptographically random. `RandomNumberGenerator` produces unpredictable bytes — important because a predictable refresh token could be guessed by an attacker.

#### 3.3.6 DependencyInjection (`DependencyInjection.cs`)

Extension method `AddInfrastructure()` that registers:
- `AppDbContext` with SQL Server connection string
- `JwtSettings` bound from `appsettings.json` via `services.Configure<JwtSettings>(...)` — Added in M3
- `IUnitOfWork` → `UnitOfWork` (Scoped)
- `IJwtTokenService` → `JwtTokenService` (Scoped) — Added in M3

Note: Individual repositories are NOT registered in DI — they're created internally by UnitOfWork with lazy initialization.

#### 3.3.7 Migrations (`Migrations/`)

EF Core migrations that create/modify the database schema:
1. **Initial migration** — creates Departments and Employees tables
2. **20260304092250_A_E_AppUsers** — creates AppUsers table (Added in M3)

Run `dotnet ef database update` from the EMS_Infrastructure project to apply pending migrations.

---

### 3.4 EMS_API — The Presentation Layer

**Location:** `EMS_API/`
**NuGet packages:**
- `Microsoft.AspNetCore.OpenApi` (10.0.2) — OpenAPI metadata
- `Microsoft.EntityFrameworkCore.Design` (10.0.3) — Design-time support for migrations
- `Swashbuckle.AspNetCore` (10.1.4) — Swagger UI
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.3) — JWT token validation middleware (Added in M3)

#### 3.4.1 Program.cs — The Entry Point

The entire app startup in one file:

```
1. Create builder
2. Register services:
   - AddControllers()
   - AddSwaggerGen()
   - AddApplication()       ← services, validators, auth service
   - AddInfrastructure()    ← DbContext, UnitOfWork, JwtTokenService, JwtSettings
   - AddAuthentication()    ← JWT Bearer scheme config (M3)
   - AddAuthorization()     ← enables [Authorize] attribute (M3)
3. Build the app
4. Configure middleware pipeline (ORDER MATTERS):
   - Swagger (dev only)
   - ExceptionHandlingMiddleware  ← catches all exceptions
   - HTTPS redirection
   - UseAuthentication()          ← WHO are you? Reads JWT, sets HttpContext.User (M3)
   - UseAuthorization()           ← Are you ALLOWED? Checks [Authorize], roles (M3)
   - MapControllers
5. Run
```

**Middleware order matters.** The exception middleware is placed early so it catches exceptions from everything downstream (controllers, services, etc.). `UseAuthentication()` MUST come before `UseAuthorization()` — authentication reads the JWT and sets the user identity, then authorization checks that identity against `[Authorize]` rules. Reverse them and authorization runs before the identity is known.

#### 3.4.1a JWT Authentication Configuration (Added in M3)

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // Token must be issued by "EMS_API"
        ValidateAudience = true,         // Token must be for "EMS_Client"
        ValidateLifetime = true,         // Reject expired tokens
        ValidateIssuerSigningKey = true, // Verify signature with our secret key
        ValidIssuer = "EMS_API",
        ValidAudience = "EMS_Client",
        IssuerSigningKey = new SymmetricSecurityKey(...),
        ClockSkew = TimeSpan.Zero        // No tolerance for expired tokens
    };
});
```

**What each validation does:**
- **ValidateIssuer:** Checks the `iss` claim in the JWT matches "EMS_API". Prevents tokens from other systems being accepted.
- **ValidateAudience:** Checks the `aud` claim matches "EMS_Client". Ensures the token was meant for this API.
- **ValidateLifetime:** Checks the `exp` claim. Rejects tokens past their expiry time.
- **ValidateIssuerSigningKey:** Verifies the token signature using our secret key. Ensures the token wasn't tampered with.
- **ClockSkew = TimeSpan.Zero:** By default, .NET adds 5 minutes of tolerance — a token expired 4 minutes ago would still be accepted. We set it to zero for precise expiry. In production with distributed systems, you might want 30 seconds.

**DefaultAuthenticateScheme / DefaultChallengeScheme:** Tells .NET which scheme to use when `[Authorize]` is encountered. "Challenge" means what to do when auth fails — JWT Bearer returns 401 Unauthorized.

#### 3.4.2 Exception Handling Middleware (`Middleware/ExceptionHandlingMiddleware.cs`)

A custom middleware class that wraps the entire request pipeline in a try-catch.

**How middleware works in .NET:**
Every request flows through a pipeline of middleware, like layers of an onion. Each middleware can:
1. Do something before passing to the next
2. Call `await _next(context)` to pass to the next middleware
3. Do something after (or catch exceptions from) the next middleware

**What this middleware does:**
```csharp
try
{
    await _next(context);  // Let the request continue
}
catch (Exception ex)
{
    await HandleExceptionAsync(context, ex);  // Catch anything that throws
}
```

Uses a **switch expression with pattern matching** to map exception types:

| Exception | Status | Response |
|---|---|---|
| `NotFoundException` | 404 | `{ success: false, message: "Department (5) was not found." }` |
| `ValidationException` | 422 | `{ success: false, message: "...", errors: { "Email": [...] } }` |
| `BadRequestException` | 400 | `{ success: false, message: "Invalid email or password." }` |
| Anything else | 500 | `{ success: false, message: "An internal server error occurred." }` |

**The 500 case is critical:** It uses a generic message, never exposing stack traces or internal details. This prevents information leakage in production. An attacker should never see your internal exception details.

JSON is serialized with `JsonNamingPolicy.CamelCase` to match JavaScript/frontend conventions (e.g., `propertyName` not `PropertyName`).

#### 3.4.3 Controllers (`Controllers/`)

All controllers are thin — no try-catch, no validation logic. They call the service and wrap the result in `ApiResponse<T>`. The exception middleware handles all errors.

**AuthController** (`Controllers/AuthController.cs`) — Added in M3:
- **No `[Authorize]`** — all endpoints are anonymous (you can't require auth on the endpoints that give you auth)
- 3 POST endpoints calling IAuthService

| HTTP Method | Route | Auth | Action | Returns |
|---|---|---|---|---|
| POST | /api/auth/register | Public | Register new user | 200 + AuthResponse (tokens) |
| POST | /api/auth/login | Public | Login | 200 + AuthResponse (tokens) |
| POST | /api/auth/refresh | Public | Refresh tokens | 200 + AuthResponse (new tokens) |

The refresh endpoint takes a `RefreshTokenRequest` DTO (just `{ "refreshToken": "..." }`) — this avoids passing a raw string as the body.

**DepartmentController** (`Controllers/DepartmentController.cs`):
- **Class-level `[Authorize]`** — all endpoints require authentication
- Write operations restricted to Admin via `[Authorize(Roles = "Admin")]`

| HTTP Method | Route | Auth | Action | Returns |
|---|---|---|---|---|
| GET | /api/department | Any authenticated | GetAll | 200 + list |
| GET | /api/department/{id} | Any authenticated | GetById | 200 + single |
| POST | /api/department | Admin only | Create | 201 + created (CreatedAtAction) |
| PUT | /api/department/{id} | Admin only | Update | 200 + updated |
| DELETE | /api/department/{id} | Admin only | Delete | 200 + success message |

**EmployeeController** (`Controllers/EmployeeController.cs`):
- **Class-level `[Authorize]`** — all endpoints require authentication
- Write operations restricted by role

| HTTP Method | Route | Auth | Action | Returns |
|---|---|---|---|---|
| GET | /api/employee | Any authenticated | GetAll | 200 + list |
| GET | /api/employee/{id} | Any authenticated | GetById | 200 + single |
| POST | /api/employee | Admin or HR | Create | 201 + created (CreatedAtAction) |
| PUT | /api/employee/{id} | Admin or HR | Update | 200 + updated |
| DELETE | /api/employee/{id} | Admin only | Delete | 200 + success message |

**How role-based `[Authorize]` works:**
1. `JwtTokenService` puts `ClaimTypes.Role` (e.g., "Admin") in the JWT claims when generating the token
2. When a request arrives with `Authorization: Bearer <token>`, the JWT Bearer middleware validates the token and extracts claims into `HttpContext.User`
3. `[Authorize(Roles = "Admin")]` calls `User.IsInRole("Admin")` which looks for a `ClaimTypes.Role` claim with that value
4. `[Authorize(Roles = "Admin,HR")]` means either role is accepted (OR logic)
5. This is automatic — but only because we used `ClaimTypes.Role` specifically. A custom claim type like `"role"` would NOT work with `[Authorize(Roles)]`

**`CreatedAtAction`:** For POST endpoints, returns 201 status with a `Location` header pointing to the GET endpoint for the newly created resource. This is proper REST.

---

## 4. How Requests Flow Through the System

### 4.1 Normal CRUD flow (e.g., Create Department — requires Admin role)

`POST /api/department` with header `Authorization: Bearer <jwt-token>` and body `{ "name": "HR", "code": "HR01" }`

```
1. HTTP Request hits Program.cs pipeline

2. ExceptionHandlingMiddleware.InvokeAsync()
   └─ calls await _next(context) — passes to next middleware

3. UseAuthentication() — reads JWT from Authorization header
   └─ Validates signature, issuer, audience, expiry
   └─ Extracts claims (sub, email, role) → sets HttpContext.User

4. UseAuthorization() — checks [Authorize(Roles = "Admin")]
   └─ Calls User.IsInRole("Admin") → checks ClaimTypes.Role claim
   └─ If not Admin → 403 Forbidden (never reaches controller)

5. Routing middleware matches → DepartmentController.Create()

6. Controller calls _departmentService.CreateDepartmentAsync(request)

5. DepartmentService:
   a. Validates request with _createValidator.ValidateAsync(request)
      → If invalid: throws ValidationException with error dictionary → middleware catches → 422
   b. request.ToEntity() — maps DTO → Department entity
   c. Sets CreatedAt = DateTime.UtcNow
   d. _unitOfWork.Departments.AddAsync(entity) — tracks in EF Change Tracker
   e. _unitOfWork.SaveChangesAsync() — generates INSERT SQL, executes against DB
   f. department.ToResponse() — maps Entity → DepartmentResponse DTO
   g. Returns DepartmentResponse

6. Controller wraps in ApiResponse<DepartmentResponse>.SuccessResponse()
   Returns CreatedAtAction (201) with Location header

7. Response flows back through middleware pipeline → client
```

### 4.2 Error flow (e.g., Department not found)

`GET /api/department/999`

```
1-3. Same as above — routes to DepartmentController.GetById(999)

4. Controller calls _departmentService.GetDepartmentByIdAsync(999)

5. Service: FindAsync(d => d.Id == 999 && d.IsActive) returns null
   → throws NotFoundException("Department", 999)

6. Exception bubbles up through the call stack — out of service, out of controller

7. ExceptionHandlingMiddleware catches it
   → switch matches NotFoundException
   → Sets status 404
   → Writes ApiResponse<object>.FailResponse("Department (999) was not found.")

8. JSON response → client
```

### 4.3 Validation failure flow

`POST /api/department` with body `{ "name": "", "code": "lowercase" }`

```
1-4. Same routing → DepartmentController → DepartmentService

5. Service calls _createValidator.ValidateAsync(request)
   → Returns IsValid = false with errors:
     - Name: "Name is required."
     - Code: "Code must contain only uppercase letters and digits."

6. Service: validationResult.ToErrorDictionary() converts errors to dictionary
   → throws ValidationException({ "Name": ["Name is required."], "Code": ["Code must..."] })

7. ExceptionHandlingMiddleware catches it
   → switch matches ValidationException
   → Sets status 422
   → Writes ApiResponse with message + errors dictionary

8. JSON response with field-level errors → client
```

### 4.4 Authentication flow (Register)

`POST /api/auth/register` with body `{ "firstName": "John", "lastName": "Doe", "email": "john@example.com", "password": "MyPassword123" }`

```
1-3. Routing → AuthController.Register() (to be built)

4. Controller calls _authService.RegisterAsync(request)

5. AuthService:
   a. Validates with _registerValidator (FirstName, LastName, Email format, Password min 8)
   b. Checks email uniqueness: FindAsync(u => u.Email == request.Email)
      → If exists: throws BadRequestException("A user with this email already exists.")
   c. Creates AppUser entity with BCrypt.HashPassword(request.Password)
      → The password "MyPassword123" becomes something like "$2a$11$xyz..." (irreversible)
   d. Calls _jwtTokenService.GenerateAccessToken(user)
      → Creates JWT with claims { sub: "1", email: "john@example.com", role: "Employee" }
      → Signs with HMAC-SHA256 using SecretKey from config
      → Returns (tokenString, expiresAt) tuple
   e. Calls _jwtTokenService.GenerateRefreshToken()
      → Generates 64 random bytes → base64 string
   f. Sets user.RefreshToken = refreshToken, user.RefreshTokenExpiryDate = now + 7 days
   g. Saves user to database (AddAsync + SaveChangesAsync)
   h. Returns AuthResponse { Token, RefreshToken, ExpiresAt, Email, Role }

6. Controller wraps in ApiResponse<AuthResponse>.SuccessResponse() → 200

7. Client stores both tokens:
   - Access token → sent in Authorization header for protected endpoints
   - Refresh token → used to get new access token when it expires
```

### 4.5 Authentication flow (Login)

`POST /api/auth/login` with body `{ "email": "john@example.com", "password": "MyPassword123" }`

```
1-4. Routes to AuthService.LoginAsync()

5. AuthService:
   a. Validates email format and password not empty
   b. Finds user: FindAsync(u => u.Email == email && u.IsActive)
   c. SECURITY CHECK: Combined null check + password verify:
      if (user is null || !BCrypt.Verify(password, user.PasswordHash))
         throw BadRequestException("Invalid email or password.")
      ↑ Same error whether email doesn't exist OR password is wrong
      ↑ This prevents USER ENUMERATION (attacker can't discover which emails are registered)
   d. Generates new access + refresh tokens
   e. Updates user's refresh token in DB (old one invalidated)
   f. Returns AuthResponse

6. Returns tokens to client
```

### 4.6 Token refresh flow

`POST /api/auth/refresh` with body `{ "refreshToken": "abc123..." }`

```
1-4. Routes to AuthService.RefreshTokenAsync()

5. AuthService:
   a. Finds user by refresh token: FindAsync(u => u.RefreshToken == refreshToken && u.IsActive)
      → If no match: NotFoundException (token doesn't exist or was already rotated)
   b. Checks expiry: if RefreshTokenExpiryDate < DateTime.UtcNow
      → BadRequestException("Refresh token has expired. Please login again.")
   c. Generates NEW access token + NEW refresh token
      ↑ TOKEN ROTATION: the old refresh token is replaced, can never be used again
   d. Updates user with new tokens + new expiry
   e. Returns new AuthResponse with fresh tokens

6. Client replaces its stored tokens with the new ones
```

---

## 5. Security Decisions Explained

| Decision | Why |
|---|---|
| **BCrypt for passwords** | Intentionally slow (configurable work factor), auto-salts each hash. SHA256 is too fast (enables brute-force). |
| **Same error for wrong email/password** | Prevents user enumeration — attackers can't probe to discover which emails are registered. |
| **Refresh token rotation** | If a token is stolen, it only works once. Next refresh by legitimate user invalidates the stolen one. |
| **500 errors hide details** | Stack traces contain file paths, class names, line numbers — goldmine for attackers. Generic message only. |
| **JWT claims include role** | Authorization decisions can be made without a database call on every request. |
| **Short-lived access tokens (30 min)** | Limits the damage window if stolen. Combined with refresh tokens, the user doesn't notice. |
| **Refresh token expiry (7 days)** | Forces periodic re-authentication. Balances security with user convenience. |
| **Secret key ≥ 32 chars** | HMAC-SHA256 needs at least 256 bits (32 bytes) for security. Shorter keys are vulnerable. |

---

## 6. Key Design Patterns Used

| Pattern | Where | Why |
|---|---|---|
| **Clean Architecture** | Project structure | Dependency rule keeps business logic independent of frameworks |
| **Repository Pattern** | GenericRepository, specific repos | Abstracts data access, makes it testable |
| **Unit of Work** | UnitOfWork class | Groups repo operations into a single transaction |
| **DTO Pattern** | Request/Response DTOs | Decouples API shape from database shape |
| **Static Factory Method** | ApiResponse\<T\> | Consistent object creation, prevents invalid state |
| **Extension Methods** | Mapping classes, ValidationExtensions | Clean syntax, no need to instantiate helper classes |
| **Middleware Pattern** | ExceptionHandlingMiddleware | Centralized cross-cutting concern (error handling) |
| **Dependency Injection** | DependencyInjection.cs files | Loose coupling, testability, interface-based programming |
| **Soft Delete** | IsActive flag | Data preservation, audit-friendly |
| **Options Pattern** | JwtSettings + IOptions\<T\> | Strongly-typed config, compile-time safety, testable |
| **Tuple Return** | IJwtTokenService.GenerateAccessToken | Returns related values together, prevents drift between token and its expiry |
| **Token Rotation** | AuthService.RefreshTokenAsync | Security — invalidates old refresh tokens on every refresh |

---

## 7. Project File Structure (Complete)

```
EMS/
├── EMS.slnx
│
├── EMS_Domain/                          ← INNER CORE (no dependencies)
│   ├── EMS_Domain.csproj
│   ├── Entities/
│   │   ├── Department.cs
│   │   ├── Employee.cs
│   │   └── AppUser.cs                  ← M3
│   └── Enum/
│       ├── Gender.cs
│       └── Role.cs                     ← M3
│
├── EMS_Application/                     ← BUSINESS LOGIC (depends on Domain)
│   ├── EMS_Application.csproj
│   ├── DependencyInjection.cs
│   ├── Common/
│   │   ├── ApiResponse.cs
│   │   ├── JwtSettings.cs              ← M3
│   │   └── ValidationExtensions.cs     ← M3
│   ├── DTO/
│   │   ├── Department/
│   │   │   ├── CreateDepartmentRequest.cs
│   │   │   ├── UpdateDepartmentRequest.cs
│   │   │   └── DepartmentResponse.cs
│   │   ├── Employee/
│   │   │   ├── CreateEmployeeRequest.cs
│   │   │   ├── UpdateEmployeeRequest.cs
│   │   │   └── EmployeeResponse.cs
│   │   └── Auth/                       ← M3
│   │       ├── RegisterRequest.cs
│   │       ├── LoginRequest.cs
│   │       ├── RefreshTokenRequest.cs
│   │       └── AuthResponse.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── BadRequestException.cs
│   │   └── ValidationException.cs
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   ├── Departments/
│   │   │   ├── IDepartmentRepository.cs
│   │   │   └── IDepartmentService.cs
│   │   ├── Employees/
│   │   │   ├── IEmployeeRepository.cs
│   │   │   └── IEmployeeService.cs
│   │   └── AppUsers/                   ← M3
│   │       ├── IAppUserRepository.cs
│   │       ├── IAuthService.cs
│   │       └── IJwtTokenService.cs
│   ├── Mapping/
│   │   ├── DepartmentMapping.cs
│   │   └── EmployeeMapping.cs
│   ├── Services/
│   │   ├── DepartmentService.cs
│   │   ├── EmployeeService.cs
│   │   └── AuthService.cs             ← M3
│   └── Validators/
│       ├── CreateDepartmentValidator.cs
│       ├── UpdateDepartmentValidator.cs
│       ├── CreateEmployeeValidator.cs
│       ├── UpdateEmployeeValidator.cs
│       ├── RegisterRequestValidator.cs  ← M3
│       └── LoginRequestValidator.cs     ← M3
│
├── EMS_Infrastructure/                  ← DATA ACCESS + EXTERNAL SERVICES (depends on Application)
│   ├── EMS_Infrastructure.csproj
│   ├── DependencyInjection.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   │       ├── DepartmentConfiguration.cs
│   │       ├── EmployeeConfiguration.cs
│   │       └── AppUserConfiguration.cs  ← M3
│   ├── Repositories/
│   │   ├── GenericRepository.cs
│   │   ├── DepartmentRepository.cs
│   │   ├── EmployeeRepository.cs
│   │   └── AppUserRepository.cs        ← M3
│   ├── Services/                        ← M3
│   │   └── JwtTokenService.cs
│   ├── UnitOfWork/
│   │   └── UnitOfWork.cs
│   └── Migrations/
│       ├── (initial migration files)
│       └── 20260304092250_A_E_AppUsers.cs  ← M3
│
└── EMS_API/                             ← PRESENTATION (depends on all)
    ├── EMS_API.csproj
    ├── Program.cs
    ├── appsettings.json                 ← includes JwtSettings section (M3)
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    └── Controllers/
        ├── AuthController.cs          ← M3
        ├── DepartmentController.cs
        └── EmployeeController.cs
```

---

## 8. NuGet Packages

| Project | Package | Version | Purpose |
|---|---|---|---|
| EMS_Domain | (none) | — | Pure C# — no external dependencies |
| EMS_Application | FluentValidation.DependencyInjectionExtensions | 12.1.1 | Validation rules + DI auto-registration |
| EMS_Application | Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.3 | `IServiceCollection` for DI extension methods |
| EMS_Application | BCrypt.Net-Next | 4.1.0 | Password hashing (M3) |
| EMS_Application | Microsoft.Extensions.Options | 10.0.3 | `IOptions<T>` for strongly-typed config (M3) |
| EMS_Infrastructure | Microsoft.EntityFrameworkCore.SqlServer | 10.0.3 | SQL Server database provider |
| EMS_Infrastructure | Microsoft.EntityFrameworkCore.Tools | 10.0.3 | Migrations CLI (`dotnet ef`) |
| EMS_API | Microsoft.AspNetCore.OpenApi | 10.0.2 | OpenAPI metadata |
| EMS_API | Microsoft.EntityFrameworkCore.Design | 10.0.3 | Design-time support for migrations |
| EMS_API | Swashbuckle.AspNetCore | 10.1.4 | Swagger UI |
| EMS_API | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.3 | JWT token validation middleware (M3) |

---

## 9. Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=EMS_DB;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "EMS_API",
    "Audience": "EMS_Client",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  }
}
```

- **ConnectionString:** Uses Windows Authentication (`Trusted_Connection=true`) with local SQL Server (`.`)
- **SecretKey:** Must be ≥32 characters for HMAC-SHA256. In production, this should come from environment variables or a secrets manager — never hardcoded in source control
- **Issuer/Audience:** Validated when receiving tokens — must match what was used when generating
- **AccessTokenExpirationMinutes:** 30 minutes — short-lived for security
- **RefreshTokenExpirationDays:** 7 days — forces re-login weekly

---

## 10. What's Still TODO

### Completed Milestones:
- **Milestone 1:** Project Setup & Clean Architecture (Score: 7.5/10) ✓
- **Milestone 2:** DTOs, Validation & Global Error Handling (Score: 8.5/10) ✓
- **Milestone 3:** Authentication & Authorization (Score: 8.5/10) ✓

### Milestone 4: Advanced Querying & Performance (NOT STARTED)
- Pagination (PagedList<T>)
- Filtering & Searching (dynamic query building)
- Sorting (dynamic, by any property)
- Specification Pattern
- In-Memory Caching (IMemoryCache)
- Response caching headers
- Attendance entity + endpoints
- EF Core query optimization (AsNoTracking, Select projections)

### Milestone 5: CQRS with MediatR (NOT STARTED)
- MediatR setup (Commands & Queries)
- CQRS pattern (separate read/write models)
- Pipeline Behaviors (logging, validation)
- Leave Request feature (Apply, Approve, Reject)
- Domain Events + Notification handlers

### Milestone 6: Background Jobs, Logging & Polish (NOT STARTED)
- Serilog structured logging
- Correlation ID middleware
- Background jobs (Hosted Service)
- API Versioning
- Rate Limiting
- Health Checks
- Unit Tests (xUnit + Moq)
