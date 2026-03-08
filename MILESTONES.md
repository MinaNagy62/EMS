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

## Milestone 5: CQRS with MediatR — IN PROGRESS
**Challenge:** Separate reads from writes using CQRS + MediatR.

### Sprint 1 — MediatR Setup + Department Queries: COMPLETED ✓ (Score: 9.5/10)

**What CQRS is:**
CQRS (Command Query Responsibility Segregation) splits reads and writes into separate models. Instead of one service class handling everything (GetAll, GetById, Create, Update, Delete), each operation becomes its own pair: a Request class (Command or Query) and a Handler class. Commands change state (writes), Queries read state (reads). This follows Single Responsibility Principle — each handler does exactly one thing with only the dependencies it needs.

**What MediatR is:**
MediatR is an in-process message dispatcher. The controller doesn't call services directly — it sends a request object via `_mediator.Send(query)`. MediatR scans the DI container for the handler that matches the request type (via `IRequestHandler<TRequest, TResponse>`) and calls it. Registration is automatic via assembly scanning, similar to how FluentValidation auto-discovers validators.

**What Pipeline Behaviors are:**
Pipeline Behaviors are MediatR's equivalent of HTTP middleware. They implement `IPipelineBehavior<TRequest, TResponse>` and wrap every request. `next()` calls the next behavior or the handler — like `await _next(context)` in middleware. Used for cross-cutting concerns (validation, logging) so handlers stay focused on business logic only.

**Delivered:**
- [x] MediatR 14.1.0 installed in EMS_Application
- [x] `ValidationBehavior<TRequest, TResponse>` — automatic validation pipeline (replaces manual validation in handlers)
  - Injects `IEnumerable<IValidator<TRequest>>` — skips if no validator exists for the request type
  - Uses `Task.WhenAll` for parallel validator execution
  - Throws custom `ValidationException` with error dictionary (same format as before)
- [x] `GetAllDepartmentsQuery : PagedRequest, IRequest<PagedResponse<DepartmentResponse>>` — inherits PagedRequest for reuse
- [x] `GetAllDepartmentsHandler` — full caching logic, only injects IUnitOfWork + IMemoryCache
- [x] `GetDepartmentByIdQuery : IRequest<DepartmentResponse>` — just `int Id`
- [x] `GetDepartmentByIdHandler` — only injects IUnitOfWork
- [x] DependencyInjection.cs: AddMediatR + ValidationBehavior registered as IPipelineBehavior
- [x] DepartmentController: dual injection (IMediator for reads, IDepartmentService for writes — transitional)
- [x] Folder structure: `Features/Departments/Queries/GetAllDepartments/` and `GetDepartmentById/`

**How MediatR discovers handlers (the matching mechanism):**
At startup, `RegisterServicesFromAssembly` scans for all `IRequestHandler<T,R>` implementations and registers them in DI. At runtime, when you call `_mediator.Send(query)`, MediatR looks at the type of the object (e.g., `GetAllDepartmentsQuery`), resolves `IRequestHandler<GetAllDepartmentsQuery, PagedResponse<DepartmentResponse>>` from DI, and calls its `Handle` method. The generic type parameter IS the key.

### Remaining Sprints:
- [ ] Sprint 2: Department Commands (Create, Update, Delete → handlers, remove DepartmentService)
- [ ] Sprint 3: Employee Queries + Commands (full refactor)
- [ ] Sprint 4: Auth Commands + Notifications (domain events for cache invalidation)
- [ ] Sprint 5: LoggingBehavior

**Interview topics this covers:**
- "What is CQRS and when would you use it?"
- "How does MediatR work? How does it find the right handler?"
- "What are Pipeline Behaviors? How are they different from HTTP middleware?"
- "What's the difference between Send and Publish in MediatR?"
- "How do you handle domain events?"
- "Can you implement CQRS without MediatR?"
- "What are the downsides of CQRS?"

**Deliverable:** All endpoints refactored to CQRS with MediatR, pipeline behaviors for validation and logging.

---

## Milestone 6: Background Jobs, Logging & Finishing Touches — NOT STARTED
**Challenge:** Add production-ready cross-cutting concerns.

**You will cover:**
1. Serilog structured logging (console + file sinks)
2. Correlation ID middleware
3. Background job: Daily attendance report (Hosted Service)
4. API Versioning (URL or header based)
5. Rate Limiting middleware
6. Health Checks
7. Swagger/OpenAPI documentation improvements
8. Unit Tests (xUnit + Moq — test at least service layer)

**Interview topics this covers:**
- "How do you handle logging in production?"
- "What background processing have you used?"
- "How do you version your APIs?"
- "What testing strategies do you use?"

**Deliverable:** Production-ready API with logging, background jobs, tests, and documentation.

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
