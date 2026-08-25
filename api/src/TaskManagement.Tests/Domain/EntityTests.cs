using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using Xunit;

namespace TaskManagement.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hashedpassword", user.PasswordHash);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public void User_HasTasksNavigationProperty()
    {
        // Arrange & Act
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        // Assert
        Assert.NotNull(user.Tasks);
        Assert.Empty(user.Tasks);
    }
}

public class TaskItemEntityTests
{
    [Fact]
    public void CreateTaskItem_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Id = 1,
            UserId = 1,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        Assert.Equal(1, task.Id);
        Assert.Equal(1, task.UserId);
        Assert.Equal("Test Task", task.Title);
        Assert.Equal("Test Description", task.Description);
        Assert.Equal(TaskStatus.Pending, task.Status);
        Assert.NotNull(task.DueDate);
    }

    [Fact]
    public void CreateTaskItem_DefaultStatusIsPending()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            UserId = 1,
            Title = "Test Task"
        };

        // Assert
        Assert.Equal(TaskStatus.Pending, task.Status);
    }

    [Fact]
    public void CreateTaskItem_WithNullDescription_ShouldSucceed()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            UserId = 1,
            Title = "Test Task",
            Description = null
        };

        // Assert
        Assert.Null(task.Description);
    }

    [Fact]
    public void TaskItem_CanChangeStatus()
    {
        // Arrange
        var task = new TaskItem
        {
            UserId = 1,
            Title = "Test Task",
            Status = TaskStatus.Pending
        };

        // Act
        task.Status = TaskStatus.InProgress;

        // Assert
        Assert.Equal(TaskStatus.InProgress, task.Status);
    }
}

public class TaskStatusEnumTests
{
    [Fact]
    public void TaskStatus_HasAllRequiredValues()
    {
        // Assert
        Assert.Equal(0, (int)TaskStatus.Pending);
        Assert.Equal(1, (int)TaskStatus.InProgress);
        Assert.Equal(2, (int)TaskStatus.Completed);
    }
}
