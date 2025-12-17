using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Application.Interfaces.Services;
using CourseProject.Dtos;
using CourseProject.Models;
using CourseProject.Services;

public class AuthService : IAuthService
{
    private IUserRepository _users;
    private IRefreshTokenRepository _refreshTokens;
    private JwtService _jwt;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        JwtService jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

        var user = new User
        {
            Phone = dto.Phone,
            Name = dto.Name,
            Surname = dto.Surname,
            Password = hash,
            Salt = salt,
            AccessLevel = 0
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        var accessToken = _jwt.GenerateToken(user.Id, user.AccessLevel, user.Name, user.Phone);
        var refreshToken = _jwt.GenerateRefreshToken();

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });

        await _refreshTokens.SaveChangesAsync();

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            UserId: user.Id,
            AccessLevel: user.AccessLevel,
            Name: user.Name,
            Surname: user.Surname,
            Passport: user.Passport,
            Phone: user.Phone
        );
    }


    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByPhoneAsync(dto.Phone)
            ?? throw new UnauthorizedAccessException();

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            throw new UnauthorizedAccessException();

        var accessToken = _jwt.GenerateToken(user.Id, user.AccessLevel, user.Name, user.Phone);
        var refreshToken = _jwt.GenerateRefreshToken();

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _refreshTokens.SaveChangesAsync();

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            UserId: user.Id,
            AccessLevel: user.AccessLevel,
            Name: user.Name,
            Surname: user.Surname,
            Passport: user.Passport,
            Phone: user.Phone
        );
    }

    public async Task<(string, string)> RefreshAsync(string refreshToken)
    {
        var tokenRow = await _refreshTokens.GetValidAsync(refreshToken)
            ?? throw new UnauthorizedAccessException();

        tokenRow.Revoked = true;

        var newAccess = _jwt.GenerateToken(
            tokenRow.User.Id,
            tokenRow.User.AccessLevel,
            tokenRow.User.Name,
            tokenRow.User.Phone);

        var newRefresh = _jwt.GenerateRefreshToken();

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = tokenRow.UserId,
            Token = newRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _refreshTokens.SaveChangesAsync();

        return (newAccess, newRefresh);
    }
}
