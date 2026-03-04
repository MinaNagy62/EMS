# EMS Project - Assistant Context

## Role
You are an assistant helping me (Minan) build this project. The project owner/instructor is another Claude Code session that reviews my code. You help me write code, answer questions, and implement milestones. Follow the exact specifications below — don't deviate from what's been decided.

## Who I Am
I am a .NET developer with 3 years of experience. I'm building this project to master backend patterns and prepare for interviews.

## Project: Employee Management System (EMS)
- **Tech Stack:** .NET 10 Web API, SQL Server, EF Core, JWT Auth
- **Architecture:** Clean Architecture (Onion)
- **Goal:** Small project but covers every important pattern and concept

---

## Current Project Structure (ACTUAL — as of M3 in progress)
```
EMS/
├── EMS_API/
│   ├── Controllers/
│   │   ├── DepartmentController.cs
│   │   └── EmployeeController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── EMS_API.csproj
├── EMS_Application/
│   ├── Common/
│   │   ├── ApiResponse.cs
│   │   ├── JwtSettings.cs                        ← NEW in M3
│   │   └── ValidationExtensions.cs               ← NEW in M3
│   ├── DTO/
│   │   ├── Department/
│   │   │   ├── CreateDepartmentRequest.cs
│   │   │   ├── UpdateDepartmentRequest.cs
│   │   │   └── DepartmentResponse.cs
│   │   ├── Employee/
│   │   │   ├── CreateEmployeeRequest.cs
│   │   │   ├── UpdateEmployeeRequest.cs
│   │   │   └── EmployeeResponse.cs
│   │   └── Auth/
│   │       ├── AuthResponse.cs                    ← NEW in M3
│   │       ├── LoginRequest.cs                    ← NEW in M3
│   │       └── RegisterRequest.cs                 ← NEW in M3
│   ├── Exceptions/
│   │   ├── BadRequestException.cs
│   │   ├── NotFoundException.cs
│   │   └── ValidationException.cs
│   ├── Interfaces/
│   │   ├── Departments/
│   │   │   ├── IDepartmentService.cs
│   │   │   └── IDepartmentRepository.cs
│   │   ├── Employees/
│   │   │   ├── IEmployeeService.cs
│   │   │   └── IEmployeeRepository.cs
│   │   ├── AppUsers/
│   │   │   ├── IAuthService.cs                    ← NEW in M3
│   │   │   ├── IJwtTokenService.cs                ← NEW in M3
│   │   │   └── IAppUserRepository.cs              ← NEW in M3
│   │   ├── IGenericRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   ├── DepartmentMapping.cs
│   │   └── EmployeeMapping.cs
│   ├── Services/
│   │   ├── DepartmentService.cs
│   │   ├── EmployeeService.cs
│   │   └── AuthService.cs                         ← NEW in M3
│   ├── Validators/
│   │   ├── CreateDepartmentValidator.cs
│   │   ├── UpdateDepartmentValidator.cs
│   │   ├── CreateEmployeeValidator.cs
│   │   ├── UpdateEmployeeValidator.cs
│   │   ├── RegisterRequestValidator.cs            ← NEW in M3
│   │   └── LoginRequestValidator.cs               ← NEW in M3
│   ├── DependencyInjection.cs
│   └── EMS_Application.csproj
├── EMS_Domain/
│   ├── Entities/
│   │   ├── Department.cs
│   │   ├── Employee.cs
│   │   └── AppUser.cs                             ← NEW in M3
│   ├── Enum/
│   │   ├── Gender.cs
│   │   └── Role.cs                                ← NEW in M3
│   └── EMS_Domain.csproj
└── EMS_Infrastructure/
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Configurations/
    │       ├── DepartmentConfiguration.cs
    │       ├── EmployeeConfiguration.cs
    │       └── AppUserConfiguration.cs             ← NEW in M3
    ├── Repositories/
    │   ├── GenericRepository.cs
    │   ├── DepartmentRepository.cs
    │   ├── EmployeeRepository.cs
    │   └── AppUserRepository.cs                    ← NEW in M3
    ├── Services/
    │   └── JwtTokenService.cs                      ← NEW in M3
    ├── UnitOfWork/
    │   └── UnitOfWork.cs
    ├── Migrations/
    ├── DependencyInjection.cs
    └── EMS_Infrastructure.csproj
```

