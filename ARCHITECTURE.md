# Architecture Overview

This solution follows Clean Architecture principles with clear separation of concerns across four core layers:
```
┌─────────────────────────────────────────────────────────────┐
│                          API Layer                          │
│                     (TaskManagement.Api)                    │
│    Controllers, Middleware, Dependency Injection, Routing   │
├─────────────────────────────────────────────────────────────┤
│                      Application Layer│                     │
│                 (TaskManagement.Application)                │
│   Services, DTOs, Interfaces, Business Logic Orchestration  │
├─────────────────────────────────────────────────────────────┤
│                         Domain Layer                        │
│                   (TaskManagement.Domain)                   │
│              Entities, Enums, Validation Rules              │
├─────────────────────────────────────────────────────────────┤
│                     Infrastructure Layer                    │
│               (TaskManagement.Infrastructure)               │
│      EF Core, Repositories, Security, External Services     │
└─────────────────────────────────────────────────────────────┘
```


## Project Structure

```
TaskManagementSystem/
├── src/
│   ├── TaskManagement.Api/                    # REST API Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs              # Authentication endpoints
│   │   │   └── TasksController.cs             # Task management endpoints
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs # DI configuration
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs # Global exception handling
│   │   ├── Program.cs                         # Application startup
│   │   └── appsettings.json                   # Configuration
│   │
│   ├── TaskManagement.Application/            # Application / Business Logic Layer
│   │   ├── DTOs/
│   │   │   ├── AuthRequestDto.cs              # Auth request DTOs
│   │   │   ├── AuthResponseDto.cs             # Auth response DTO
│   │   │   ├── CreateTaskDto.cs               # Task creation DTO
│   │   │   ├── UpdateTaskDto.cs               # Task update DTO
│   │   │   └── TaskItemDto.cs                 # Task read DTO
│   │   ├── Interfaces/
│   │   │   ├── IAuthenticationService.cs      # Auth service contract
│   │   │   ├── ITaskService.cs                # Task service contract
│   │   │   ├── IUserRepository.cs             # User repository contract
│   │   │   ├── ITaskRepository.cs             # Task repository contract
│   │   │   ├── IPasswordHasher.cs             # Password hashing contract
│   │   │   └── IJwtTokenProvider.cs           # JWT token contract
│   │   ├── Services/
│   │   │   ├── AuthenticationService.cs       # Authentication implementation
│   │   │   └── TaskService.cs                 # Task operations implementation
│   │   ├── Exceptions/
│   │   │   ├── ValidationException.cs         # Validation errors
│   │   │   ├── NotFoundException.cs           # Resource not found
│   │   │   └── ForbiddenException.cs          # Authorization errors
│   │   └── TaskManagement.Application.csproj
│   │
│   ├── TaskManagement.Domain/                 # Domain / Entity Layer
│   │   ├── Entities/
│   │   │   ├── User.cs                        # User entity
│   │   │   └── TaskItem.cs                    # TaskItem entity
│   │   ├── Enums/
│   │   │   └── TaskStatus.cs                  # Task status enumeration
│   │   └── TaskManagement.Domain.csproj
│   │
│   └── TaskManagement.Infrastructure/         # Infrastructure / Persistence Layer
│       ├── AppDbContext.cs                    # Entity Framework DbContext
│       ├── Repositories/
│       │   ├── UserRepository.cs              # User data access
│       │   └── TaskRepository.cs              # Task data access
│       ├── Security/
│       │   ├── PasswordHasher.cs              # BCrypt password hashing
│       │   └── JwtTokenProvider.cs            # JWT token generation
│       ├── Migrations/
│       │   └── (EF Core migrations)
│       └── TaskManagement.Infrastructure.csproj
│
└── tests/
    └── TaskManagement.Tests/                  # Unit & Integration Tests
        ├── AuthenticationServiceTests.cs      # Auth service tests
        ├── TaskServiceTests.cs                # Task service tests
        ├── RepositoryTests.cs                 # Repository tests
        └── TaskManagement.Tests.csproj
```


## Key Architectural Decisions

### 1. **Dependency Inversion**
- All dependencies flow inward toward the Domain layer
- Domain has zero external dependencies (no EF, no external frameworks)
- Application depends only on Domain
- Infrastructure implements application interfaces
- API has no direct database access

