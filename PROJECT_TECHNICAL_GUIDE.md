# EMS Project — Complete Technical Guide

Everything you need to understand this project, top to bottom.

---

## 1. What Is This Project?

An **Employee Management System (EMS)** REST API. It manages **Departments** and **Employees**. Built with .NET 10, SQL Server, and Entity Framework Core. The goal is to learn and demonstrate backend patterns for interviews.

---

## 2. Architecture: Clean Architecture (Onion)

The project is split into 4 layers. The core rule: **inner layers never depend on outer layers**.

```
┌─────────────────────────────────────────────────┐
│                   EMS_API                       │  ← Outermost (controllers, middleware, Program.cs)
│  ┌─────────────────────────────────────────┐    │
│  │          EMS_Infrastructure             │    │  ← Database, EF Core, repositories
│  │  ┌─────────────────────────────────┐    │    │
│  │  │       EMS_Application           │    │    │  ← Business logic, DTOs, interfaces, services
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

**Why this matters:** Your business logic (Application) defines *interfaces* (e.g., `IDepartmentRepository`). Infrastructure *implements* them. This means you could swap SQL Server for PostgreSQL by only changing Infrastructure — Application and Domain don't care.

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
| Employees | ICollection\<Employee\> | Navigation property (one-to-many) |

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
| IsActive | bool | Soft delete flag |
| CreatedAt | DateTime | Set once on creation |
| UpdatedAt | DateTime? | Set on every update |

#### Enum

**Gender** (`Enum/Gender.cs`): `Male = 1, Female = 2`

---

### 3.2 EMS_Application — Business Logic

**Location:** `EMS_Application/`
**NuGet packages:** `FluentValidation.DependencyInjectionExtensions`, `Microsoft.Extensions.DependencyInjection.Abstractions`

This is the biggest layer. It contains: interfaces, DTOs, mapping, validation, services, exceptions, and the API response wrapper.

#### 3.2.1 Interfaces (`Interfaces/`)

**IGenericRepository\<T\>** — base repository with common CRUD:
- `GetByIdAsync(int id)` → find by primary key
- `GetAllAsync(filter?, includes[])` → list with optional WHERE and Include
- `FindAsync(predicate, includes[])` → single entity with optional Include
- `AddAsync(entity)` → track for insert
- `Update(entity)` → track for update
- `Remove(entity)` → track for delete

**IDepartmentRepository** / **IEmployeeRepository** — extend `IGenericRepository<T>` with no extra methods (yet). They exist so you *can* add entity-specific queries later without modifying the generic one.

**IUnitOfWork** — groups repositories + `SaveChangesAsync()`. This ensures all changes in a single request go into one database transaction.
```
UnitOfWork.Departments  → IDepartmentRepository
UnitOfWork.Employees    → IEmployeeRepository
UnitOfWork.SaveChangesAsync()  → commits everything
```

**IDepartmentService** / **IEmployeeService** — define the business operations (GetAll, GetById, Create, Update, Delete). These are what controllers call. They accept DTOs and return DTOs — never entities.

#### 3.2.2 DTOs (`DTO/`)

DTOs (Data Transfer Objects) are the shapes that go in and out of the API. Entities NEVER leave the service layer.

**Department DTOs:**
- `CreateDepartmentRequest` — Name, Code, Description (what the client sends to POST)
- `UpdateDepartmentRequest` — Name, Code, Description (what the client sends to PUT)
- `DepartmentResponse` — Id, Name, Code, Description, IsActive, CreatedAt, UpdatedAt (what the API returns)

**Employee DTOs:**
- `CreateEmployeeRequest` — all fields except Id, IsActive, CreatedAt, UpdatedAt, Department nav
- `UpdateEmployeeRequest` — same fields as Create
- `EmployeeResponse` — all fields + `DepartmentName` (denormalized from the nav property)

**Why Create and Update are separate classes even though they're identical:** They represent different intents. If Update later becomes a PATCH (partial update with nullable fields), they'll diverge. Separate types = separate validators = easy to change one without affecting the other.

#### 3.2.3 Mapping (`Mapping/`)

Manual mapping via **extension methods** (no AutoMapper). Each mapping file has 3 methods:

| Method | Direction | Used By |
|---|---|---|
| `.ToResponse()` | Entity → Response DTO | Service (returning data) |
| `.ToEntity()` | Create Request → Entity | Service (creating) |
| `.ApplyUpdate()` | Update Request → existing Entity | Service (updating) |

There's also an `IEnumerable<T>.ToResponse()` overload for mapping lists.

**Why `ApplyUpdate` instead of `ToEntity`?** When updating, you need the existing entity from the database (it has Id, CreatedAt, IsActive, etc.). You can't create a new one — you mutate the tracked one so EF Core generates an UPDATE statement.

#### 3.2.4 Validators (`Validators/`)

FluentValidation validators — one per request DTO (4 total). Rules are defined in the constructor using a fluent chain.

**Department rules:**
- Name: not empty, 2-100 chars
- Code: not empty, 2-10 chars, regex `^[A-Z0-9]+$` (uppercase letters and digits only — also null-safe)
- Description: max 500

**Employee rules:**
- FirstName/LastName: not empty, 2-50 chars
- Email: not empty, valid email format
- Phone: max 20 (optional, so no NotEmpty)
- DateOfBirth: must be before today (`LessThan(DateTime.Today)`)
- HireDate: today or before (`LessThanOrEqualTo(DateTime.Today)`)
- Salary: greater than 0
- Gender: must be a defined enum value (`IsInEnum()`)
- JobTitle: not empty, 2-100 chars
- DepartmentId: greater than 0

**How they're registered:** `AddValidatorsFromAssembly()` in `DependencyInjection.cs` scans the assembly and auto-registers every class that extends `AbstractValidator<T>` into DI.

**Where validation happens:** In the **service layer** (not controllers, not middleware). Services will inject `IValidator<CreateDepartmentRequest>`, call `.ValidateAsync()`, and throw `ValidationException` if invalid. *(This wiring is not yet implemented — it's the next step.)*

#### 3.2.5 Custom Exceptions (`Exceptions/`)

Three custom exception types that the middleware catches and translates to HTTP responses:

| Exception | Constructor | Generated Message | HTTP Status |
|---|---|---|---|
| `NotFoundException` | (entityName, key) | "department (5) was not found." | 404 |
| `BadRequestException` | (message) | Whatever you pass | 400 |
| `ValidationException` | (IDictionary\<string, string[]\>) | "One or more validation errors occurred." | 422 |

**Why in Application layer, not API?** Because services throw them. If exceptions lived in API, Application would need to reference API — breaking the dependency rule.

#### 3.2.6 ApiResponse\<T\> (`Common/ApiResponse.cs`)

A generic wrapper so every API response has a consistent shape:

```json
// Success
{
  "success": true,
  "message": "Request completed successfully.",
  "data": { ... },
  "errors": null
}

