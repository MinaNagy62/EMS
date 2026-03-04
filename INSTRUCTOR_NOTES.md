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
| Authentication (JWT) | In Progress M3 | JwtTokenService done, AuthService done with all 3 flows. Still needs AuthController + middleware config |
| Authorization (Role & Policy based) | Not Started | Pending in M3 |
| Pagination, Filtering, Sorting | Not Started | - |
| Caching (In-Memory & Distributed) | Not Started | - |
| Background Jobs (Hangfire/Hosted Services) | Not Started | - |
| Logging (Serilog structured logging) | Not Started | - |
| Unit Testing & Integration Testing | Not Started | - |
| API Versioning | Not Started | - |
| Rate Limiting | Not Started | - |
| CQRS with MediatR | Not Started | - |
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

#### What is still pending to complete M3:
1. AuthController (POST /api/auth/register, POST /api/auth/login, POST /api/auth/refresh)
2. RefreshTokenRequest DTO (for refresh endpoint body)
3. JWT authentication configuration in Program.cs (AddAuthentication, AddJwtBearer)
4. `app.UseAuthentication()` before `app.UseAuthorization()` in pipeline
5. Install Microsoft.AspNetCore.Authentication.JwtBearer in EMS_API
6. `[Authorize]` on DepartmentController and EmployeeController
7. Role-based authorization policies
8. Current user service (optional for M3)

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

## Weaknesses / Areas to Watch
1. Attention to detail on first pass — misses edge cases (null-safety, hardcoded values, security leaks)
2. Tends to hardcode values before being reminded to use config/options
3. Needs prompting to think about security implications (user enumeration wasn't caught independently)
4. Code duplication builds up until called out (validation logic was copied 6 times before extraction)
