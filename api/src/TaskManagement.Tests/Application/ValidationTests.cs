using TaskManagement.Application.Dtos;
using TaskManagement.Application.Exceptions;
using Xunit;

namespace TaskManagement.Tests.Application;

public class ValidationTests
{
    [Fact]
    public void ValidateUserRegister_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        // Act & Assert - should not throw
        ValidateUserRegister(dto);
    }

    [Fact]
    public void ValidateUserRegister_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "SecurePass123!"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateUserRegister(dto));
    }

    [Fact]
    public void ValidateUserRegister_WithWeakPassword_ShouldFail()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "weak"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateUserRegister(dto));
    }

    [Fact]
    public void ValidateUserRegister_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "",
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateUserRegister(dto));
    }

    [Fact]
    public void ValidateTaskCreate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new TaskItemCreateDto
        {
            Title = "Valid Title",
            Description = "Some description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert - should not throw
        ValidateTaskCreate(dto);
    }

    [Fact]
    public void ValidateTaskCreate_WithTitleTooShort_ShouldFail()
    {
        // Arrange
        var dto = new TaskItemCreateDto
        {
            Title = "AB",
            Description = "Some description"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateTaskCreate(dto));
    }

    [Fact]
    public void ValidateTaskCreate_WithTitleTooLong_ShouldFail()
    {
        // Arrange
        var dto = new TaskItemCreateDto
        {
            Title = new string('A', 101),
            Description = "Some description"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateTaskCreate(dto));
    }

    [Fact]
    public void ValidateTaskCreate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var dto = new TaskItemCreateDto
        {
            Title = "Valid Title",
            Description = new string('A', 1001)
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateTaskCreate(dto));
    }

    [Fact]
    public void ValidateTaskCreate_WithPastDueDate_ShouldFail()
    {
        // Arrange
        var dto = new TaskItemCreateDto
        {
            Title = "Valid Title",
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateTaskCreate(dto));
    }

    [Fact]
    public void ValidateTaskCreate_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var dto = new TaskItemCreateDto
        {
            Title = "",
            Description = "Some description"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateTaskCreate(dto));
    }

    private static void ValidateUserRegister(UserRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationException("Username is required.");

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            throw new ValidationException("Email must be valid.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            throw new ValidationException("Password must be at least 8 characters.");
    }

    private static void ValidateTaskCreate(TaskItemCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Title is required.");

        if (dto.Title.Length < 3)
            throw new ValidationException("Title must be at least 3 characters.");

        if (dto.Title.Length > 100)
            throw new ValidationException("Title cannot exceed 100 characters.");

        if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 1000)
            throw new ValidationException("Description cannot exceed 1000 characters.");

        if (dto.DueDate.HasValue && dto.DueDate < DateTime.UtcNow)
            throw new ValidationException("DueDate cannot be in the past.");
    }
}