// Failure
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "data": null,
  "errors": {
    "Email": ["Email is required.", "A valid email address is required."],
    "Salary": ["Salary must be greater than 0."]
  }
}
```

Uses **static factory methods** (a design pattern):
- `ApiResponse<T>.SuccessResponse(data, message?)` — sets Success=true
- `ApiResponse<T>.FailResponse(message, errors?)` — sets Success=false

This prevents mistakes like forgetting to set `Success = true`.

#### 3.2.7 Services (`Services/`)

**DepartmentService** and **EmployeeService** — the business logic layer. They:
1. Receive DTOs from controllers
2. Use UnitOfWork to access repositories
3. Map between DTOs and entities
4. Throw custom exceptions on errors
5. Return response DTOs

**Key patterns in services:**
- **Soft delete:** `Delete` doesn't remove from DB — it sets `IsActive = false`
- **All queries filter by `IsActive`:** `GetAll` uses `.GetAllAsync(d => d.IsActive)`, not `.GetAllAsync()`
- **GetById throws NotFoundException:** Services never return null for "not found" — they throw. The middleware turns it into a 404.
- **Employee queries include Department:** Because `EmployeeResponse` needs `DepartmentName`, the service passes `e => e.Department` as an include expression.
- **CreatedAt/UpdatedAt set in service:** `DateTime.UtcNow` is set before saving.

#### 3.2.8 DependencyInjection (`DependencyInjection.cs`)

Extension method `AddApplication()` that registers:
- `IDepartmentService` → `DepartmentService` (Scoped)
- `IEmployeeService` → `EmployeeService` (Scoped)
- All FluentValidation validators from the assembly

Called in `Program.cs` as `builder.Services.AddApplication()`.

---

### 3.3 EMS_Infrastructure — Database & Data Access

**Location:** `EMS_Infrastructure/`
**NuGet packages:** `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`

#### 3.3.1 AppDbContext (`Data/AppDbContext.cs`)

The EF Core database context. Exposes `DbSet<Employee>` and `DbSet<Department>`. Applies all Fluent API configurations from the assembly automatically via `ApplyConfigurationsFromAssembly()`.

#### 3.3.2 Entity Configurations (`Data/Configurations/`)

Fluent API configuration — one per entity. This is where database schema rules are defined (column sizes, indexes, relationships, defaults).

**DepartmentConfiguration:**
- `Code` has a unique index
- `IsActive` defaults to `true` at database level
- `HasMany(Employees).WithOne(Department)` — one-to-many relationship
- `OnDelete(DeleteBehavior.Restrict)` — can't delete a department that has employees

**EmployeeConfiguration:**
- `Email` has a unique index
- `Salary` has precision (18, 2)
- All required fields marked with `.IsRequired()`
- Max lengths match the entity properties

**Why Fluent API over Data Annotations?** Keeps entities clean (no `[Required]`, `[MaxLength]` attributes cluttering the model). Also, Fluent API can do things annotations can't (composite indexes, owned types, etc.).

#### 3.3.3 Repositories (`Repositories/`)

**GenericRepository\<T\>** — implements `IGenericRepository<T>`:
- Uses `DbSet<T>` for all operations
- `GetAllAsync` builds a query with optional WHERE and Includes, then calls `.ToListAsync()`
- `FindAsync` builds a query with Includes, then calls `.FirstOrDefaultAsync(predicate)`
- `AddAsync` uses `_dbSet.AddAsync()` (async because value generators might need DB access in HiLo scenarios)
- `Update` and `Remove` are sync — they only mark the entity in the Change Tracker

**DepartmentRepository** / **EmployeeRepository** — inherit GenericRepository with the specific entity type. No extra methods yet, but the classes exist so you can add entity-specific queries without touching the generic.

#### 3.3.4 UnitOfWork (`UnitOfWork/UnitOfWork.cs`)

Implements `IUnitOfWork`. Wraps all repositories and the DbContext.

**Lazy initialization:** Repositories aren't created until first access:
```csharp
public IDepartmentRepository Departments =>
    _departments ??= new DepartmentRepository(_context);
