# Task Management System - Clean Architecture Implementation


## Understanding

Build a complete ASP.NET Core 8 Web API with Clean Architecture layers (Domain, Application, Infrastructure, API) and comprehensive xUnit tests covering domain validation, services, repositories, authentication, and controllers. Follow TDD by creating tests before implementation.


## Assumptions

- Using .NET 8 with Entity Framework Core
- SQL Server as the database provider
- JWT for authentication with secure password hashing (bcrypt or similar)
- xUnit + Moq for testing framework
- No existing projects in the workspace
- DI container: built-in ASP.NET Core DI
- DTOs for API contracts, no EF entities exposed directly


## Approach

1. **Structure**: Create 5 projects following dependency hierarchy (Domain ← Application ← Infrastructure ← API; Tests interfaces with all layers)
2. **Domain First**: Define entities, enums, and value objects (not dependent on any layer)
3. **Test-Driven**: For each feature, write tests first (Red), then implementation (Green)
4. **Application Layer**: Services, DTOs, interfaces for repositories & auth, business logic coordination
5. **Infrastructure**: EF Core DbContext, repository implementations, JWT service, password hashing
6. **API Layer**: Controllers, middleware, dependency injection wiring
7. **Incremental**: Complete each component end-to-end (Domain → Tests → Application → Infrastructure → API) before moving to the next


## Key Files

- TaskManagement.Domain/Entities/User.cs - User aggregate root
- TaskManagement.Domain/Entities/TaskItem.cs - Task aggregate root
- TaskManagement.Domain/Enums/TaskStatus.cs - Task status enumeration
- TaskManagement.Application/Services/AuthService.cs - Auth business logic
- TaskManagement.Application/Services/TaskService.cs - Task management logic
- TaskManagement.Infrastructure/Data/AppDbContext.cs - EF Core context
- TaskManagement.Api/Controllers/AuthController.cs - Auth endpoints
- TaskManagement.Api/Controllers/TasksController.cs - Task endpoints
- TaskManagement.Tests/* - All unit tests


## Risks & Open Questions

- Password hashing strategy: Use bcrypt via BCrypt.Net-Next NuGet
- JWT token expiration: Set reasonable defaults (15 min access, 7 day refresh if needed)
- Error handling middleware: Implement consistent error responses
- Database migrations: Create initial migration and seed if needed
- Concurrency: Optimistic locking via EF Core version tokens (optional for scope)


## Plan Steps

- ✅ **Create solution and five projects (Domain, Application, Infrastructure, API, Tests)**
- ✅ **Define Domain layer: User and TaskItem entities, TaskStatus enum**
- ✅ **Create Application layer interfaces: IRepository, IAuthService, ITaskService, DTOs**
- ✅ **Write Domain & Application unit tests (TDD)**
- 🔄 **Implement Infrastructure: DbContext, repository, auth service, password hashing**
-  **Write Infrastructure tests (mocking DbContext)**
-  **Create API project structure and dependency injection configuration**
-  **Implement Auth controllers and endpoints**
-  **Write Auth integration/controller tests**
-  **Implement Task controllers and endpoints**
-  **Write Task integration/controller tests**
-  **Final build validation and any remaining polish**

---

## References

- Check [README.md](README.md) for the full documentation
