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
| Entity Design & EF Core (Fluent API) | Reviewed M1 | Solid |
| Repository & Unit of Work Pattern | Reviewed M1 | Solid implementation with filter+includes |
| DTOs & Manual Mapping | Done M2 | DTOs + mapping done, ApplyUpdate pattern is good |
| Validation (FluentValidation) | Done M2 | 4 validators created, all rules correct. One bug: uppercase null-safety (pending fix) |
| Custom Exceptions | Done M2 | NotFoundException, BadRequestException, ValidationException — clean implementations |
| ApiResponse<T> Wrapper | Done M2 | Factory methods, fixed null-safety on Message property after review |
| Error Handling & Middleware | Not Started | Pending — last piece of M2 |
| Authentication (JWT) | Not Started | - |
| Authorization (Role & Policy based) | Not Started | - |
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
- Controllers still have try-catch — will be fixed when middleware is added

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
- `.Must(code => code == code.ToUpper())` — NullReferenceException if Code is null (PENDING FIX)
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

Bugs found during final review (both fixed):
- `nameof(department)` / `nameof(existing)` producing wrong entity names in NotFoundException — fixed to string literals
- DepartmentService not injecting/using validators despite validators existing — fixed

Minor carry-forward items (not blocking):
- Validation GroupBy/ToDictionary logic duplicated 4x — extract helper eventually
- `Exceptions.ValidationException` fully qualified (namespace conflict) — using alias would be cleaner
- `null!` in delete responses — works but is a code smell

## Strengths Identified (Across Milestones)
1. Learns from feedback — every M1 issue was addressed, ApiResponse fix applied immediately
2. Good instinct for code organization (restructured Domain layer on own initiative)
3. Understands navigation property loading (includes Department in Employee queries)
4. Clean mapping approach with extension methods
5. ApplyUpdate pattern shows independent thinking
6. Exception design follows conventions (entityName+key pattern, field-level validation errors)
7. Validator rules are thorough — didn't miss any property

## Weaknesses / Areas to Watch
1. Attention to detail — null-safety bug in uppercase validator (missed edge case)
2. Still using KeyNotFoundException instead of custom exceptions (pending fix)
3. Controllers still have try-catch pattern (pending middleware)