**Dependency Rule:** Domain depends on nothing. Application depends on Domain. Infrastructure depends on Application. API depends on all.

**NuGet Packages installed:**
- EMS_API: Microsoft.AspNetCore.OpenApi (10.0.2), Microsoft.EntityFrameworkCore.Design (10.0.3), Swashbuckle.AspNetCore (10.1.4)
- EMS_Application: Microsoft.Extensions.DependencyInjection.Abstractions (10.0.3), FluentValidation.DependencyInjectionExtensions (12.1.1), BCrypt.Net-Next (4.1.0), Microsoft.Extensions.Options (10.0.3)
- EMS_Infrastructure: Microsoft.EntityFrameworkCore.SqlServer (10.0.3), Microsoft.EntityFrameworkCore.Tools (10.0.3), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.3)
- EMS_Domain: none

---

## Entities

### Department (EMS_Domain/Entities/Department.cs)
| Property | Type | Notes |
|---|---|---|
| Id | int | Primary key |
| Name | string | Required, max 100 chars |
| Code | string | Required, unique, max 10 |
| Description | string? | Optional, max 500 |
| IsActive | bool | Default true (soft delete) |
| CreatedAt | DateTime | Set on creation |
| UpdatedAt | DateTime? | Set on update |
| Employees | ICollection\<Employee\> | Nav property, initialized = new List\<Employee\>() |

### Employee (EMS_Domain/Entities/Employee.cs)
| Property | Type | Notes |
|---|---|---|
| Id | int | Primary key |
| FirstName | string | Required, max 50 |
| LastName | string | Required, max 50 |
| Email | string | Required, unique |
| Phone | string? | Optional, max 20 |
| DateOfBirth | DateTime | Required |
| HireDate | DateTime | Required |
| Salary | decimal | Required, precision(18,2) |
| Gender | Gender enum | Male=1, Female=2 (in Domain/Enum/Gender.cs) |
| JobTitle | string | Required, max 100 |
| DepartmentId | int | Foreign key |
| Department | Department | Navigation property |
| IsActive | bool | Default true |
| CreatedAt | DateTime | Set on creation |
| UpdatedAt | DateTime? | Set on update |

### AppUser (EMS_Domain/Entities/AppUser.cs) — NEW in M3
| Property | Type | Notes |
|---|---|---|
| Id | int | Primary key |
| FirstName | string | Required, max 50 |
| LastName | string | Required, max 50 |
| Email | string | Required, unique |
| PasswordHash | string | Required, BCrypt hash |
| Role | Role enum | Default: Employee |
| RefreshToken | string? | Stored for refresh flow |
| RefreshTokenExpiryDate | DateTime? | 7 days from generation |
| IsActive | bool | Default true |
| CreatedAt | DateTime | Set on creation |

### Enums
- **Gender** (Domain/Enum/Gender.cs): Male=1, Female=2
- **Role** (Domain/Enum/Role.cs): Admin=1, HR=2, Employee=3

---

## All 6 Milestones Overview

| # | Milestone | Status |
|---|---|---|
| 1 | Project Setup & Clean Architecture | COMPLETED (Score: 7.5/10) |
| 2 | DTOs, Validation & Error Handling | COMPLETED (Score: 8.5/10) |
| 3 | Authentication & Authorization | IN PROGRESS |
| 4 | Advanced Querying & Performance | Not Started |
| 5 | CQRS with MediatR | Not Started |
| 6 | Background Jobs, Logging & Polish | Not Started |

---