### 2. **Repository Pattern**
- Abstract data access behind `IUserRepository` and `ITaskRepository`
- Enables easy mocking in tests and future persistence changes
- Single Responsibility: each repository focuses on one entity

### 3. **Service Layer**
- `AuthenticationService` handles all authentication logic separately from controllers
- `TaskService` encapsulates all task business rules
- Services orchestrate between repositories, validators, and external services
- Controllers remain thin and focused on HTTP concerns

### 4. **DTOs (Data Transfer Objects)**
- Never expose domain entities directly from API endpoints
- DTOs are request/response contracts
- Decouples API contract from domain model changes
- Examples: `CreateTaskDto`, `UpdateTaskDto`, `TaskItemDto`, `AuthResponseDto`

### 5. **Validation Strategy**
- Business rule validation in service layer (not controllers)
- Detailed validation messages for API consumers
- Validation exceptions caught by middleware and converted to 400 responses

### 6. **Authentication & Authorization**
- JWT bearer tokens for stateless authentication
- User ID extracted from JWT claims, never from request body
- Authorization rules enforced in services (not just controllers)
- Users can only access/modify their own tasks

### 7. **Exception Handling**
- Custom exceptions for known error types (`ValidationException`, `NotFoundException`, `ForbiddenException`)
- Global exception handling middleware converts to HTTP status codes
- Prevents information leakage while providing meaningful responses

### 8. **Async/Await Throughout**
- All I/O operations use async patterns
- Cancellation token support for graceful shutdown
- Scalable resource usage under load


## API Endpoints

### Authentication
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123"
}

Response 201 Created:
{
  "userId": 1,
  "username": "johndoe",
  "email": "john@example.com",
  "token": "eyJhbGc..."
}
```
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123"
}

Response 200 OK:
{
  "userId": 1,
  "username": "johndoe",
  "email": "john@example.com",
  "token": "eyJhbGc..."
}
```

### Tasks
All task endpoints require `Authorization: Bearer {token}` header.
```
GET /api/tasks
Response 200 OK: List of all tasks for authenticated user

GET /api/tasks/{id}
Response 200 OK: Single task detail
Response 404 Not Found: Task doesn't exist or not owned by user
Response 403 Forbidden: Task owned by different user

POST /api/tasks
Content-Type: application/json
{
  "title": "Implement authentication",
  "description": "Add JWT-based auth",
  "dueDate": "2024-12-31"
}
Response 201 Created: Created task object
Response 400 Bad Request: Validation error

PUT /api/tasks/{id}
Content-Type: application/json
{
  "title": "Updated title",
  "status": "InProgress"
}
Response 200 OK: Updated task
Response 404 Not Found: Task doesn't exist
Response 403 Forbidden: Not task owner

DELETE /api/tasks/{id}
Response 204 No Content: Successful deletion
Response 404 Not Found: Task doesn't exist
Response 403 Forbidden: Not task owner
```


## Validation Rules

### Password Complexity
- Minimum 8 characters
- Must contain uppercase letter (A-Z)
- Must contain lowercase letter (a-z)
- Must contain digit (0-9)

### Email
- Must be valid email format

### Username
- 3-100 characters

### Task Title
- Required
- 3-100 characters

### Task Description
- Optional
- Maximum 1000 characters

### Task Due Date
- Cannot be in the past when creating/updating

### Task Status
- Must be: `Pending`, `InProgress`, or `Completed`


## Setting Up & Running

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB or express)
- Visual Studio 2022 or VS Code

### Installation
1. **Clone the repository**
```sh
git clone https://github.com/DougBorges/TaskManagementSystem.git
cd TaskManagementSystem
```

2. **Update appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "SecretKey": "your-very-secure-secret-key-must-be-at-least-32-characters-long-for-hs256",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementClient",
    "ExpirationMinutes": 60
  }
}
```

### For Production
- Store JWT secret in secure configuration (Azure Key Vault, AWS Secrets Manager, etc.)
- Change connection string to production SQL Server
- Enable HTTPS enforceement
- Configure CORS appropriately
- Set logging to Warning or Error

3. **Create Database**
```sh
cd src/TaskManagement.Api
dotnet ef database update
```

4. **Build Solution**
```sh
dotnet build
```

5. **Run Tests**
```sh
dotnet test
```

6. **Run API**
```sh
dotnet run --project src/TaskManagement.Api
```

API will be available at `https://localhost:5001` (or `http://localhost:5000`)

