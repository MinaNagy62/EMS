# Instructor Notes - Minan's .NET Backend Journey

## Developer Profile
- **Experience:** 3 years .NET backend
- **Goal:** Master backend .NET patterns & ace interviews
- **Project:** Employee Management System (EMS)

## Skills Tracking

### Areas to Evaluate (per milestone)
| Skill Area | Status | Notes |
|---|---|---|
| Project Structure & Clean Architecture | Reviewed M1 | Good structure, improved in M2 (Entities folder, Enum folder) |
| Entity Design & EF Core (Fluent API) | Reviewed M1+M3 | Solid. AppUser entity well-designed with refresh token support |
| Repository & Unit of Work Pattern | Reviewed M1+M3 | Solid. Extended with AppUserRepository and lazy init in UoW |
| DTOs & Manual Mapping | Done M2 | DTOs + mapping done, ApplyUpdate pattern is good |
| Validation (FluentValidation) | Done M2+M3 | 6 validators total. Fixed null-safety with Matches regex. Added auth validators |
| Custom Exceptions | Done M2 | NotFoundException, BadRequestException, ValidationException — clean implementations |
| ApiResponse<T> Wrapper | Done M2 | Factory methods, fixed null-safety on Message property after review |
| Error Handling & Middleware | Done M2 | Global Exception Handling Middleware with switch expression, camelCase JSON |
| ValidationExtensions Helper | Done M3 | ToErrorDictionary() extension extracted from duplicated code across 6 service methods |
| Options Pattern | Done M3 | JwtSettings class with IOptions<JwtSettings> — used in JwtTokenService and AuthService |
| Authentication (JWT) | Done M3 | Full JWT auth: JwtTokenService, AuthService (3 flows), AuthController, JWT middleware in Program.cs |
| Authorization (Role & Policy based) | Done M3 | [Authorize] on controllers, role-based: Admin/HR/Employee with proper access matrix |
| Pagination | Done M4 | PagedRequest/PagedResponse<T>, BaseEntity constraint, Skip/Take, deterministic OrderBy |
| Filtering & Searching | Done M4 | Query DTOs extending PagedRequest, dynamic filter list, case-insensitive search |
| Sorting | Done M4 | Dynamic expression tree (Expression.Property + Expression.Convert), reflection with BindingFlags.IgnoreCase |
| Caching (In-Memory) | Done M4 | IMemoryCache on DepartmentService, CancellationTokenSource bulk invalidation, sliding+absolute expiry |
| Background Jobs (Hangfire/Hosted Services) | Not Started | - |
| Logging (Serilog structured logging) | Not Started | - |
| Unit Testing & Integration Testing | Not Started | - |
| API Versioning | Not Started | - |
| Rate Limiting | Not Started | - |
| CQRS with MediatR | Done M5 | Full CQRS refactor: 13 handlers (Dept queries/commands, Emp queries/commands, Auth commands), ValidationBehavior, LoggingBehavior. Zero service classes remain. |
| Specification Pattern | Not Started | - |
| Dependency Injection (advanced) | Not Started | - |

## Review History

### Milestone 1 Review — 2026-03-02
**Overall: 7.5/10 — Solid foundation with room to improve**

#### Strengths
1. Clean Architecture dependency rule followed correctly
2. Fluent API configurations are clean and complete
3. Generic repository with filter + includes — beyond basics
4. Lazy initialization in UnitOfWork (??=)
5. Soft delete implemented correctly
6. DI extension methods keep Program.cs clean
7. OnDelete Restrict on Department-Employee relationship
8. CreatedAtAction in POST — proper REST

#### Weaknesses Found
1. Unused `using System.ComponentModel.DataAnnotations` in both entities
2. Parameter naming: `Employee` (capital E) in EmployeeService
3. Folder naming: `IDepartment`/`IEmployee` looked like interface names
4. Employees collection not initialized — NullReferenceException risk
5. Unused `using System.Linq.Expressions` in IDepartmentService
6. Inconsistent IsActive filtering between GetById vs Update/Delete
7. Employee.IsActive missing `= true` default
8. Gender enum in same file as Employee
9. Controllers had repetitive try-catch

