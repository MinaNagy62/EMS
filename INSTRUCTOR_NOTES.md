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
| DTOs & Manual Mapping | Partially done M2 | DTOs + mapping done, ApplyUpdate pattern is good |
| Validation (FluentValidation) | Not Started | Pending in M2 |
| Error Handling & Middleware | Not Started | Pending in M2 |
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

#### Issues NOT Fixed (carried to M2, some still pending)
- Unused `using System.Linq.Expressions` in IDepartmentService — FIXED in M2 (now uses DTO imports)
- Controllers still have try-catch — will be fixed when middleware is added

### Milestone 2 Progress Review — 2026-03-02 (IN PROGRESS)
**Status: ~50% complete**

#### What was completed:
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

#### What is still pending:
1. FluentValidation validators (4 validators)
2. Custom exceptions (NotFoundException, BadRequestException, ValidationException)
3. Global Exception Handling Middleware
4. ApiResponse<T> wrapper
5. Remove try-catch from controllers (after middleware)
6. Replace KeyNotFoundException with custom NotFoundException in services
7. Register validators in DependencyInjection.cs

#### Early observations on M2 work:
- Developer improved code organization significantly between M1 and M2
- ApplyUpdate mapping pattern shows good thinking — separating creation mapping from update mapping
- Employee queries correctly include Department for DepartmentName resolution
- Null-safe DepartmentName mapping: `employee.Department?.Name ?? string.Empty`

## Strengths Identified (Across Milestones)
1. Learns from feedback — every M1 issue was addressed
2. Good instinct for code organization (restructured Domain layer on own initiative)
3. Understands navigation property loading (includes Department in Employee queries)
4. Clean mapping approach with extension methods

## Weaknesses / Areas to Watch
1. Attention to detail — some carry-forward items get missed on first pass
2. Still using KeyNotFoundException instead of custom exceptions
3. Controllers still have try-catch pattern (needs middleware)
4. No validation yet — this is the biggest gap remaining
