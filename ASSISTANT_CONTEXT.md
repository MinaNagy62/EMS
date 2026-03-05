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

## Current Project Structure (ACTUAL — M3 COMPLETED)
```
EMS/
├── EMS_API/
│   ├── Controllers/
│   │   ├── AuthController.cs                      ← M3
│   │   ├── DepartmentController.cs                ← [Authorize] added M3
│   │   └── EmployeeController.cs                  ← [Authorize] added M3
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Program.cs                                 ← JWT auth config added M3
│   ├── appsettings.json                           ← JwtSettings section added M3
│   └── EMS_API.csproj
├── EMS_Application/
│   ├── Common/
│   │   ├── ApiResponse.cs
│   │   ├── JwtSettings.cs                         ← M3
│   │   └── ValidationExtensions.cs                ← M3
│   ├── DTO/
│   │   ├── Department/
│   │   │   ├── CreateDepartmentRequest.cs
│   │   │   ├── UpdateDepartmentRequest.cs
│   │   │   └── DepartmentResponse.cs
│   │   ├── Employee/
│   │   │   ├── CreateEmployeeRequest.cs
│   │   │   ├── UpdateEmployeeRequest.cs
│   │   │   └── EmployeeResponse.cs
│   │   └── Auth/                                  ← M3
│   │       ├── AuthResponse.cs
│   │       ├── LoginRequest.cs
│   │       ├── RegisterRequest.cs
│   │       └── RefreshTokenRequest.cs
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
│   │   ├── AppUsers/                              ← M3
│   │   │   ├── IAuthService.cs
│   │   │   ├── IJwtTokenService.cs
│   │   │   └── IAppUserRepository.cs
│   │   ├── IGenericRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   ├── DepartmentMapping.cs
│   │   └── EmployeeMapping.cs
│   ├── Services/
│   │   ├── DepartmentService.cs
│   │   ├── EmployeeService.cs
│   │   └── AuthService.cs                         ← M3
│   ├── Validators/
│   │   ├── CreateDepartmentValidator.cs
│   │   ├── UpdateDepartmentValidator.cs
│   │   ├── CreateEmployeeValidator.cs
│   │   ├── UpdateEmployeeValidator.cs
│   │   ├── RegisterRequestValidator.cs            ← M3
│   │   └── LoginRequestValidator.cs               ← M3
│   ├── DependencyInjection.cs
│   └── EMS_Application.csproj
├── EMS_Domain/
│   ├── Entities/
│   │   ├── Department.cs
│   │   ├── Employee.cs
│   │   └── AppUser.cs                             ← M3
│   ├── Enum/
│   │   ├── Gender.cs
│   │   └── Role.cs                                ← M3
│   └── EMS_Domain.csproj
└── EMS_Infrastructure/
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Configurations/
    │       ├── DepartmentConfiguration.cs
    │       ├── EmployeeConfiguration.cs
    │       └── AppUserConfiguration.cs             ← M3
    ├── Repositories/
    │   ├── GenericRepository.cs
    │   ├── DepartmentRepository.cs
    │   ├── EmployeeRepository.cs
    │   └── AppUserRepository.cs                    ← M3
    ├── Services/
    │   └── JwtTokenService.cs                      ← M3
    ├── UnitOfWork/
    │   └── UnitOfWork.cs
    ├── Migrations/
    ├── DependencyInjection.cs
    └── EMS_Infrastructure.csproj
```

**Dependency Rule:** Domain depends on nothing. Application depends on Domain. Infrastructure depends on Application. API depends on all.

**NuGet Packages installed:**
- EMS_API: Microsoft.AspNetCore.OpenApi (10.0.2), Microsoft.EntityFrameworkCore.Design (10.0.3), Swashbuckle.AspNetCore (10.1.4), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.3)
- EMS_Application: Microsoft.Extensions.DependencyInjection.Abstractions (10.0.3), FluentValidation.DependencyInjectionExtensions (12.1.1), BCrypt.Net-Next (4.1.0), Microsoft.Extensions.Options (10.0.3)
- EMS_Infrastructure: Microsoft.EntityFrameworkCore.SqlServer (10.0.3), Microsoft.EntityFrameworkCore.Tools (10.0.3)
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

