# Quick Start Guide


## Prerequisites

- .NET 8.0 SDK or later
- SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 or VS Code


## 1. Clone & Setup

```sh
git clone https://github.com/DougBorges/TaskManagementSystem.git
cd TaskManagementSystem
```


## 2. Update Database Configuration

Edit `src/TaskManagement.Api/appsettings.json`:
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


## 3. Create Database

```sh
# Navigate to API project
cd src/TaskManagement.Api

# Apply migrations
dotnet ef database update

# Go back to root
cd ../..
```


## 4. Build Solution

```sh
dotnet build
```

Expected output:
```
Build succeeded. X warnings
```


## 5. Run Tests

```sh
dotnet test
```

Expected output:
```
Passed!  - Failed: 0, Passed: 31, Skipped: 0
```


## 6. Start API

```sh
dotnet run --project src/TaskManagement.Api
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```


## 7. Test API

### Option A: Swagger UI
Open `http://localhost:5000/swagger` in your browser.

### Option B: PowerShell Command Line
**Register**:
```powershell
$body = @{
    username = "testuser"
    email = "test@example.com"
    password = "SecurePass123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

$token = $response.token
Write-Host "Token: $token"
```

**Create Task**:
```powershell
$headers = @{ Authorization = "Bearer $token" }

$body = @{
    title = "My First Task"
    description = "This is a test task"
    dueDate = "2024-12-31"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/tasks" `
    -Method Post `
    -ContentType "application/json" `
    -Headers $headers `
    -Body $body | ConvertTo-Json
```

**Get All Tasks**:
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/tasks" `
    -Method Get `
    -Headers $headers | ConvertTo-Json
```


## 8. Run Specific Tests

```sh
# Auth tests only
dotnet test --filter "AuthenticationServiceTests"

# Task service tests only
dotnet test --filter "TaskServiceTests"

# Tests matching pattern
dotnet test --filter "ClassName=AuthenticationServiceTests|MethodName=LoginAsync_WithValidCredentials_ReturnsAuthResponse"
```

## API Endpoints Summary

| Method | Endpoint | Auth Required | Purpose |
|--------|----------|---------------|---------|
| POST | `/api/auth/register` | ❌ | Create account |
| POST | `/api/auth/login` | ❌ | Get JWT token |
| GET | `/api/tasks` | ✅ | List your tasks |
| GET | `/api/tasks/{id}` | ✅ | Get task detail |
| POST | `/api/tasks` | ✅ | Create task |
| PUT | `/api/tasks/{id}` | ✅ | Update task |
| DELETE | `/api/tasks/{id}` | ✅ | Delete task |


## Example Workflow

### 1. Register
```json
POST /api/auth/register
{
  "username": "john",
  "email": "john@example.com",
  "password": "MySecurePass123"
}

Response 201 Created:
{
  "userId": 1,
  "username": "john",
  "email": "john@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Create Task
```json
POST /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
{
  "title": "Complete project",
  "description": "Finish the task management system",
  "dueDate": "2024-12-31"
}

Response 201 Created:
{
  "id": 1,
  "userId": 1,
  "title": "Complete project",
  "description": "Finish the task management system",
  "status": "Pending",
  "dueDate": "2024-12-31",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### 3. Get All Tasks
```json
GET /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Response 200 OK:
[
  {
    "id": 1,
    "userId": 1,
    "title": "Complete project",
    "status": "Pending",
    ...
  }
]
```

### 4. Update Task
```json
PUT /api/tasks/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
{
  "status": "InProgress"
}

Response 200 OK:
{
  "id": 1,
  "userId": 1,
  "title": "Complete project",
  "status": "InProgress",
  ...
}
```

### 5. Delete Task
```
DELETE /api/tasks/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Response 204 No Content
```


## Project Structure

```
src/
├── TaskManagement.Api/          # REST API endpoints
├── TaskManagement.Application/  # Business logic (services, DTOs)
├── TaskManagement.Domain/       # Domain entities (User, TaskItem)
└── TaskManagement.Infrastructure/ # Data access, security
tests/
└── TaskManagement.Tests/        # Unit tests
```


## Key Files to Review

| File | Purpose |
|------|---------|
| `src/TaskManagement.Api/Program.cs` | Startup configuration & DI |
| `src/TaskManagement.Api/Controllers/AuthController.cs` | Auth endpoints |
| `src/TaskManagement.Api/Controllers/TasksController.cs` | Task endpoints |
| `src/TaskManagement.Application/Services/AuthenticationService.cs` | Auth logic |
| `src/TaskManagement.Application/Services/TaskService.cs` | Task logic |
| `src/TaskManagement.Domain/Entities/User.cs` | User entity |
| `src/TaskManagement.Domain/Entities/TaskItem.cs` | Task entity |
| `tests/TaskManagement.Tests/AuthenticationServiceTests.cs` | Auth tests |
| `tests/TaskManagement.Tests/TaskServiceTests.cs` | Task tests |


## Troubleshooting

### Database Migration Issues
```sh
# Reset migrations (if needed)
dotnet ef database drop --project src/TaskManagement.Infrastructure
dotnet ef database update --project src/TaskManagement.Infrastructure
```

### Port Already in Use
The API runs on port 5000/5001. If that's taken:
```sh
dotnet run --project src/TaskManagement.Api -- --urls "http://localhost:5555"
```

### Tests Fail to Run
```sh
# Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

### JWT Secret Too Short
The secret in appsettings.json must be at least 32 characters for HS256 algorithm.

---

## References

- Check [README.md](README.md) for the full documentation