#### Issues Fixed by Developer
- Removed unused using from entities ✓
- Fixed parameter casing ✓
- Renamed folders to Departments/Employees ✓
- Initialized Employees collection ✓
- Added `= true` to Employee.IsActive ✓
- Made Delete consistent with FindAsync ✓
- Moved Gender to own file (Enum/Gender.cs) ✓
- Moved entities to Entities/ folder ✓

#### Issues NOT Fixed (carried to M2)
- Unused `using System.Linq.Expressions` in IDepartmentService — FIXED in M2 (now uses DTO imports)
- Controllers still have try-catch — FIXED in M2 (middleware added)

### Milestone 2 Progress Reviews

#### M2 Review #1 — 2026-03-02 (DTOs & Mapping — ~50% done)
**Status: First half completed**

What was completed:
1. All 6 DTOs created correctly (Create/Update Request + Response for both entities)
2. Manual mapping with extension methods — clean approach
3. `ApplyUpdate()` pattern instead of manual property-by-property in service — smart improvement
4. Service interfaces updated to accept DTOs and return Response DTOs
5. Services refactored to use mapping methods
6. Employee service includes Department navigation when querying (for DepartmentName)
7. Controllers updated to use DTOs
8. Domain restructured: Entities/ folder, Enum/ folder
9. Switched to SwaggerGen (better than just OpenApi)
10. All M1 carry-forward issues resolved

Early observations:
- Developer improved code organization significantly between M1 and M2
- ApplyUpdate mapping pattern shows good thinking — separating creation mapping from update mapping
- Employee queries correctly include Department for DepartmentName resolution
- Null-safe DepartmentName mapping: `employee.Department?.Name ?? string.Empty`

#### M2 Review #2 — 2026-03-03 (Exceptions, ApiResponse, Validators — ~80% done)

What was completed:
1. Custom exceptions created correctly in EMS_Application/Exceptions/
   - NotFoundException takes (entityName, key) — produces clean message
   - BadRequestException takes message string
   - ValidationException wraps IDictionary<string, string[]> — proper field-level errors
2. ApiResponse<T> wrapper with static factory methods (SuccessResponse, FailResponse)
   - Initially had nullable Message without default — fixed to `= string.Empty` after review feedback
3. 4 FluentValidation validators with correct rules
   - Department: Name, Code (with uppercase check), Description
   - Employee: all 10 properties validated correctly
   - Good use of IsInEnum(), LessThan vs LessThanOrEqualTo distinction for DOB vs HireDate
4. FluentValidation.DependencyInjectionExtensions package installed
5. Validators registered via AddValidatorsFromAssembly in DependencyInjection.cs

Score: 8/10

Strengths in this batch:
- Clean exception design — NotFoundException with entityName+key is interview-ready
- ValidationException using IDictionary<string, string[]> matches ASP.NET conventions
- Validator rules are comprehensive and correct
- Responded to ApiResponse null-safety feedback immediately

Issues found:
- `.Must(code => code == code.ToUpper())` — NullReferenceException if Code is null (FIXED with Matches regex)
- Create and Update validators are 100% duplicated (acceptable since DTOs are different types)

#### M2 Review #3 (Final) — 2026-03-04 — COMPLETED
**Score: 8.5/10**

All remaining items implemented:
1. Uppercase validator bug — fixed with `Matches(@"^[A-Z0-9]+$")` (smarter than null guard)
2. Global Exception Handling Middleware — switch expression, proper status codes, camelCase JSON
3. NotFoundException used correctly in both services (nameof bug caught and fixed)
4. Controllers cleaned — no try-catch, ApiResponse<T> throughout
5. Middleware registered in Program.cs
6. Validators injected in BOTH services (was missing from DepartmentService — caught in review, fixed)

### Milestone 3 Progress Reviews

#### M3 Review #1 — 2026-03-04 (Foundation — Entity, Enum, Config, Migration, DTOs)
**Status: Foundation laid (~15%)**

What was completed:
1. AppUser entity with all required fields (PasswordHash, RefreshToken, RefreshTokenExpiryDate, Role, IsActive, CreatedAt)
2. Role enum (Admin=1, HR=2, Employee=3)
3. AppUserConfiguration — Fluent API with email unique index
4. AppDbContext updated with DbSet<AppUser>
5. Migration created (20260304092250_A_E_AppUsers)
6. IAppUserRepository interface extending IGenericRepository

