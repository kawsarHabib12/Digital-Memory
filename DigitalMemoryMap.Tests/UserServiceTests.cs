using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.BLL.Services;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Moq;
using Xunit;

namespace DigitalMemoryMap.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(_userRepoMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Register_ValidInput_ReturnsSuccessAndAuthToken()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            FullName = "Alice Johnson",
            Email = "alice@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.UserId = 1; return u; });
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("mock_token");

        // Act
        var result = await _userService.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal("mock_token", result.Token);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            FullName = "Alice",
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new User { UserId = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _userService.RegisterAsync(dto));
    }

    [Fact]
    public async Task Register_WeakPassword_ThrowsValidationException()
    {
        // Arrange: Missing uppercase and digit
        var dto = new RegisterRequestDto
        {
            FullName = "Alice",
            Email = "alice@example.com",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _userService.RegisterAsync(dto));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            UserId = 1,
            FullName = "Alice",
            Email = "alice@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            Role = new Role { RoleName = "User" }
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("alice@example.com")).ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("valid_token");

        // Act
        var result = await _userService.LoginAsync(new LoginRequestDto { Email = "alice@example.com", Password = password });

        // Assert
        Assert.Equal("valid_token", result.Token);
        Assert.Equal("Alice", result.FullName);
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "alice@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!"),
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("alice@example.com")).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _userService.LoginAsync(new LoginRequestDto { Email = "alice@example.com", Password = "WrongPassword1!" }));
    }

    [Fact]
    public async Task Login_DeactivatedUser_ThrowsForbiddenException()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            UserId = 1,
            Email = "banned@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = false
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("banned@example.com")).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _userService.LoginAsync(new LoginRequestDto { Email = "banned@example.com", Password = password }));
    }
}
