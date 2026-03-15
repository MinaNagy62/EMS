# Employee Management System (EMS) - Milestones

## Project Overview
A REST API for managing employees, departments, attendance, and leave requests.
Small in scope, but deep in patterns and best practices.

**Tech Stack:** .NET 10 Web API, SQL Server, EF Core, JWT Auth

---

## Milestone 1: Project Setup & Clean Architecture — COMPLETED ✓
**Score: 7.5/10**

**What was covered:**
1. Clean Architecture layers (Domain, Application, Infrastructure, API)
2. Proper project references (dependency rule)
3. Domain entities: `Employee`, `Department` (with relationships)
4. EF Core setup with Fluent API configuration (no Data Annotations)
5. SQL Server connection & initial migration
6. CRUD endpoints for Department AND Employee
7. Repository Pattern + Unit of Work Pattern (with Generic Repository)
8. Dependency Injection registration via extension methods

**Key review feedback:**
- Architecture was solid, dependency rule followed correctly
- Issues were mostly attention to detail (unused usings, parameter casing, missing defaults)
- All issues were fixed before moving to M2

---

## Milestone 2: DTOs, Validation & Global Error Handling — COMPLETED ✓
**Score: 8.5/10**

### Everything delivered:
- [x] Request/Response DTOs (6 total)
- [x] Manual mapping with extension methods (ApplyUpdate pattern)
- [x] Service interfaces updated to use DTOs
- [x] Services refactored to use mapping
- [x] Controllers updated to use DTOs, thin (no try-catch)
- [x] Employee queries include Department navigation (for DepartmentName)
- [x] Domain restructured (Entities/ folder, Enum/ folder)
- [x] Custom Exceptions (NotFoundException, BadRequestException, ValidationException)
- [x] ApiResponse<T> wrapper with factory methods
- [x] FluentValidation validators (4 files) with correct rules (Matches regex for null-safe uppercase)
- [x] Validators registered via AddValidatorsFromAssembly
- [x] Validators injected and used in both services (service-layer validation)
- [x] ValidationExtensions.ToErrorDictionary() helper (extracted from duplicated code)
- [x] Global Exception Handling Middleware (404/422/400/500)
- [x] Controllers cleaned — no try-catch, ApiResponse<T> wrapping
- [x] Middleware registered in Program.cs

**Review scores:**
- M2 Review #1 (DTOs & Mapping): Good
- M2 Review #2 (Exceptions, ApiResponse, Validators): 8/10
- M2 Review #3 (Final — Middleware, cleanup): 8.5/10

**Interview topics covered:**
- "Why use DTOs?"
- "How do you handle validation in .NET?"
- "How do you handle errors globally?"
- "What middleware have you written?"
- "Why manual mapping over AutoMapper?"
- "Where should validation happen?"

---

## Milestone 3: Authentication & Authorization — COMPLETED ✓
**Score: 8.5/10**

### Everything delivered:

**Domain layer:**
- [x] AppUser entity (Id, FirstName, LastName, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiryDate, IsActive, CreatedAt)
- [x] Role enum (Admin=1, HR=2, Employee=3)

**Infrastructure layer:**
- [x] AppUserConfiguration (Fluent API: HasKey, max lengths, unique email index, PasswordHash required)
- [x] AppDbContext updated with DbSet<AppUser>
- [x] Migration created (20260304092250_A_E_AppUsers)
- [x] AppUserRepository extending GenericRepository<AppUser>
- [x] IUnitOfWork + UnitOfWork updated with AppUsers (lazy init)
- [x] JwtTokenService — HS256, claims (Sub, Email, Role, Jti), IOptions<JwtSettings>, RandomNumberGenerator, tuple return `(string Token, DateTime ExpiresAt)`
- [x] JWT settings in appsettings.json, registered via services.Configure<JwtSettings>()
- [x] IJwtTokenService registered in Infrastructure DI

**Application layer:**
- [x] Auth DTOs: RegisterRequest, LoginRequest, AuthResponse, RefreshTokenRequest
- [x] IAuthService interface (RegisterAsync, LoginAsync, RefreshTokenAsync)
- [x] IJwtTokenService interface — tuple return for token + expiry
- [x] AuthService implementation with all 3 flows (Register, Login, Refresh)
- [x] JwtSettings class (Options pattern)
- [x] ValidationExtensions.ToErrorDictionary() helper
- [x] Auth validators: RegisterRequestValidator, LoginRequestValidator
- [x] IAuthService registered in Application DI