```
This uses the `??=` null-coalescing assignment — if `_departments` is null, create it; otherwise reuse.

**Why UnitOfWork?** Without it, each repository would call `SaveChanges` independently. With it, you can make changes across multiple repositories and commit them all in one transaction via `SaveChangesAsync()`.

Implements `IDisposable` to dispose the DbContext when the scope ends.

#### 3.3.5 DependencyInjection (`DependencyInjection.cs`)

Extension method `AddInfrastructure()` that registers:
- `AppDbContext` with SQL Server connection string
- `IUnitOfWork` → `UnitOfWork` (Scoped)

Note: Individual repositories are NOT registered in DI — they're created internally by UnitOfWork.

---

### 3.4 EMS_API — The Presentation Layer

**Location:** `EMS_API/`
**NuGet packages:** `Microsoft.AspNetCore.OpenApi`, `Microsoft.EntityFrameworkCore.Design`, `Swashbuckle.AspNetCore`

#### 3.4.1 Program.cs — The Entry Point

The entire app startup in one file:

```
1. Create builder
2. Register services:
   - AddControllers()
   - AddSwaggerGen()
   - AddApplication()       ← services, validators
   - AddInfrastructure()    ← DbContext, UnitOfWork
3. Build the app
4. Configure middleware pipeline (ORDER MATTERS):
   - Swagger (dev only)
   - ExceptionHandlingMiddleware  ← catches all exceptions
   - HTTPS redirection
   - Authorization
   - MapControllers
