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

## Current Project Structure (ACTUAL — as of M2 ~80% complete)
```
EMS/
├── EMS_API/
│   ├── Controllers/
│   │   ├── DepartmentController.cs
│   │   └── EmployeeController.cs
│   ├── Program.cs
│   └── EMS_API.csproj
├── EMS_Application/
│   ├── Common/
│   │   └── ApiResponse.cs                     ← NEW in M2
│   ├── DTO/
│   │   ├── Department/
│   │   │   ├── CreateDepartmentRequest.cs
│   │   │   ├── UpdateDepartmentRequest.cs
│   │   │   └── DepartmentResponse.cs
│   │   └── Employee/
│   │       ├── CreateEmployeeRequest.cs
│   │       ├── UpdateEmployeeRequest.cs
│   │       └── EmployeeResponse.cs
│   ├── Exceptions/
│   │   ├── BadRequestException.cs             ← NEW in M2
│   │   ├── NotFoundException.cs               ← NEW in M2
│   │   └── ValidationException.cs             ← NEW in M2
│   ├── Interfaces/
│   │   ├── Departments/
│   │   │   ├── IDepartmentService.cs
│   │   │   └── IDepartmentRepository.cs
│   │   ├── Employees/
│   │   │   ├── IEmployeeService.cs
│   │   │   └── IEmployeeRepository.cs
│   │   ├── IGenericRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   ├── DepartmentMapping.cs
│   │   └── EmployeeMapping.cs
│   ├── Services/
│   │   ├── DepartmentService.cs
│   │   └── EmployeeService.cs
│   ├── Validators/
│   │   ├── CreateDepartmentValidator.cs       ← NEW in M2
│   │   ├── UpdateDepartmentValidator.cs       ← NEW in M2
│   │   ├── CreateEmployeeValidator.cs         ← NEW in M2
│   │   └── UpdateEmployeeValidator.cs         ← NEW in M2
│   ├── DependencyInjection.cs
│   └── EMS_Application.csproj
├── EMS_Domain/
│   ├── Entities/
│   │   ├── Department.cs
│   │   └── Employee.cs
│   ├── Enum/
│   │   └── Gender.cs
│   └── EMS_Domain.csproj
└── EMS_Infrastructure/
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Configurations/
    │       ├── DepartmentConfiguration.cs
    │       └── EmployeeConfiguration.cs
    ├── Repositories/
    │   ├── GenericRepository.cs
    │   ├── DepartmentRepository.cs
    │   └── EmployeeRepository.cs
    ├── UnitOfWork/
    │   └── UnitOfWork.cs
    ├── Migrations/
    ├── DependencyInjection.cs
    └── EMS_Infrastructure.csproj
```

**Dependency Rule:** Domain depends on nothing. Application depends on Domain. Infrastructure depends on Application. API depends on all.

**NuGet Packages installed:**
- EMS_API: Microsoft.AspNetCore.OpenApi, Microsoft.EntityFrameworkCore.Design, Swashbuckle.AspNetCore
- EMS_Application: Microsoft.Extensions.DependencyInjection.Abstractions, FluentValidation.DependencyInjectionExtensions (12.1.1)
- EMS_Infrastructure: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools
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

---

## All 6 Milestones Overview

| # | Milestone | Status |
|---|---|---|
| 1 | Project Setup & Clean Architecture | COMPLETED (Score: 7.5/10) |
| 2 | DTOs, Validation & Error Handling | IN PROGRESS (~80%) |
| 3 | Authentication & Authorization | Not Started |
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

## Milestone 2 — Current Status (IN PROGRESS ~80%)

### What's DONE:
- [x] All 6 DTOs: CreateDepartmentRequest, UpdateDepartmentRequest, DepartmentResponse, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeResponse
- [x] Manual mapping extension methods: DepartmentMapping (ToResponse, ToEntity, ApplyUpdate), EmployeeMapping (ToResponse, ToEntity, ApplyUpdate)
- [x] Service interfaces updated: accept DTOs, return Response DTOs
- [x] Services refactored: use mapping, use FindAsync consistently
- [x] Controllers updated: use Request DTOs
- [x] Employee queries include Department navigation (for DepartmentName in response)
- [x] Domain restructured: Entities/ folder, Enum/ folder
- [x] Swagger added to Program.cs
- [x] Custom Exceptions created (NotFoundException, BadRequestException, ValidationException)
- [x] ApiResponse<T> wrapper created with factory methods (SuccessResponse, FailResponse)
- [x] FluentValidation validators created (4 validators with all rules)
- [x] FluentValidation package installed (FluentValidation.DependencyInjectionExtensions 12.1.1)
- [x] Validators registered in DependencyInjection.cs via AddValidatorsFromAssembly

### What's REMAINING:

#### 1. Fix uppercase validator null-safety bug
In CreateDepartmentValidator and UpdateDepartmentValidator, the `.Must(code => code == code.ToUpper())` rule will throw NullReferenceException if Code is null. Fix with null guard: `.Must(code => code != null && code == code.ToUpper())` or use `.Matches(@"^[A-Z]+$")`.

#### 2. Global Exception Handling Middleware
Create in `EMS_API/Middleware/ExceptionHandlingMiddleware.cs`:
- Wrap `await _next(context)` in try-catch
- Catch custom exceptions and map to HTTP status codes:

| Exception Type | HTTP Status Code |
|---|---|
| NotFoundException | 404 |
| ValidationException | 422 |
| BadRequestException | 400 |
| Any other exception | 500 ("Internal server error" — don't leak details) |

- Return `ApiResponse<object>.FailResponse(...)` as JSON
- For ValidationException, include the Errors dictionary
- Register in Program.cs with `app.UseMiddleware<ExceptionHandlingMiddleware>()`

#### 3. Replace KeyNotFoundException with NotFoundException in Services
In DepartmentService and EmployeeService, replace:
```csharp
throw new KeyNotFoundException($"Department with ID {id} not found.");
```
With:
```csharp
throw new NotFoundException("Department", id);
```

#### 4. Clean up controllers
Remove all try-catch blocks from controllers. The middleware handles everything now.

### Implementation Details of Completed Work:

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

#### Validators (EMS_Application/Validators/)
**Department validators (Create + Update):**
- Name: NotEmpty, Length(2, 100)
- Code: NotEmpty, Length(2, 10), Must be uppercase (needs null guard fix)
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

#### DependencyInjection.cs (EMS_Application)
Registers: DepartmentService, EmployeeService, and all validators via `AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())`

---

## Rules for Milestone 2
- NO AutoMapper — manual mapping only (already done)
- Controllers must be thin — no try-catch after middleware
- Validation happens in the service layer (inject IValidator), not controller
- Middleware handles ALL exceptions
- Validation calls custom ValidationException with error dictionary

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

### After Milestone 2 (answer when done):
1. Why use DTOs instead of exposing entities directly?
2. What is FluentValidation and why over Data Annotations?
3. How does middleware work in .NET? What is the request pipeline?
4. Why manual mapping over AutoMapper? Trade-offs?
5. Where should validation happen — controller, service, or both?

---

## Key Files
- `INSTRUCTOR_NOTES.md` — Instructor's private tracking (strengths, weaknesses, review history)
- `MILESTONES.md` — Full milestone roadmap with status
- `ASSISTANT_CONTEXT.md` — This file. Full context for assistant Claude