**API layer:**
- [x] AuthController — POST /api/auth/register, /login, /refresh (anonymous — no [Authorize])
- [x] JWT authentication config in Program.cs (AddAuthentication + AddJwtBearer with full token validation)
- [x] `app.UseAuthentication()` before `app.UseAuthorization()` in pipeline
- [x] `ClockSkew = TimeSpan.Zero` — no tolerance for expired tokens
- [x] `[Authorize]` on DepartmentController and EmployeeController (class level)
- [x] Role-based authorization:
  - Department: GET = any authenticated user, POST/PUT/DELETE = Admin only
  - Employee: GET = any authenticated user, POST/PUT = Admin or HR, DELETE = Admin only
- [x] Microsoft.AspNetCore.Authentication.JwtBearer package installed in API

**Packages installed in M3:**
- BCrypt.Net-Next (4.1.0) in Application
- Microsoft.Extensions.Options (10.0.3) in Application
- Microsoft.AspNetCore.Authentication.JwtBearer (10.0.3) in API

### Review scores:
- M3 Review #1 (Foundation): Good
- M3 Review #2 (DTOs, Interface, Config): Good
- M3 Review #3 (Core Implementation): 7.5/10 → fixed to 8.5/10
- M3 Review #5 (Final — Controller, JWT config, Authorization): 8.5/10 — COMPLETED

### Security decisions:
- Login: same error for wrong email AND wrong password (prevents user enumeration)
- Refresh token rotation on every refresh (old token invalidated)
- BCrypt for password hashing (intentionally slow, salted)
- ClockSkew = TimeSpan.Zero (no 5-minute tolerance on expired tokens)
- 500 errors never leak internal details
- No hardcoded expiry values — all from JwtSettings via Options pattern

### Authorization matrix:
| Endpoint | Admin | HR | Employee |
|---|---|---|---|
| GET departments | ✓ | ✓ | ✓ |
| POST/PUT/DELETE departments | ✓ | ✗ | ✗ |
| GET employees | ✓ | ✓ | ✓ |
| POST/PUT employees | ✓ | ✓ | ✗ |
| DELETE employees | ✓ | ✗ | ✗ |
| Auth endpoints (register/login/refresh) | Public | Public | Public |

**Interview topics covered:**
- "Explain JWT authentication flow"
- "Difference between Authentication and Authorization"
- "How do refresh tokens work?"
- "Role-based vs Policy-based authorization"
- "Why BCrypt over SHA256?"
- "What is user enumeration and how do you prevent it?"
- "What is the Options pattern?"
- "What is token rotation?"
- "Why ClockSkew = TimeSpan.Zero?"
- "How does [Authorize(Roles)] work under the hood?"

**Deliverable:** Secured API — only authenticated users access resources, roles control actions.

---

## Milestone 4: Advanced Querying & Performance — COMPLETED ✓
**Score: 9/10**

### Everything delivered:

**Sprint 1 — Pagination:**
- [x] PagedRequest with PageNumber (default 1, min 1) and PageSize (default 10, max 50, min 1)
- [x] PagedResponse<T> with Items, PageNumber, PageSize, TotalCount, computed TotalPages/HasPreviousPage/HasNextPage
- [x] BaseEntity abstract class with Id — generic constraint changed from `class` to `BaseEntity`
- [x] GetPagedAsync in GenericRepository — CountAsync + OrderBy(Id) + Skip/Take
- [x] Department and Employee endpoints both paginated with [FromQuery]

**Sprint 2 — Filtering & Searching:**
- [x] DepartmentQueryRequest extends PagedRequest (Search?, IsActive?)
- [x] EmployeeQueryRequest extends PagedRequest (Search?, DepartmentId?, Gender?)
- [x] GetPagedAsync updated to accept `List<Expression<Func<T, bool>>>` (multiple filters = AND)
- [x] Dynamic filter building in service layer — keeps repository generic
- [x] Case-insensitive search with .ToLower().Contains() (EF Core translates to SQL LOWER())
- [x] Department IsActive overridable (defaults to active-only)

**Sprint 3 — Sorting:**
- [x] SortBy (string?) and SortDescending (bool) added to PagedRequest
- [x] ApplySorting with dynamic expression tree (Expression.Parameter → Property → Convert → Lambda)
- [x] Reflection with BindingFlags.IgnoreCase for case-insensitive property matching
- [x] Invalid property fallback to Id (no crash)
- [x] Both OrderBy and OrderByDescending supported