## Milestone 1 Review Summary (COMPLETED — 7.5/10)

### Strengths
- Clean Architecture dependency rule correct
- Fluent API configs clean and complete
- Generic repository with filter + includes
- Lazy initialization in UnitOfWork
- Soft delete correct, DI extensions clean
- OnDelete Restrict, CreatedAtAction proper REST

### All M1 Issues Were Fixed
- Removed unused usings ✓
- Fixed parameter casing ✓
- Renamed interface folders ✓
- Initialized collection nav property ✓
- Added IsActive default ✓
- Consistent FindAsync filtering ✓
- Gender enum in own file ✓
- Entities in Entities/ folder ✓

---

## Milestone 2 — COMPLETED (Score: 8.5/10)

### What was delivered:
- [x] All 6 DTOs: CreateDepartmentRequest, UpdateDepartmentRequest, DepartmentResponse, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeResponse
- [x] Manual mapping extension methods: DepartmentMapping (ToResponse, ToEntity, ApplyUpdate), EmployeeMapping (ToResponse, ToEntity, ApplyUpdate)
- [x] Service interfaces updated: accept DTOs, return Response DTOs
- [x] Services refactored: use mapping, use FindAsync consistently
- [x] Controllers updated: use Request DTOs, thin (no try-catch)
- [x] Employee queries include Department navigation (for DepartmentName in response)
- [x] Domain restructured: Entities/ folder, Enum/ folder
- [x] Swagger added to Program.cs
- [x] Custom Exceptions created (NotFoundException, BadRequestException, ValidationException)
- [x] ApiResponse<T> wrapper created with factory methods (SuccessResponse, FailResponse)
- [x] FluentValidation validators created (4 validators with all rules, null-safe with Matches regex)
- [x] FluentValidation package installed (FluentValidation.DependencyInjectionExtensions 12.1.1)
- [x] Validators registered in DependencyInjection.cs via AddValidatorsFromAssembly
- [x] Validators injected and called in both services (service-layer validation)
- [x] Global Exception Handling Middleware (NotFoundException→404, ValidationException→422, BadRequestException→400, unhandled→500)
- [x] Middleware registered in Program.cs
- [x] NotFoundException uses proper entity names ("Department", "Employee") not variable names

### Implementation Details:

#### Custom Exceptions (EMS_Application/Exceptions/)
- **NotFoundException** — constructor: `(string entityName, object key)`, message: `"{entityName} ({key}) was not found."`
- **BadRequestException** — constructor: `(string message)`
- **ValidationException** — constructor: `(IDictionary<string, string[]> errors)`, has `Errors` property, message: `"One or more validation errors occurred."`

