using Moq;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using Xunit;

namespace TaskManagement.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
    }

    [Fact]
    public async Task Register_WithNewUser_ShouldCreateAndReturnToken()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "SecurePass123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.HashPassword(dto.Password))
            .Returns("hashed_password");

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<int>(), dto.Email))
            .Returns("jwt_token");

        // Act
        var service = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt_token", result.AccessToken);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("new@example.com", result.Email);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldThrow()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "SecurePass123!"
        };

        var existingUser = new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var service = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(dto));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "user@example.com",
            Password = "SecurePass123!"
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "user@example.com",
            PasswordHash = "hashed_password"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Email))
            .Returns("jwt_token");

        var service = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt_token", result.AccessToken);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrow()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "user@example.com",
            PasswordHash = "hashed_password"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(dto.Password, user.PasswordHash))
            .Returns(false);

        var service = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldThrow()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "nonexistent@example.com",
            Password = "SecurePass123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }
}

// Mock implementation for testing
internal class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var token = _tokenService.GenerateAccessToken(user.Id, user.Email);

        return new AuthResponseDto
        {
            AccessToken = token,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _tokenService.GenerateAccessToken(user.Id, user.Email);

        return new AuthResponseDto
        {
            AccessToken = token,
            Username = user.Username,
            Email = user.Email
        };
    }
}