Open `https://localhost:5001/swagger` for Swagger UI to test endpoints.


## Testing

Unit tests use xUnit and Moq with comprehensive coverage:
```sh
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "ClassName=AuthenticationServiceTests"

# Run and generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Test Structure
- **AuthenticationServiceTests.cs**: Register/login validation, password handling
- **TaskServiceTests.cs**: CRUD operations, authorization, validation
- **RepositoryTests.cs**: Data access contracts, domain validation

Each test follows AAA pattern: Arrange, Act, Assert.


## Technology Stack

- **Framework**: ASP.NET Core 8
- **Authentication**: JWT Bearer
- **Database**: SQL Server with Entity Framework Core 8
- **Password Hashing**: BCrypt.Net
- **Testing**: xUnit + Moq
- **Build**: .NET CLI / Visual Studio
- **API Documentation**: Swagger/OpenAPI


## HTTP Status Codes

- **200 OK**: Successful GET/PUT operation
- **201 Created**: Successful POST/resource creation
- **204 No Content**: Successful DELETE operation
- **400 Bad Request**: Validation error or malformed request
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: User lacks permission (e.g., accessing another user's task)
- **404 Not Found**: Resource doesn't exist
- **500 Internal Server Error**: Unhandled server exception


## Error Response Format

All errors return JSON with descriptive message:
```json
{
  "error": "Title must be between 3 and 100 characters"
}
```


## Security Considerations

1. **Passwords**: Never stored in plain text. BCrypt hashing with automatic salt.
2. **JWT Token**: 
   - HS256 algorithm with 32+ character secret
   - Expires after configured time (default 60 minutes)
   - Issued with user ID claim for authorization
3. **Authorization**: Service-layer checks ensure users can only access their own tasks
4. **SQL Injection**: Parameterized queries via EF Core
5. **CORS**: Configured to allow cross-origin requests (customize for production)
6. **HTTPS**: Enforced in production


## Database Schema

### Users Table
- `Id` (INT, Primary Key, Identity)
- `Username` (VARCHAR(100), Not Null)
- `Email` (VARCHAR(255), Not Null, Unique)
- `PasswordHash` (VARCHAR(255), Not Null)
- `CreatedAt` (DATETIME2, Not Null)

### Tasks Table
- `Id` (INT, Primary Key, Identity)
- `UserId` (INT, Foreign Key, Cascade Delete)
- `Title` (VARCHAR(100), Not Null)
- `Description` (VARCHAR(1000), Nullable)
- `Status` (INT, Not Null) [0=Pending, 1=InProgress, 2=Completed]
- `DueDate` (DATE, Nullable)
- `CreatedAt` (DATETIME2, Not Null)


## Future Enhancements

- [ ] Refresh token implementation
- [ ] Role-based access control (Admin, User roles)
- [ ] Task categories/tags
- [ ] Task subtasks/checklist
- [ ] Task comments/collaboration
- [ ] Email notifications
- [ ] Audit logging
- [ ] API rate limiting
- [ ] GraphQL endpoint
- [ ] Docker containerization


## Contributing

This is a demonstration project following best practices for ASP.NET Core development. Key principles applied:

1. SOLID principles throughout
2. Clean code and meaningful naming
3. Comprehensive test coverage
4. Proper separation of concerns
5. Async patterns for scalability
6. Proper exception handling
7. Security best practices


## License

This project is part of a technical interview assessment.


## Contact

For questions about the implementation or architecture decisions, please refer to code comments and documentation.
</parameter>
</invoke>
</function_calls>

Now let's create an ARCHITECTURE.md document:
<function_calls>
<invoke name="run_command_in_terminal">
<parameter name="command">cd "D:\dev\Technical Interview Test Ballast Lane\TaskManagementSystem" && New-Item -ItemType File -Name "ARCHITECTURE.md" -Force</parameter>
</invoke>
</function_calls>
<function_calls>
<invoke name="replace_string_in_file">
<parameter name="file_path">D:\dev\Technical Interview Test Ballast Lane\TaskManagementSystem\ARCHITECTURE.md</parameter>
<parameter name="old_string"></parameter>
<parameter name="new_string"># Architecture Documentation


## Clean Architecture Principles

This project implements Robert C. Martin's Clean Architecture, which emphasizes:

1. **Independence from Frameworks**: Business logic doesn't depend on frameworks
2. **Testability**: Core logic can be tested without external dependencies
3. **Independence of UI**: Business rules don't change when UI changes
4. **Independence of Database**: Can swap persistence layer easily
5. **Independence of External Agencies**: Business rules isolated from external services


## Dependency Flow

```
Domain (innermost, no dependencies)
   ↑