Observations:
- Named `RefreshTokenExpiryDate` instead of spec's `RefreshTokenExpiryTime` — acceptable, no functional impact
- Foundation follows established patterns from M1/M2

#### M3 Review #2 — 2026-03-04 (Auth DTOs, IAuthService, BCrypt, JWT Settings)
**Status: ~30%**

What was completed:
1. Auth DTOs: RegisterRequest, LoginRequest, AuthResponse — all correct
2. IAuthService interface with 3 proper methods (RegisterAsync, LoginAsync, RefreshTokenAsync)
3. Correctly removed the initial IAppUserService (was returning raw entities — DTO rule violation)
4. BCrypt.Net-Next package installed
5. JWT settings added to appsettings.json (all 5 values)

Minor naming nits (not blocking):
- DTOs initially in `DTO/AppUser/` folder instead of `DTO/Auth/` — later corrected
- `ExpireAt` instead of `ExpiresAt` — later corrected

#### M3 Review #3 — 2026-03-04 (Core Implementation Sprint)
**Status: ~70% — First pass score: 7.5/10**

What was completed:
1. AppUserRepository — follows established pattern
2. UnitOfWork updated with lazy AppUsers initialization
3. IUnitOfWork updated with IAppUserRepository AppUsers
4. IJwtTokenService interface — `(string Token, DateTime ExpiresAt)` tuple return
5. JwtTokenService implementation — HS256, claims (Sub, Email, Role, Jti), IOptions<JwtSettings>, RandomNumberGenerator
6. AuthService implementation — all 3 flows (Register, Login, Refresh)
7. Auth validators — RegisterRequestValidator, LoginRequestValidator
8. DI registrations — AuthService in Application, JwtTokenService in Infrastructure

Issues found (3 total):
1. **Hardcoded magic numbers** — `AddDays(7)` and `AddMinutes(30)` instead of reading from JwtSettings config
2. **User enumeration vulnerability** — separate error messages for "email not found" vs "wrong password" let attackers discover registered emails
3. **Validation GroupBy/ToDictionary duplicated** — same code in 6 places across services (carry-forward from M2)

#### M3 Review #4 — 2026-03-04 (Bug fixes)
**Status: ~70% — Revised score: 8.5/10**

All 3 issues fixed:
1. **Options pattern** — Created `JwtSettings` class, injected `IOptions<JwtSettings>` into both AuthService and JwtTokenService. No more hardcoded values. `GenerateAccessToken` now returns tuple `(string Token, DateTime ExpiresAt)` so expiry comes from same source as actual token.
2. **User enumeration fixed** — Login now combines email lookup + password check into single error: `"Invalid email or password."` — attacker can't distinguish
3. **ValidationExtensions.ToErrorDictionary()** — Extracted extension method, now used in all 6 validation points across DepartmentService, EmployeeService, and AuthService

Also fixed:
- Department validators: `.Must()` replaced with `.Matches(@"^[A-Z0-9]+$")` — null-safe (M2 carry-forward finally resolved)
- Both DepartmentService and EmployeeService now use `ToErrorDictionary()` too

#### M3 Review #5 (Final) — 2026-03-05 — COMPLETED
**Score: 8.5/10**

Final sprint completed (built by instructor as code-along):
1. **RefreshTokenRequest DTO** — simple DTO with RefreshToken property for the refresh endpoint body
2. **AuthController** — 3 POST endpoints (register, login, refresh). Thin controller, no try-catch, no [Authorize]. All anonymous.
3. **JWT Bearer config in Program.cs:**
   - AddAuthentication with JwtBearerDefaults as default scheme
   - AddJwtBearer with full TokenValidationParameters (Issuer, Audience, SigningKey, Lifetime)
   - ClockSkew = TimeSpan.Zero — no 5-minute tolerance on expired tokens
4. **Middleware pipeline order corrected:** UseAuthentication() before UseAuthorization()
5. **[Authorize] on controllers:**
   - DepartmentController: class-level [Authorize], POST/PUT/DELETE restricted to Admin
   - EmployeeController: class-level [Authorize], POST/PUT restricted to Admin+HR, DELETE restricted to Admin