**Sprint 4 — Caching:**
- [x] IMemoryCache injected in DepartmentService (cache DTOs, not entities)
- [x] Cache key built from all query params (page, size, search, isActive, sortBy, sortDescending)
- [x] Sliding expiration (10 min) + absolute expiration (1 hour)
- [x] CancellationTokenSource + CancellationChangeToken for bulk invalidation
- [x] InvalidateCache() called on Create, Update, Delete
- [x] AddMemoryCache() in Program.cs
- [x] Only departments cached (low cardinality, rarely changes)

### Review scores:
- Sprint 1 (Pagination): 8/10 → fixed to 9/10
- Sprint 2 (Filtering & Searching): 8.5/10
- Sprint 3 (Sorting): 9/10
- Sprint 4 (Caching): 9.5/10

### Key architectural decisions:
- BaseEntity constraint enables deterministic ordering in generic repository
- Filters built in service layer, not repository — preserves dependency rule and keeps repo reusable
- Expression trees for dynamic sorting — no third-party library, pure .NET
- Cache at service layer (DTOs), not repository (entities) — avoids EF Core Change Tracker corruption
- Static CancellationTokenSource because IMemoryCache is singleton and must outlive scoped service instances

**Interview topics covered:**
- "How do you implement pagination in .NET?"
- "What is the difference between IQueryable and IEnumerable?"
- "How do you build dynamic LINQ expressions?"
- "What is an expression tree and how does EF Core use them?"
- "What caching strategies have you used?"
- "How do you handle cache invalidation?"
- "Why cache DTOs and not entities?"
- "What is the cache-aside pattern?"

**Deliverable:** Efficient, queryable endpoints with pagination, filtering, sorting, and caching.

---

## Milestone 5: CQRS with MediatR — COMPLETED ✓
**Score: 9.5/10**

### What was achieved:
Complete refactoring from service-based architecture to CQRS with MediatR. **Zero service classes remain.** All 13 operations across 3 features are now individual Command/Query + Handler pairs. Two pipeline behaviors (Logging + Validation) handle cross-cutting concerns automatically.

### The transformation:
| Before M5 | After M5 |
|---|---|
| 3 service classes (Department, Employee, Auth) | 0 service classes |
| Controllers inject services | Controllers inject only IMediator |
| 3 manual DI registrations | 0 — MediatR auto-discovers all handlers |
| Manual validation in every method | ValidationBehavior does it automatically |
| No request logging | LoggingBehavior logs every request with timing |

### Sprint 1 — MediatR Setup + Department Queries (Score: 9.5/10)
- [x] MediatR 14.1.0 installed in EMS_Application
- [x] `ValidationBehavior<TRequest, TResponse>` — automatic validation pipeline with Task.WhenAll
- [x] `GetAllDepartmentsQuery : PagedRequest, IRequest<...>` — inherits PagedRequest for reuse
- [x] `GetAllDepartmentsHandler` — full caching logic, only injects IUnitOfWork + IMemoryCache
- [x] `GetDepartmentByIdQuery` / `GetDepartmentByIdHandler`
- [x] DepartmentController: dual injection (transitional)

### Sprint 2 — Department Commands (Score: 9/10)
- [x] `CreateDepartmentCommand/Handler` — properties on command (not embedded DTO)
- [x] `UpdateDepartmentCommand/Handler` — NotFoundException check, cache invalidation
- [x] `DeleteDepartmentCommand : IRequest` (void) — soft delete
- [x] Validators updated to target Commands
- [x] DepartmentController uses only IMediator
- [x] DepartmentService + IDepartmentService deleted
- [x] Dead mapping methods (ToEntity, ApplyUpdate) removed

### Sprint 3 — Employee Queries + Commands (Score: 9.5/10)
- [x] `GetAllEmployeesQuery : PagedRequest` — dynamic filters (Search, DepartmentId, Gender)
- [x] `GetAllEmployeesHandler` — includes Department navigation
- [x] `GetEmployeeByIdHandler` — includes Department for DepartmentName
- [x] `CreateEmployeeCommand/Handler` — all 10 properties
- [x] `UpdateEmployeeCommand/Handler` — NotFoundException + full update
- [x] `DeleteEmployeeCommand : IRequest` (void) — soft delete, no cache
- [x] Validators target Commands (10 rules on Create)
- [x] EmployeeController uses only IMediator (Admin,HR for writes, Admin for delete)
- [x] EmployeeService + IEmployeeService deleted