### AppUser (EMS_Domain/Entities/AppUser.cs)
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
| 3 | Authentication & Authorization | COMPLETED (Score: 8.5/10) |
| 4 | Advanced Querying & Performance | Not Started |
| 5 | CQRS with MediatR | Not Started |
| 6 | Background Jobs, Logging & Polish | Not Started |

---

## Milestone 3 — COMPLETED (Score: 8.5/10)

### Auth Endpoints:
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | /api/auth/register | Public | Register new user, returns tokens |
| POST | /api/auth/login | Public | Login, returns tokens |
| POST | /api/auth/refresh | Public | Refresh expired access token |

### Authorization Matrix:
| Endpoint | Admin | HR | Employee |
|---|---|---|---|
| GET departments | ✓ | ✓ | ✓ |
| POST/PUT/DELETE departments | ✓ | ✗ | ✗ |
| GET employees | ✓ | ✓ | ✓ |
| POST/PUT employees | ✓ | ✓ | ✗ |
| DELETE employees | ✓ | ✗ | ✗ |

### Key Implementation Details:

**Program.cs JWT config:**
- AddAuthentication with JwtBearerDefaults
- TokenValidationParameters: ValidateIssuer, ValidateAudience, ValidateLifetime, ValidateIssuerSigningKey
- ClockSkew = TimeSpan.Zero (no 5-min tolerance)
- Pipeline: UseAuthentication() before UseAuthorization()

**AuthController:** Thin, no [Authorize], no try-catch. 3 POST endpoints calling IAuthService.

**AuthService flows:**
- Register: validate → check email unique → BCrypt hash → generate tokens → save → return AuthResponse
- Login: validate → find user → combined error for wrong email/password → generate tokens → save → return
- Refresh: find by token → check expiry → rotate tokens → save → return

**Security:**
- BCrypt password hashing
- User enumeration prevention (same error for wrong email/password)
- Refresh token rotation
- ClockSkew = Zero
- Role-based [Authorize] on controllers

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

### How [Authorize(Roles)] Works
- JwtTokenService puts `ClaimTypes.Role` claim in the JWT
- JWT Bearer middleware reads token, extracts claims, sets `HttpContext.User`
- `[Authorize(Roles = "Admin")]` calls `User.IsInRole("Admin")` which looks for `ClaimTypes.Role` claim
- Must use `ClaimTypes.Role` specifically (not just "role") for this to work automatically
- `UseAuthentication()` must come before `UseAuthorization()` in pipeline

### ClockSkew = TimeSpan.Zero
- Default ClockSkew is 5 minutes — a token expired 4 minutes ago would still be accepted
- We set it to zero for precise expiry control
- In production with distributed systems, you might want a small skew (30 seconds)

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

### After Milestone 3:
1. How does JWT authentication work? What are the 3 parts of a JWT?
2. What is the difference between authentication and authorization?
3. Why BCrypt over SHA256 for password hashing?
4. What is the refresh token flow and why not just use a long-lived access token?
5. What is user enumeration and how do you prevent it?
6. Why is JwtTokenService in Infrastructure and not Application?
7. What is the Options pattern and why use it over IConfiguration?
8. What is token rotation and why is it important?
9. How does [Authorize(Roles)] work under the hood with ClaimTypes.Role?
10. Why set ClockSkew to TimeSpan.Zero?

---

## Key Files
- `INSTRUCTOR_NOTES.md` — Instructor's private tracking (strengths, weaknesses, review history)
- `MILESTONES.md` — Full milestone roadmap with status
- `ASSISTANT_CONTEXT.md` — This file. Full context for assistant Claude
- `PROJECT_TECHNICAL_GUIDE.md` — Comprehensive technical documentation of the full codebase