Application (depends on Domain only)
   ↑
Infrastructure (implements Application interfaces)
   ↑
API (depends on Application through DI)
```

**Key Rule**: Dependencies ALWAYS point inward. Never let inner layers know about outer layers.


## Layer Responsibilities

### Domain Layer (`TaskManagement.Domain`)
**Purpose**: Defines entities and core business rules.

**Contains**:
- `User` entity - represents a system user
- `TaskItem` entity - represents a todo item
- `TaskStatus` enum - valid task states

**Characteristics**:
- ✅ No external dependencies
- ✅ No database code
- ✅ No framework-specific code
- ✅ Pure C# and business logic only
- ✅ Entities contain only data, not behavior

**Usage Patterns**:
```csharp
var task = new TaskItem 
{ 
    UserId = userId,
    Title = title,
    Status = TaskStatus.Pending,
    CreatedAt = DateTime.UtcNow
};
// No database logic here - just value objects
```

### Application Layer (`TaskManagement.Application`)
**Purpose**: Organizes and orchestrates business operations using domain objects.

**Contains**:

1. **Services**
   - `AuthenticationService`: Handles user registration/login
   - `TaskService`: Manages task CRUD operations
   
   **Pattern**: Services coordinate repositories, validators, and business logic
```csharp
public async Task<TaskItemDto> CreateTaskAsync(CreateTaskDto request, int userId)
{
    // 1. Validate input
    ValidateCreateTaskRequest(request);
    
    // 2. Enforce business rules
    if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow.Date)
        throw new ValidationException("Due date cannot be in the past");
    
    // 3. Create domain object
    vartask = new TaskItem { /* ... */ };
       
       // 4. Persist via repository
       var created = await _taskRepository.AddAsync(task, cancellationToken);
       
       // 5. Return DTO
       return MapToDto(created);
   }
```

2. **DTOs (Data Transfer Objects)**
   - `CreateTaskDto`: What client sends when creating
   - `UpdateTaskDto`: What client sends when updating
   - `TaskItemDto`: What API returns to client
   - `AuthResponseDto`: What API returns after auth
   
   **Why**: Decouples API contract from domain model
```csharp
// API contract never changes
public class CreateTaskDto 
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
}

// Internal TaskItemcan change without breaking API
   public class TaskItem 
   {
       public int Id { get; set; }
       public int UserId { get; set; }
       // ... fields ...
   }
```

3. **Interfaces** (Contracts)
   - `IAuthenticationService`: Auth operations contract
   - `ITaskService`: Task operations contract
   - `IUserRepository`: User data access contract
   - `ITaskRepository`: Task data access contract
   - `IPasswordHasher`: Password hashing contract
   - `IJwtTokenProvider`: Token generation contract
   
   **Why**: Enable dependency injection, make services interchangeable, allow mocking

4. **Exceptions**
   - `ValidationException` (400 Bad Request)
   - `NotFoundException` (404 Not Found)
   - `ForbiddenException` (403 Forbidden)
   
   **Why**: Custom exceptions communicate intent and enable precise error handling

**Characteristics**:
- ✅ Depends on Domain only
- ✅ No Entity Framework code
- ✅ No HTTP/API code
- ✅ Orchestrates domain objects and repositories
- ✅ Contains business logic validation
- ✅ Returns DTOs, not domain entities

**Key Design**: Services are the "use case" layer - they represent business operations.

### Infrastructure Layer (`TaskManagement.Infrastructure`)
**Purpose**: Implements technical concerns and interfaces defined by Application.

**Contains**:

1. **Data Access** (`AppDbContext`, Repositories)
```csharp
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    
    // Implements ITaskRepository interface
    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        var entry = await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }
}
```
   - Implements `IUserRepository` and `ITaskRepository`
   - Handles all database queries
   - Uses Entity Framework Core for data access
   - Repositories are thin - minimal logic, mostly query building

2. **Security** (Password hasher, JWT provider)
```csharp
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
   }