6. **Microsoft.AspNetCore.Authentication.JwtBearer package** installed in EMS_API

Authorization matrix:
| Endpoint | Admin | HR | Employee |
|---|---|---|---|
| GET departments | ✓ | ✓ | ✓ |
| POST/PUT/DELETE departments | ✓ | ✗ | ✗ |
| GET employees | ✓ | ✓ | ✓ |
| POST/PUT employees | ✓ | ✓ | ✗ |
| DELETE employees | ✓ | ✗ | ✗ |
| Auth (register/login/refresh) | Public | Public | Public |

Note: This final sprint was built by the instructor (me) as a code-along — developer asked for help to finish M3. Code quality is clean, follows all established patterns.

### Milestone 4 Progress Reviews

#### M4 Review #1 — Pagination (Sprint 1)
**Score: 8/10 → fixed to 9/10**

What was completed:
1. PagedRequest with PageNumber (default 1) and PageSize (default 10)
2. PagedResponse<T> with Items, PageNumber, PageSize, TotalCount, computed TotalPages/HasPreviousPage/HasNextPage
3. GetPagedAsync in GenericRepository — CountAsync then Skip/Take with ToListAsync
4. IDepartmentService/DepartmentService updated to use PagedRequest/PagedResponse
5. DepartmentController GetAll uses [FromQuery] PagedRequest

Issues found (3):
1. **No input validation on PagedRequest** — negative page or huge page size. FIXED: private backing fields with clamping in setters, MaxPageSize=50
2. **Employee not paginated** — FIXED: same pattern applied to EmployeeService/Controller
3. **No deterministic ordering** — Skip/Take without OrderBy gives inconsistent pages. FIXED: Created BaseEntity with Id, changed constraint from `where T : class` to `where T : BaseEntity`, added `.OrderBy(x => x.Id)` before Skip/Take

Key decision: BaseEntity abstraction — constrains generic repository to entities with Id, enabling default sorting. Shows good architectural thinking.

#### M4 Review #2 — Filtering & Searching (Sprint 2)
**Score: 8.5/10**

What was completed:
1. DepartmentQueryRequest extends PagedRequest (Search, IsActive)
2. EmployeeQueryRequest extends PagedRequest (Search, DepartmentId, Gender)
3. GetPagedAsync signature changed from single filter to `List<Expression<Func<T, bool>>>`
4. Dynamic filter building in service layer — each Where() becomes AND in SQL
5. Case-insensitive search with .ToLower().Contains()
6. Department IsActive filter overridable (defaults to true)
7. Controllers updated with [FromQuery] query DTOs

Design decision: Chose Option A (build filters in service, keep repo generic) — correct. Preserves dependency rule, keeps repo reusable, business logic stays in service.

#### M4 Review #3 — Sorting (Sprint 3)
**Score: 9/10**

What was completed:
1. SortBy and SortDescending added to PagedRequest
2. ApplySorting private static method in GenericRepository
3. Reflection with BindingFlags.IgnoreCase for case-insensitive property lookup
4. Expression tree: Expression.Parameter → Expression.Property → Expression.Convert → Expression.Lambda
5. Invalid property silently falls back to Id (no crash)
6. OrderByDescending when SortDescending=true

Expression tree understanding is strong — correctly handled boxing value types with Expression.Convert for Func<T, object>.

#### M4 Review #4 (Final) — Caching (Sprint 4)
**Score: 9.5/10**

What was completed:
1. IMemoryCache injected into DepartmentService (not repository — correct layer)
2. Cache key built from all query params (page, size, search, isActive, sortBy, sortDescending)
3. Sliding expiration (10 min) + absolute expiration (1 hour)
4. CancellationTokenSource for bulk invalidation — all entries linked via CancellationChangeToken
5. InvalidateCache() on Create, Update, Delete — cancel, dispose, replace CTS
6. AddMemoryCache() in Program.cs
7. Only departments cached (not employees — correct trade-off)

Static _cacheResetToken is correct because IMemoryCache is singleton and CTS must outlive scoped DepartmentService instances.