#### ApiResponse<T> (EMS_Application/Common/ApiResponse.cs)
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request completed successfully.")
    public static ApiResponse<T> FailResponse(string message, IDictionary<string, string[]>? errors = null)
}
```

#### ValidationExtensions (EMS_Application/Common/ValidationExtensions.cs)
```csharp
public static class ValidationExtensions
{
    public static IDictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
}
```
Used across all services to convert FluentValidation errors into the dictionary format for ValidationException.

#### Validators (EMS_Application/Validators/)
**Department validators (Create + Update):**
- Name: NotEmpty, Length(2, 100)
- Code: NotEmpty, Length(2, 10), Matches `^[A-Z0-9]+$` (uppercase + digits only)
- Description: MaximumLength(500)

**Employee validators (Create + Update):**
- FirstName: NotEmpty, Length(2, 50)
- LastName: NotEmpty, Length(2, 50)
- Email: NotEmpty, EmailAddress
- Phone: MaximumLength(20)
- DateOfBirth: LessThan(DateTime.Today)
- HireDate: LessThanOrEqualTo(DateTime.Today)
- Salary: GreaterThan(0)
- Gender: IsInEnum
- JobTitle: NotEmpty, Length(2, 100)
- DepartmentId: GreaterThan(0)

#### Middleware (EMS_API/Middleware/ExceptionHandlingMiddleware.cs)
Catches all exceptions, maps to HTTP status codes, returns `ApiResponse<object>.FailResponse(...)` as JSON with camelCase. Uses switch expression with pattern matching. 500 errors return generic message (no detail leak).

#### DependencyInjection.cs (EMS_Application)
Registers: DepartmentService, EmployeeService, AuthService, and all validators via `AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())`

#### DependencyInjection.cs (EMS_Infrastructure)
Registers: AppDbContext (SQL Server), JwtSettings (Options pattern), IUnitOfWork, IJwtTokenService

---

## Milestone 3 — IN PROGRESS (Authentication & Authorization)

### What's DONE:
- [x] AppUser entity with all properties (Id, FirstName, LastName, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiryDate, IsActive, CreatedAt)
- [x] Role enum (Admin=1, HR=2, Employee=3)
- [x] AppUserConfiguration (Fluent API: HasKey, max lengths, unique email index, PasswordHash required)
- [x] AppUserRepository extending GenericRepository<AppUser>
- [x] IAppUserRepository interface
- [x] AppDbContext updated with DbSet<AppUser>
- [x] IUnitOfWork updated with AppUsers property
- [x] UnitOfWork updated with lazy AppUsers initialization
- [x] Migration created (20260304092250_A_E_AppUsers)
- [x] Auth DTOs: AuthResponse (Token, RefreshToken, ExpiresAt, Email, Role), LoginRequest (Email, Password), RegisterRequest (FirstName, LastName, Email, Password)
- [x] IJwtTokenService interface — returns `(string Token, DateTime ExpiresAt)` tuple from GenerateAccessToken
- [x] JwtTokenService implementation — HS256 signing, claims (Sub, Email, Role, Jti), uses IOptions<JwtSettings>, cryptographic refresh token with RandomNumberGenerator
- [x] JwtSettings class (Options pattern) — SecretKey, Issuer, Audience, AccessTokenExpirationMinutes, RefreshTokenExpirationDays
- [x] JwtSettings configured in appsettings.json and registered via services.Configure<JwtSettings>()
- [x] IAuthService interface — RegisterAsync, LoginAsync, RefreshTokenAsync
- [x] AuthService implementation:
  - RegisterAsync: validate → check email unique → BCrypt hash password → generate tokens → save refresh token → return AuthResponse
  - LoginAsync: validate → find user → verify password → generic error message for both wrong email/password (prevents user enumeration) → generate tokens → save → return
  - RefreshTokenAsync: find by refresh token → check not expired → generate new pair (rotation) → save → return
- [x] Auth validators: RegisterRequestValidator (FirstName/LastName 2-50, Email valid, Password min 8), LoginRequestValidator (Email valid, Password required)
- [x] DI registered: IAuthService→AuthService, IJwtTokenService→JwtTokenService
- [x] BCrypt.Net-Next package installed for password hashing
- [x] Microsoft.AspNetCore.Authentication.JwtBearer package installed
- [x] Microsoft.Extensions.Options package installed

### What's REMAINING for M3:
- [ ] AuthController (POST /api/auth/register, POST /api/auth/login, POST /api/auth/refresh)
- [ ] JWT authentication configuration in Program.cs (AddAuthentication, AddJwtBearer with token validation parameters)
- [ ] [Authorize] attributes on Department and Employee controllers
- [ ] Role-based authorization ([Authorize(Roles = "Admin")] on specific endpoints)
- [ ] Test all auth endpoints via Swagger

### Auth Flow:
```
Register → POST /api/auth/register → returns AccessToken + RefreshToken
Login    → POST /api/auth/login    → returns AccessToken + RefreshToken
Refresh  → POST /api/auth/refresh  → returns new AccessToken + RefreshToken
```

### Security decisions made:
- Login returns same error ("Invalid email or password.") for both wrong email and wrong password (prevents user enumeration)
- Refresh tokens are rotated on every refresh (old token invalidated)
- 500 errors never leak internal details
- BCrypt for password hashing (intentionally slow, salted)

---

## Rules for All Milestones
- NO AutoMapper — manual mapping only
- Controllers must be thin — no try-catch (middleware handles everything)
- Validation happens in the service layer (inject IValidator), not controller
- Middleware handles ALL exceptions
- Validation uses ValidationExtensions.ToErrorDictionary() helper
- All responses wrapped in ApiResponse<T>
- Services return DTOs, never entities
- All config values read from settings (no hardcoded magic numbers) — use Options pattern

---

## Concepts Explained by Instructor

### Concurrency in EF Core
- Default: "Last Write Wins" — silent data loss
- Solution: Add `byte[] RowVersion`, configure `.IsRowVersion()` in Fluent API
- EF Core includes RowVersion in UPDATE WHERE clause
- Mismatch → `DbUpdateConcurrencyException`

### AddAsync vs Add / Update vs Remove
- `AddAsync`: async because value generators MAY need DB access (HiLo). With IDENTITY, sync `Add` works fine
- `Update`: sync — only marks Modified in Change Tracker
- `Remove`: sync — only marks Deleted in Change Tracker
- `SaveChangesAsync`: all actual SQL runs here

### Options Pattern
- Create a POCO class matching appsettings.json section (e.g., `JwtSettings`)
- Register with `services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"))`
- Inject `IOptions<JwtSettings>` in services, access via `.Value`
- Better than `IConfiguration` — strongly typed, compile-time safety, testable