```
   - Implements `IPasswordHasher` using BCrypt
   - Implements `IJwtTokenProvider` using System.IdentityModel.Tokens.Jwt

3. **Entity Framework Configuration**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
       modelBuilder.Entity<User>(entity =>
       {
           entity.HasKey(e => e.Id);
           entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
           entity.HasIndex(e => e.Email).IsUnique();
       });
   }
```

**Characteristics**:
- ✅ Implements Application interfaces
- ✅ Handles Entity Framework configuration
- ✅ Manages database migrations
- ✅ Pure technical implementations
- ✅ Can be replaced (e.g., swap SQL Server for PostgreSQL)

**Key Design**: Infrastructure is pluggable. Could implement `ITaskRepository` with REST API, files, MongoDB, etc.

### API Layer (`TaskManagement.Api`)
**Purpose**: HTTP endpoint exposure and dependency injection.

**Contains**:

1. **Controllers** (Thin HTTP handlers)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    
    public TasksController(ITaskService taskService) => _taskService = taskService;
    
    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> Create([FromBody] CreateTaskDto request)
    {
        var userId = GetUserId(); // Extract from JWT claims
        var task = await _taskService.CreateTaskAsync(request, userId); // Delegate to service
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
       }
   }
```
   - Pure HTTP routing and status code handling
   - All logic delegated to services
   - Extract claims from JWT
   - Never access DbContext directly

2. **Dependency Injection Setup**
```csharp
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    services.AddScoped<IUserRepository, UserRepository>();
       services.AddScoped<ITaskRepository, TaskRepository>();
       services.AddScoped<IAuthenticationService, AuthenticationService>();
       services.AddScoped<ITaskService, TaskService>();
       // ...
   }
```
   - Wires up all dependencies
   - Configures database
   - Only here do we instantiate concrete implementations

3. **Middleware** (Request/response pipeline)
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try {await _next(context); }
           catch (ValidationException ex) // Custom exception
           {
               context.Response.StatusCode = 400;
               await context.Response.WriteAsync(JsonSerializer.Serialize(
                   new { error = ex.Message }));
           }
       }
   }
```
   - Global exception handling
   - Converts custom exceptions to HTTP status codes
   - Prevents information leakage

4. **Configuration**
   - JWT secret, issuer, audience
   - Database connection string
   - CORS settings
   - Logging configuration

**Characteristics**:
- ✅ Depends on Application and Infrastructure
- ✅ Controllers are thin (1-5 lines of logic typically)
- ✅ DI configures all wiring
- ✅ Business logic stays in services
- ✅ HTTP concerns only in controllers/middleware


## Data Flow Example: Create Task

```
1. HTTP Request arrives
   POST /api/tasks
   Authorization: Bearer {jwt}
   { "title": "My Task", "dueDate": "2024-12-31" }
   
2. TasksController.Create()
   - Extracts userId from JWT claims
   - Calls _taskService.CreateTaskAsync(dto, userId)
   
3. TaskService.CreateTaskAsync()
   - Validates input (Title 3-100 chars, DueDate not past)
   - Creates TaskItem entity
   - Calls _taskRepository.AddAsync(task)
   
4. TaskRepository.AddAsync()
   - Calls _context.Tasks.AddAsync()
   - Calls _context.SaveChangesAsync()
   - Returns persisted entity
   
5. TaskService maps to DTO
   - Returns TaskItemDto to controller
   
6. TasksController returns HTTP response
   - 201 Created with Location header
   - Response body: { "id": 1, "title": "My Task", ... }
```


## Testing Strategy

Tests ensure each layer works correctly in isolation:
```csharp
// Service test - mocks repository, validates service logic
public async Task CreateTaskAsync_WithValidRequest_ReturnsTaskDto()
{
    // Arrange: Mock repository
    var mockRepo = new Mock<ITaskRepository>();
    var service = new TaskService(mockRepo.Object);
    
    var request = new CreateTaskDto { Title = "Test" };
    var expectedTask = new TaskItem { Id = 1, Title = "Test" };
    
    mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedTask);
    
    // Act
    var result = await service.CreateTaskAsync(request, userId: 1);
    
    // Assert: Verify service worked correctly
    Assert.Equal("Test", result.Title);
    mockRepo.Verify(r => r.AddAsync(...), Times.Once); // Verified it called repo
}
```

**Why mocking?**
- Tests logic in isolation
- Don't actually hit database
- Run in milliseconds
- Can test failure paths easily


## SOLID Principles Applied