5. Run
```

**Middleware order matters.** The exception middleware is placed early so it catches exceptions from everything downstream (controllers, services, etc.).

#### 3.4.2 Exception Handling Middleware (`Middleware/ExceptionHandlingMiddleware.cs`)

A custom middleware class that wraps the entire request pipeline in a try-catch.

**How middleware works in .NET:**
Every request flows through a pipeline of middleware, like layers of an onion. Each middleware can:
1. Do something before passing to the next
2. Call `await _next(context)` to pass to the next middleware
3. Do something after (or catch exceptions from) the next middleware

**What this middleware does:**
1. Calls `await _next(context)` (lets the request continue to controllers/services)
2. If an exception bubbles up, catches it
3. Uses a `switch` expression to map exception type → HTTP status code + response body
4. Writes an `ApiResponse<object>.FailResponse(...)` as JSON

| Exception | Status | Response |
|---|---|---|
| `NotFoundException` | 404 | `{ success: false, message: "department (5) was not found." }` |
| `ValidationException` | 422 | `{ success: false, message: "...", errors: { "Email": [...] } }` |
| `BadRequestException` | 400 | `{ success: false, message: "..." }` |
| Anything else | 500 | `{ success: false, message: "An internal server error occurred." }` |

**The 500 case is critical:** It uses a generic message, never exposing stack traces or internal details. This prevents information leakage in production.

JSON is serialized with `camelCase` naming policy to match JavaScript/frontend conventions.

#### 3.4.3 Controllers (`Controllers/`)

**DepartmentController** and **EmployeeController** — thin REST controllers. They:
1. Receive HTTP requests
2. Call the corresponding service method
3. Wrap the result in `ApiResponse<T>.SuccessResponse()`
4. Return with appropriate HTTP status code

**Endpoints:**

| HTTP Method | Route | Action | Returns |
|---|---|---|---|
| GET | /api/department | GetAll | 200 + list |
| GET | /api/department/{id} | GetById | 200 + single |
| POST | /api/department | Create | 201 + created entity |
| PUT | /api/department/{id} | Update | 200 + updated entity |
| DELETE | /api/department/{id} | Delete | 200 + success message |

Same pattern for `/api/employee`.

**Why no try-catch in controllers?** The exception middleware handles all error cases. Controllers only handle the happy path. This keeps them thin and focused.

**`CreatedAtAction`:** For POST endpoints, returns 201 status with a `Location` header pointing to the GET endpoint for the newly created resource. This is proper REST.

---

## 4. How a Request Flows Through the System

Example: `POST /api/department` with body `{ "name": "HR", "code": "HR01" }`

```
1. HTTP Request hits Program.cs pipeline

2. ExceptionHandlingMiddleware.InvokeAsync()
   └─ calls await _next(context) — passes to next middleware

3. Routing middleware matches → DepartmentController.Create()

4. Controller calls _departmentService.CreateDepartmentAsync(request)

5. DepartmentService:
   a. (Future: validate request with FluentValidation)
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

**If something goes wrong** (e.g., department ID 999 not found on GET):
```
1-3. Same as above
4. Controller calls _departmentService.GetDepartmentByIdAsync(999)
5. Service: FindAsync returns null → throws NotFoundException("department", 999)
6. Exception bubbles up through the call stack
7. ExceptionHandlingMiddleware catches it
8. Maps to 404 + ApiResponse<object>.FailResponse("department (999) was not found.")
9. Writes JSON response → client
```

---

## 5. Key Design Patterns Used