### Sprint 4 — Auth Refactor (Score: 9.5/10)
- [x] `RegisterCommand/Handler` — BCrypt hashing, email uniqueness, JWT generation
- [x] `LoginCommand/Handler` — user enumeration prevention, token rotation
- [x] `RefreshTokenCommand/Handler` — BONUS (not assigned, developer added it)
- [x] Handlers inject IUnitOfWork + IJwtTokenService + IOptions<JwtSettings>
- [x] Validators renamed to target Commands (RegisterCommandValidator, LoginCommandValidator)
- [x] AuthController uses only IMediator
- [x] AuthService + IAuthService deleted
- [x] DependencyInjection.cs: ZERO service registrations

### Sprint 5 — LoggingBehavior (Score: 9.5/10)
- [x] `LoggingBehavior<TRequest, TResponse>` — IPipelineBehavior with ILogger
- [x] Structured logging: `{RequestName}`, `{ElapsedMs}` (log aggregation friendly)
- [x] Stopwatch for execution timing
- [x] LogInformation on entry/exit, LogError on exception
- [x] Re-throws with `throw;` (preserves stack trace)
- [x] Does NOT log request payload (security — passwords, tokens)
- [x] Registered BEFORE ValidationBehavior (wraps everything, catches validation failures)

### Complete handler inventory (13 total):
| Feature | Operation | Type | Returns |
|---|---|---|---|
| Department | GetAllDepartments | Query | PagedResponse\<DepartmentResponse\> |
| Department | GetDepartmentById | Query | DepartmentResponse |
| Department | CreateDepartment | Command | DepartmentResponse |
| Department | UpdateDepartment | Command | DepartmentResponse |
| Department | DeleteDepartment | Command | void |
| Employee | GetAllEmployees | Query | PagedResponse\<EmployeeResponse\> |
| Employee | GetEmployeeById | Query | EmployeeResponse |
| Employee | CreateEmployee | Command | EmployeeResponse |
| Employee | UpdateEmployee | Command | EmployeeResponse |
| Employee | DeleteEmployee | Command | void |
| Auth | Register | Command | AuthResponse |
| Auth | Login | Command | AuthResponse |
| Auth | RefreshToken | Command | AuthResponse |

### Pipeline flow:
```
Request → LoggingBehavior (logs entry + timing)
            → ValidationBehavior (validates, throws if invalid)
                → Handler (business logic)
            ← ValidationBehavior
         ← LoggingBehavior (logs exit + elapsed ms)
Response
```

### Review scores:
- Sprint 1 (MediatR Setup + Dept Queries): 9.5/10
- Sprint 2 (Department Commands): 9/10
- Sprint 3 (Employee Queries + Commands): 9.5/10
- Sprint 4 (Auth Refactor): 9.5/10
- Sprint 5 (LoggingBehavior): 9.5/10

**Interview topics covered:**
- "What is CQRS and when would you use it?"
- "How does MediatR work? How does it find the right handler?"
- "What are Pipeline Behaviors? How are they different from HTTP middleware?"
- "What's the difference between Send and Publish in MediatR?"
- "Can you implement CQRS without MediatR?"
- "What are the downsides of CQRS?"
- "Why is Login a Command and not a Query?"
- "How do you handle cross-cutting concerns in CQRS?"
- "Where does validation happen in a CQRS architecture?"
- "Why not log the request payload in LoggingBehavior?"

**Deliverable:** All endpoints refactored to CQRS with MediatR, pipeline behaviors for validation and logging.

---

## Project Complete

**5 milestones completed. Score trajectory: 7.5 → 8.5 → 8.5 → 9.0 → 9.5**

The EMS project covers: Clean Architecture, Repository + UoW, DTOs + manual mapping, FluentValidation, global exception middleware, JWT auth + refresh tokens, role-based authorization, pagination + filtering + sorting, in-memory caching with bulk invalidation, CQRS with MediatR (13 handlers), and pipeline behaviors (validation + logging).

---

## How We Work Together

1. **Before each milestone:** I explain what you'll build and the concepts behind it
2. **You code:** Implement the milestone yourself
3. **Review:** Show me your code — I review it as a senior/lead developer would
4. **Feedback:** I tell you:
   - What you did well (strengths)
   - What needs improvement (with specific suggestions)
   - Interview questions you should be able to answer
   - Improvements to carry into the next milestone
5. **Next milestone:** Incorporates lessons from the previous review