### Single Responsibility
- Each class has one reason to change
- `AuthenticationService` handles only auth
- `TaskService` handles only tasks
- Repositories handle only data access

### Open/Closed
- Services open for extension, closed for modification
- Can add new auth methods without changing existing code
- Can implement `ITaskRepository` for different database

### Liskov Substitution
- `TaskRepository` implements `ITaskRepository`
- Could implement with SQL, NoSQL, REST API - all work the same
- Controllers don't care which implementation they receive

### Interface Segregation
- `ITaskService` defines only task operations
- `ITaskRepository` defines only data persistence
- Services don't implement unnecessary interfaces

### Dependency Inversion
- Depend on abstractions (`ITaskService`), not concretions (`TaskService`)
- Database implementation is abstracted away
- Can swap implementations without changing consumers


## Comparison: Tight vs Loose Coupling

### ❌ Tightly Coupled (Bad)
```csharp
public class TaskController : ControllerBase
{
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
        // Direct database access - violates all principles
        var context = new AppDbContext();
        var task = new TaskItem { Title = dto.Title };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        
        return Ok(task); // Exposing domain entity
    }
}
```
Problems:
- Direct DB access in controller
- Can't mock context for testing
- No separation of concerns
- Entity exposed in API

### ✅ Loosely Coupled (Good)
```csharp
public class TaskController : ControllerBase
{
    private readonly ITaskService _service;
    
    public TaskController(ITaskService service) => _service = service;
    
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
        var userId = GetUserId(); // From JWT
        var result = await _service.CreateTaskAsync(dto, userId); // Delegate to service
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```
Advantages:
- All logic in service
- Service injected - can mock
- Clear separation
- Returns DTO, not entity
- Testable without database


## Configuration Management

Environment-specific configuration is stored in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)..."
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementClient",
    "ExpirationMinutes": 60
  }
}
```

For production:
- Store secrets in Azure Key Vault / AWS Secrets Manager
- Use environment variables
- Don't commit secrets to version control


## Error Handling Philosophy

### Validation Errors (400 Bad Request)
```csharp
throw new ValidationException("Title must be 3-100 characters");
// Converted to 400 by middleware
```

### Not Found Errors (404)
```csharp
throw new NotFoundException("Task not found");
// Converted to 404 by middleware
```

### Authorization Errors (403)
```csharp
throw new ForbiddenException("You cannot access this task");
// Converted to 403 by middleware
```

This centralizes error handling and ensures consistency.


## Async/Await Patterns

All I/O operations use async:
```csharp
public async Task<TaskItemDto> CreateTaskAsync(
    CreateTaskDto request, 
    int userId, 
    CancellationToken cancellationToken = default) // Graceful shutdown support
{
    // ...
    var createdTask = await _taskRepository.AddAsync(task, cancellationToken);
    return MapToDto(createdTask);
}
```

Benefits:
- Threads not blocked waiting for I/O
- Scales to handle more concurrent requests
- Responsive to cancellation signals
- Timeout support via `CancellationToken`


## Performance Considerations

1. **Async I/O**: Prevents thread pool exhaustion
2. **Connection Pooling**: EF Core pools database connections
3. **No N+1 Queries**: Repositories fetch needed data in single query
4. **Single Query per Request**: Services call repositories once per use case
5. **Cancellation Support**: Long operations can be cancelled


## Security Architecture

```
Client Request
    ↓
[JWT Token Validation Middleware]
    ↓ (Extracts User ID from Claims)
[Controller]
    ↓
[Service Layer - Authorization Check]
    ↓
if (task.UserId != userId) throw ForbiddenException();
    ↓
[Repository - Execute Query]
    ↓
Response with Proper HTTP Status Code
```

Key security decisions:
1. Service layer checks permissions (not just controllers)
2. UserId from JWT token, never from request
3. Password hashed with BCrypt (not plain text)
4. JWT validates integrity (not just expiration)
5. HTTPS enforced in production


## Conclusion

This architecture provides:
- ✅ Clear separation of concerns
- ✅ Testable business logic
- ✅ Easy to change/extend
- ✅ Framework-independent domain
- ✅ Professional error handling
- ✅ Security best practices
- ✅ Scalable async patterns
- ✅ SOLID principles throughout

---

## References

- Check [QUICKSTART.md](QUICKSTART.md) for the AI-generated quick start guide
- Check [README.md](README.md) for the full documentation