| Pattern | Where | Why |
|---|---|---|
| **Clean Architecture** | Project structure | Dependency rule keeps business logic independent of frameworks |
| **Repository Pattern** | GenericRepository, specific repos | Abstracts data access, makes it testable |
| **Unit of Work** | UnitOfWork class | Groups repo operations into a single transaction |
| **DTO Pattern** | Request/Response DTOs | Decouples API shape from database shape |
| **Static Factory Method** | ApiResponse\<T\> | Consistent object creation, prevents invalid state |
| **Extension Methods** | Mapping classes | Clean syntax for entity ↔ DTO conversion |
| **Middleware Pattern** | ExceptionHandlingMiddleware | Centralized cross-cutting concern (error handling) |
| **Dependency Injection** | DependencyInjection.cs files | Loose coupling, testability, interface-based programming |
| **Soft Delete** | IsActive flag | Data preservation, audit-friendly |

---

## 6. Project File Structure (Complete)

```
EMS/
├── EMS.slnx
│
├── EMS_Domain/                          ← INNER CORE (no dependencies)
│   ├── EMS_Domain.csproj
│   ├── Entities/
│   │   ├── Department.cs
│   │   └── Employee.cs
│   └── Enum/
│       └── Gender.cs
│
├── EMS_Application/                     ← BUSINESS LOGIC (depends on Domain)
│   ├── EMS_Application.csproj
│   ├── DependencyInjection.cs
│   ├── Common/
│   │   └── ApiResponse.cs
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
│   │   ├── NotFoundException.cs
│   │   ├── BadRequestException.cs
│   │   └── ValidationException.cs
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   ├── Departments/
│   │   │   ├── IDepartmentRepository.cs
│   │   │   └── IDepartmentService.cs
│   │   └── Employees/
│   │       ├── IEmployeeRepository.cs
│   │       └── IEmployeeService.cs
│   ├── Mapping/
│   │   ├── DepartmentMapping.cs
│   │   └── EmployeeMapping.cs
│   ├── Services/
│   │   ├── DepartmentService.cs
│   │   └── EmployeeService.cs
│   └── Validators/
│       ├── CreateDepartmentValidator.cs
│       ├── UpdateDepartmentValidator.cs
│       ├── CreateEmployeeValidator.cs
│       └── UpdateEmployeeValidator.cs
│
├── EMS_Infrastructure/                  ← DATA ACCESS (depends on Application)
│   ├── EMS_Infrastructure.csproj
│   ├── DependencyInjection.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   │       ├── DepartmentConfiguration.cs
│   │       └── EmployeeConfiguration.cs
│   ├── Repositories/
│   │   ├── GenericRepository.cs
│   │   ├── DepartmentRepository.cs
│   │   └── EmployeeRepository.cs
│   ├── UnitOfWork/
│   │   └── UnitOfWork.cs
│   └── Migrations/
│
└── EMS_API/                             ← PRESENTATION (depends on all)
    ├── EMS_API.csproj
    ├── Program.cs
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    └── Controllers/
        ├── DepartmentController.cs
        └── EmployeeController.cs
```

---

## 7. NuGet Packages

| Project | Package | Purpose |
|---|---|---|
| EMS_Domain | (none) | Pure C# — no external dependencies |
| EMS_Application | FluentValidation.DependencyInjectionExtensions | Validation rules + DI registration |
| EMS_Application | Microsoft.Extensions.DependencyInjection.Abstractions | `IServiceCollection` for the DI extension method |
| EMS_Infrastructure | Microsoft.EntityFrameworkCore.SqlServer | SQL Server database provider |
| EMS_Infrastructure | Microsoft.EntityFrameworkCore.Tools | Migrations CLI (`dotnet ef`) |
| EMS_API | Microsoft.AspNetCore.OpenApi | OpenAPI metadata |
| EMS_API | Microsoft.EntityFrameworkCore.Design | Design-time support for migrations |
| EMS_API | Swashbuckle.AspNetCore | Swagger UI |

---

## 8. What's Still TODO (Milestone 2 Remaining)

One thing left: **inject validators into services**. Right now validators are registered in DI but no service calls them yet. The next step is:
1. Inject `IValidator<CreateDepartmentRequest>` etc. into DepartmentService/EmployeeService
2. Call `validator.ValidateAsync(request)` at the start of Create/Update methods
3. If validation fails, throw `ValidationException` with the errors grouped by property name

After that, Milestone 2 is complete.
