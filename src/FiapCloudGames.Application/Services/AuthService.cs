using FiapCloudGames.Application.DTOs.Auth;
using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Application.Validators;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Enums;
using FiapCloudGames.Domain.Exceptions;
using FiapCloudGames.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IAuditLogger _audit;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IAuditLogger audit,
        ILogger<AuthService> logger)
    {
        _users = users;
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
        _audit = audit;
        _logger = logger;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var validation = UserValidator.ValidateRegistration(request.Name, request.Email, request.Password);
        if (!validation.IsValid)
            throw new DomainException(string.Join(" ", validation.Errors));

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(email, ct))
            throw new ConflictException("Email is already in use.");

        var user = new User(request.Name.Trim(), email, _hasher.Hash(request.Password));
        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User registered: {Email}", email);
        await _audit.LogAsync("UserRegistered", user.Id, $"User {email} registered.", new { user.Id, user.Email }, ct);

        return new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            throw new UnauthorizedException("Invalid credentials.");

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var (token, expires) = _jwt.Generate(user);
        _logger.LogInformation("User logged in: {Email}", user.Email);
        await _audit.LogAsync("UserLoggedIn", user.Id, $"User {user.Email} logged in.", null, ct);

        return new LoginResponse(token, expires, user.Email, user.Role.ToString());
    }
}
