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

## Milestone 3: Authentication & Authorization — IN PROGRESS (~70%)
**Challenge:** Secure your API with JWT and implement role-based access.

### What's DONE:
- [x] AppUser entity (Id, FirstName, LastName, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiryDate, IsActive, CreatedAt)
- [x] Role enum (Admin=1, HR=2, Employee=3)
- [x] AppUserConfiguration (Fluent API: HasKey, max lengths, unique email index, PasswordHash required)
- [x] AppDbContext updated with DbSet<AppUser>
- [x] Migration created (20260304092250_A_E_AppUsers)
- [x] AppUserRepository extending GenericRepository<AppUser>
- [x] IUnitOfWork + UnitOfWork updated with AppUsers (lazy init)
- [x] Auth DTOs: RegisterRequest, LoginRequest, AuthResponse (Token, RefreshToken, ExpiresAt, Email, Role)
- [x] IAuthService interface (RegisterAsync, LoginAsync, RefreshTokenAsync)
- [x] AuthService implementation with all 3 flows:
  - Register: validate → check email unique → BCrypt hash → generate tokens → save refresh token → return
  - Login: validate → find user → combined email/password error (prevents user enumeration) → generate tokens → save → return
  - Refresh: find by token → check expiry → rotate tokens → save → return
- [x] IJwtTokenService interface — returns `(string Token, DateTime ExpiresAt)` tuple
- [x] JwtTokenService implementation — HS256, claims (Sub, Email, Role, Jti), IOptions<JwtSettings>, RandomNumberGenerator for refresh tokens
- [x] JwtSettings class (Options pattern) — SecretKey, Issuer, Audience, AccessTokenExpirationMinutes, RefreshTokenExpirationDays
- [x] JWT settings in appsettings.json, registered via services.Configure<JwtSettings>()
- [x] Auth validators: RegisterRequestValidator (FirstName/LastName 2-50, Email valid, Password min 8), LoginRequestValidator (Email valid, Password required)
- [x] BCrypt.Net-Next package installed
- [x] Microsoft.AspNetCore.Authentication.JwtBearer package installed
- [x] Microsoft.Extensions.Options package installed
- [x] DI registered: IAuthService→AuthService (Application), IJwtTokenService→JwtTokenService (Infrastructure)
- [x] ValidationExtensions.ToErrorDictionary() used across all services

### What's REMAINING:
- [ ] AuthController (POST /api/auth/register, POST /api/auth/login, POST /api/auth/refresh)
- [ ] RefreshTokenRequest DTO (for refresh endpoint body)
- [ ] JWT authentication configuration in Program.cs (AddAuthentication, AddJwtBearer with token validation)
- [ ] `app.UseAuthentication()` before `app.UseAuthorization()` in pipeline
- [ ] `[Authorize]` on DepartmentController and EmployeeController (class level)
- [ ] AuthController stays anonymous (no [Authorize])
- [ ] Role-based authorization (Admin, HR, Employee policies)
- [ ] Current user service (extract claims from token) — optional

### Review scores so far:
- M3 Review #1 (Foundation): Good — clean entity/enum/config
- M3 Review #2 (DTOs, Interface, Config): Good — minor naming nits only
- M3 Review #3 (Core Implementation): 7.5/10 → fixed to 8.5/10 (3 issues found and fixed: hardcoded values, user enumeration, validation duplication)

### Security decisions:
- Login: same error for wrong email AND wrong password (prevents user enumeration)
- Refresh token rotation on every refresh (old token invalidated)
- 500 errors never leak internal details
- BCrypt for password hashing (intentionally slow, salted)
- No hardcoded expiry values — all from JwtSettings via Options pattern

**Interview topics this covers:**
- "Explain JWT authentication flow"
- "Difference between Authentication and Authorization"
- "How do refresh tokens work?"
- "Role-based vs Policy-based authorization"
- "Why BCrypt over SHA256?"
- "What is user enumeration and how do you prevent it?"
- "What is the Options pattern?"
- "What is token rotation?"

**Deliverable:** Secured API — only authenticated users access resources, roles control actions.

---

## Milestone 4: Advanced Querying & Performance — NOT STARTED
**Challenge:** Handle real-world data scenarios — pagination, filtering, caching.

**You will cover:**
1. Pagination (PagedList<T>)
2. Filtering & Searching (dynamic query building)
3. Sorting (dynamic, by any property)
4. Specification Pattern (encapsulate query logic)
5. In-Memory Caching (IMemoryCache)
6. Response caching headers
7. Attendance entity + endpoints (track employee check-in/out)
8. EF Core query optimization (AsNoTracking, Select projections, Include)

**Interview topics this covers:**
- "How do you implement pagination?"
- "What is the Specification Pattern?"
- "How do you optimize EF Core queries?"
- "What caching strategies have you used?"

**Deliverable:** Efficient, queryable endpoints with caching.

---

## Milestone 5: CQRS with MediatR & Advanced Patterns — NOT STARTED
**Challenge:** Separate reads from writes using CQRS + MediatR.

**You will cover:**
1. MediatR setup (Commands & Queries)
2. CQRS pattern (separate read/write models)
3. MediatR Pipeline Behaviors (logging, validation)
4. Leave Request feature (Apply, Approve, Reject — state machine)
5. Domain Events (LeaveApproved, LeaveRejected)
6. Notification handlers (react to domain events)
7. Refactor existing endpoints to use MediatR

**Interview topics this covers:**
- "What is CQRS and when would you use it?"
- "How does MediatR work?"
- "What are Pipeline Behaviors?"
- "How do you handle domain events?"

**Deliverable:** Leave management with full CQRS, domain events, and pipeline behaviors.

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
