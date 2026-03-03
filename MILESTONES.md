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

## Milestone 2: DTOs, Validation & Global Error Handling — IN PROGRESS (~80%)
**Challenge:** Never expose domain entities directly. Handle errors like a pro.

### What's DONE:
- [x] Request/Response DTOs (CreateDepartmentRequest, UpdateDepartmentRequest, DepartmentResponse, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeResponse)
- [x] Manual mapping with extension methods (DepartmentMapping, EmployeeMapping) — includes ApplyUpdate pattern
- [x] Service interfaces updated to use DTOs
- [x] Services refactored to use mapping
- [x] Controllers updated to use DTOs
- [x] Employee queries include Department navigation (for DepartmentName)
- [x] Domain restructured (Entities/ folder, Enum/ folder)
- [x] All M1 carry-forward fixes applied
- [x] Custom Exceptions (NotFoundException, BadRequestException, ValidationException) in EMS_Application/Exceptions/
- [x] ApiResponse<T> wrapper with factory methods in EMS_Application/Common/
- [x] FluentValidation validators (4 files) in EMS_Application/Validators/
- [x] FluentValidation.DependencyInjectionExtensions package installed
- [x] Validators registered in DependencyInjection.cs via AddValidatorsFromAssembly

### What's REMAINING:
- [ ] Fix uppercase validator null-safety bug in Department validators
- [ ] Global Exception Handling Middleware in API/Middleware/
  - NotFoundException → 404
  - ValidationException → 422
  - BadRequestException → 400
  - Any other → 500 (don't leak details)
- [ ] Replace KeyNotFoundException with NotFoundException in services
- [ ] Remove try-catch from controllers (middleware handles everything)
- [ ] Register middleware in Program.cs

**Review scores so far:**
- M2 Review #1 (DTOs & Mapping): Good — ApplyUpdate pattern, clean extension methods
- M2 Review #2 (Exceptions, ApiResponse, Validators): 8/10 — one null-safety bug to fix

**Interview topics this covers:**
- "Why use DTOs?"
- "How do you handle validation in .NET?"
- "How do you handle errors globally?"
- "What middleware have you written?"
- "Why manual mapping over AutoMapper?"
- "Where should validation happen?"

**Deliverable:** Employee + Department CRUD with proper DTOs, validation, error responses.

---

## Milestone 3: Authentication & Authorization — NOT STARTED
**Challenge:** Secure your API with JWT and implement role-based access.

**You will cover:**
1. Identity setup (or custom user entity)
2. JWT token generation & validation
3. Refresh tokens
4. Register / Login endpoints
5. Role-based authorization (Admin, HR, Employee)
6. Policy-based authorization
7. Protecting endpoints with `[Authorize]`
8. Current user service (extracting claims from token)

**Interview topics this covers:**
- "Explain JWT authentication flow"
- "Difference between Authentication and Authorization"
- "How do refresh tokens work?"
- "Role-based vs Policy-based authorization"

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