**Milestone 4 Final Score: 9/10**

### Milestone 5 Progress Reviews

#### M5 Review #1 — MediatR Setup + Department Queries (Sprint 1)
**Score: 9.5/10**

What was completed:
1. MediatR 14.1.0 installed in EMS_Application
2. `ValidationBehavior<TRequest, TResponse>` in `Behaviors/` — IPipelineBehavior with IEnumerable<IValidator<TRequest>>, uses Task.WhenAll for parallel validation, throws custom ValidationException
3. `GetAllDepartmentsQuery : PagedRequest, IRequest<PagedResponse<DepartmentResponse>>` — inherits PagedRequest (smart reuse), adds Search and IsActive
4. `GetAllDepartmentsHandler` — full caching logic moved from DepartmentService, only injects IUnitOfWork + IMemoryCache (no validators — read handler doesn't need them)
5. `GetDepartmentByIdQuery : IRequest<DepartmentResponse>` — just int Id
6. `GetDepartmentByIdHandler` — clean, only injects IUnitOfWork
7. DependencyInjection.cs updated: AddMediatR with assembly scanning + ValidationBehavior as IPipelineBehavior. Old service registrations kept for transitional period
8. DepartmentController — dual injection (IMediator for GET, IDepartmentService for POST/PUT/DELETE). Transitional approach is correct.
9. InvalidateCache() made public static — anticipated command handlers will need cross-handler cache invalidation
10. Folder structure: Features/Departments/Queries/GetAllDepartments/ and GetDepartmentById/

Key observations:
- Inherited GetAllDepartmentsQuery from PagedRequest instead of duplicating properties — shows good instinct for code reuse
- ValidationBehavior uses Task.WhenAll for parallel validator execution — not just correct, it's optimal
- Correctly understood the transitional pattern — didn't try to rip out everything at once
- Asked insightful question about how MediatR discovers handlers (generic type matching via DI)
- Asked about MediatR logging — correctly understood MediatR has NO built-in logging, behaviors are user-built

#### M5 Review #2 — Department Commands (Sprint 2)
**Score: 9/10**

What was completed:
1. CreateDepartmentCommand/Handler — properties directly on command (not embedded DTO), IUnitOfWork only
2. UpdateDepartmentCommand/Handler — NotFoundException check, cache invalidation
3. DeleteDepartmentCommand : IRequest (void) — correct use of MediatR void pattern, soft delete
4. Validators updated to target Commands (CreateDepartmentValidator, UpdateDepartmentValidator)
5. DepartmentController uses only IMediator — IDepartmentService removed entirely
6. DepartmentService.cs and IDepartmentService.cs deleted
7. DI registration cleaned up
8. CreatedAtAction on POST with location header
9. command.Id = id pattern for PUT (URL as single source of truth)

Issues found (1 minor):
- Dead mapping methods (ToEntity, ApplyUpdate) left in DepartmentMapping.cs — FIXED after review

Interview answer (URL vs body ID): Correct — URL identifies resource, body describes change. Controller overwrites command.Id to prevent conflict.

#### M5 Review #3 — Employee Queries + Commands (Sprint 3)
**Score: 9.5/10**

What was completed:
1. GetAllEmployeesQuery : PagedRequest, IRequest — same reuse pattern as departments
2. GetAllEmployeesHandler — dynamic filter list (Search, DepartmentId, Gender), includes Department navigation
3. GetEmployeeByIdHandler — includes Department for DepartmentName in response
4. CreateEmployeeCommand with all 10 properties, CreateEmployeeHandler
5. UpdateEmployeeCommand/Handler with NotFoundException check
6. DeleteEmployeeCommand : IRequest (void) — soft delete, no cache
7. Validators target Commands with comprehensive rules (10 rules on Create)
8. Controller uses only IMediator, authorization granularity (Admin,HR for Create/Update, Admin for Delete)
9. EmployeeService + IEmployeeService deleted, DI cleaned
10. EmployeeMapping cleaned — only ToResponse remains

No caching on employees — correct per M4 design decision.

Interview answer (list of filters vs single && expression): Correct — independent, conditional, clean code. Added technical reason: each Where() = AND in SQL, same performance.

#### M5 Review #4 — Auth Refactor (Sprint 4)
**Score: 9.5/10**

What was completed:
1. RegisterCommand/Handler — BCrypt hashing, email uniqueness check, JWT generation via IJwtTokenService
2. LoginCommand/Handler — user enumeration prevention (same error for both cases), token rotation
3. RefreshTokenCommand/Handler — BONUS (not assigned, but correctly included from old AuthService)
4. All 3 handlers inject IUnitOfWork + IJwtTokenService + IOptions<JwtSettings>
5. Validators renamed: RegisterCommandValidator, LoginCommandValidator — target Commands
6. AuthController uses only IMediator — 3 POST endpoints
7. AuthService + IAuthService deleted
8. DependencyInjection.cs has ZERO service registrations — only MediatR, FluentValidation, ValidationBehavior

RefreshTokenHandler validates expiry date and rotates tokens on every refresh.

Interview answer (Login as Command): "Because it changes state. That's the only rule that matters." — Perfect. Concise, correct, confident.

#### M5 Review #5 — LoggingBehavior (Sprint 5)
**Score: 9.5/10**

What was completed:
1. LoggingBehavior<TRequest, TResponse> — IPipelineBehavior with ILogger
2. Logs request type name on entry (LogInformation)
3. Stopwatch for execution timing
4. Logs success with elapsed milliseconds (LogInformation)
5. Logs failure with exception message (LogError) — re-throws with `throw;` (preserves stack trace)
6. Structured logging with named placeholders ({RequestName}, {ElapsedMs}) — log aggregation friendly
7. Does NOT log request payload — prevents sensitive data (passwords, tokens) in logs
8. Registered BEFORE ValidationBehavior in DI — wraps everything, catches validation failures too

**Milestone 5 Final Score: 9.5/10**

## Strengths Identified (Across Milestones)
1. Learns from feedback — every issue raised has been addressed
2. Good instinct for code organization (restructured Domain layer on own initiative)
3. Understands navigation property loading (includes Department in Employee queries)
4. Clean mapping approach with extension methods
5. ApplyUpdate pattern shows independent thinking
6. Exception design follows conventions (entityName+key pattern, field-level validation errors)
7. Validator rules are thorough — didn't miss any property
8. Options pattern adopted naturally — JwtSettings with IOptions<T>
9. Tuple return from JwtTokenService — ties ExpiresAt to actual token expiry (no drift)
10. Security awareness — fixed user enumeration after being shown the issue
11. Responsive to feedback — fixes issues in same session, doesn't push back
12. Asks good questions — asked about ClaimTypes.Role auto-detection and Options pattern binding (shows curiosity)
13. BaseEntity extraction for generic constraint — independently solved the deterministic ordering problem with the right architectural approach
14. Expression tree construction — understood boxing, reflection, and lambda building on first attempt
15. CancellationTokenSource for cache invalidation — chose the advanced pattern over simple key tracking
16. Consistent improvement trajectory — scores: 7.5 → 8.5 → 8.5 → 9.0 → 9.5
17. CQRS adoption was smooth — understood the pattern quickly, applied it correctly on first attempt
18. Query inheriting PagedRequest shows he thinks about code reuse before writing new classes
19. Initiative — built RefreshTokenCommand without being asked (Sprint 4), recognized it belonged in the refactor
20. Security awareness improved — no sensitive data in LoggingBehavior logs, user enumeration prevention preserved in LoginHandler
21. Clean delete patterns — removed all dead code (services, interfaces, mapping methods) after each sprint
22. Correct pipeline behavior ordering — LoggingBehavior before ValidationBehavior without being told why
23. Structured logging with named placeholders — understands log aggregation tools need parseable formats

## Weaknesses / Areas to Watch
1. Attention to detail on first pass — misses edge cases (null-safety, hardcoded values, security leaks)
2. Tends to hardcode values before being reminded to use config/options
3. Needs prompting to think about security implications (user enumeration wasn't caught independently)
4. Code duplication builds up until called out (validation logic was copied 6 times before extraction)
5. Needed help finishing M3 final sprint (controller + JWT config + authorization) — not a weakness per se, but shows the API/middleware wiring is less familiar than the service/domain work
