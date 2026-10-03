using System.Text.RegularExpressions;
using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;

namespace DigitalMemoryMap.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        ValidatePasswordComplexity(dto.Password);

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = 1, // User
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var savedUser = await _userRepository.AddAsync(user);
        var token = _tokenService.GenerateToken(savedUser);

        return new AuthResponseDto
        {
            UserId = savedUser.UserId,
            FullName = savedUser.FullName,
            Email = savedUser.Email,
            Role = "User",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Token = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("Account is deactivated.");
        }

        var token = _tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "User",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Token = token
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationException("CurrentPassword", "Current password is incorrect.");
        }

        ValidatePasswordComplexity(dto.NewPassword);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return new UserProfileDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "User",
            Bio = user.Bio,
            ProfilePhotoPath = user.ProfilePhotoPath,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.FullName = dto.FullName.Trim();
        user.Bio = dto.Bio?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return new UserProfileDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "User",
            Bio = user.Bio,
            ProfilePhotoPath = user.ProfilePhotoPath,
            CreatedAt = user.CreatedAt
        };
    }

    private static void ValidatePasswordComplexity(string password)
    {
        if (password.Length < 8 || password.Length > 64)
        {
            throw new ValidationException("Password", "Password must be between 8 and 64 characters.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            throw new ValidationException("Password", "Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            throw new ValidationException("Password", "Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            throw new ValidationException("Password", "Password must contain at least one digit.");
        }
    }
}