### Why JwtTokenService is in Infrastructure
- It depends on framework/crypto libraries (System.IdentityModel.Tokens.Jwt, Microsoft.IdentityModel.Tokens)
- Application layer defines WHAT it needs (IJwtTokenService interface)
- Infrastructure implements HOW (JWT signing, key management)
- Same pattern as repositories — interface in Application, implementation in Infrastructure

### Static Factory Method Pattern
- Used in ApiResponse<T> — SuccessResponse() and FailResponse()
- Ensures consistent object creation (Success=true always set correctly)
- Same pattern as Task.FromResult(), Results.Ok() in .NET framework

---

## Interview Questions to Know

### After Milestone 1:
1. What is Clean Architecture and what problem does it solve?
2. What's the difference between Repository Pattern and Unit of Work?
3. Why use Fluent API instead of Data Annotations?
4. Explain the dependency rule in Clean Architecture
5. How does DI work under the hood in .NET?
6. Why did you use DeleteBehavior.Restrict instead of Cascade?
7. Difference between FindAsync (DbSet) and FirstOrDefaultAsync (LINQ)?
8. Why is Update() sync but AddAsync() async?
9. What happens with concurrent updates? (RowVersion / concurrency tokens)

### After Milestone 2:
1. Why use DTOs instead of exposing entities directly?
2. What is FluentValidation and why over Data Annotations?
3. How does middleware work in .NET? What is the request pipeline?
4. Why manual mapping over AutoMapper? Trade-offs?
5. Where should validation happen — controller, service, or both?

### After Milestone 3 (answer when done):
1. How does JWT authentication work? What are the 3 parts of a JWT?
2. What is the difference between authentication and authorization?
3. Why BCrypt over SHA256 for password hashing?
4. What is the refresh token flow and why not just use a long-lived access token?
5. What is user enumeration and how do you prevent it?
6. Why is JwtTokenService in Infrastructure and not Application?
7. What is the Options pattern and why use it over IConfiguration?
8. What is token rotation and why is it important?

---

## Key Files
- `INSTRUCTOR_NOTES.md` — Instructor's private tracking (strengths, weaknesses, review history)
- `MILESTONES.md` — Full milestone roadmap with status
- `ASSISTANT_CONTEXT.md` — This file. Full context for assistant Claude
- `PROJECT_TECHNICAL_GUIDE.md` — Comprehensive technical documentation of the full codebase
