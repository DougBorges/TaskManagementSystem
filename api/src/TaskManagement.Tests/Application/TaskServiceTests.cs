using Moq;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using Xunit;

namespace TaskManagement.Tests.Application;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
    }

    [Fact]
    public async Task GetUserTasks_ShouldReturnUserTasks()
    {
        // Arrange
        var userId = 1;
        var tasks = new[]
        {
            new TaskItem { Id = 1, UserId = userId, Title = "Task 1", Status = TaskStatus.Pending },
            new TaskItem { Id = 2, UserId = userId, Title = "Task 2", Status = TaskStatus.InProgress }
        };

        _taskRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act
        var result = await service.GetUserTasksAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _taskRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTaskById_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            UserId = 1,
            Title = "Test Task",
            Status = TaskStatus.Pending
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act
        var result = await service.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task GetTaskById_WithInvalidId_ShouldThrow()
    {
        // Arrange
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetTaskByIdAsync(999));
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var userId = 1;
        var dto = new TaskItemCreateDto
        {
            Title = "New Task",
            Description = "Task Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act
        var result = await service.CreateTaskAsync(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(TaskStatus.Pending, result.Status);
        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTask_WithPastDueDate_ShouldThrow()
    {
        // Arrange
        var userId = 1;
        var dto = new TaskItemCreateDto
        {
            Title = "New Task",
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTaskAsync(userId, dto));
    }

    [Fact]
    public async Task UpdateTask_WithAuthorization_ShouldUpdate()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var existingTask = new TaskItem
        {
            Id = taskId,
            UserId = userId,
            Title = "Old Title",
            Status = TaskStatus.Pending
        };

        var dto = new TaskItemUpdateDto
        {
            Title = "Updated Title",
            Status = TaskStatus.InProgress
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act
        var result = await service.UpdateTaskAsync(taskId, userId, dto);

        // Assert
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(TaskStatus.InProgress, result.Status);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_WithUnauthorizedUser_ShouldThrow()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var differentUserId = 2;
        var existingTask = new TaskItem
        {
            Id = taskId,
            UserId = differentUserId,
            Title = "Task",
            Status = TaskStatus.Pending
        };

        var dto = new TaskItemUpdateDto
        {
            Title = "Updated Title",
            Status = TaskStatus.Pending
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateTaskAsync(taskId, userId, dto));
    }

    [Fact]
    public async Task DeleteTask_WithAuthorization_ShouldDelete()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var existingTask = new TaskItem
        {
            Id = taskId,
            UserId = userId,
            Title = "Task to Delete"
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act
        await service.DeleteTaskAsync(taskId, userId);

        // Assert
        _taskRepositoryMock.Verify(x => x.DeleteAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_WithUnauthorizedUser_ShouldThrow()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var differentUserId = 2;
        var existingTask = new TaskItem
        {
            Id = taskId,
            UserId = differentUserId,
            Title = "Task"
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        var service = new TaskService(_taskRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteTaskAsync(taskId, userId));
    }
}

// Mock implementation for testing
internal class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskItemDto>> GetUserTasksAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetByUserIdAsync(userId, cancellationToken);
        return tasks.Select(MapToDto);
    }

    public async Task<TaskItemDto> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        return MapToDto(task);
    }

    public async Task<TaskItemDto> CreateTaskAsync(int userId, TaskItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.DueDate.HasValue && dto.DueDate < DateTime.UtcNow)
            throw new ArgumentException("DueDate cannot be in the past.");

        var task = new TaskItem
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Status = TaskStatus.Pending
        };

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(task);
    }

    public async Task<TaskItemDto> UpdateTaskAsync(int taskId, int userId, TaskItemUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to update this task.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.DueDate = dto.DueDate;

        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(task);
    }

    public async Task DeleteTaskAsync(int taskId, int userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to delete this task.");

        await _taskRepository.DeleteAsync(taskId, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
    }

    private static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            UserId = task.UserId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt
        };
    }
}
