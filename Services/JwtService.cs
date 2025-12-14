using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace CourseProject.Services;

public class JwtService
{
    private readonly IConfiguration _cfg;
    public JwtService(IConfiguration cfg) => _cfg = cfg;


    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }


    public string GenerateToken(long userId, int accessLevel, string? name, string? phone)
    {
        var key = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
        var issuer = _cfg["Jwt:Issuer"] ?? "ticket_service_backend";
        var audience = _cfg["Jwt:Audience"] ?? "ticket_service_frontend";
        var expiryMinutes = int.TryParse(_cfg["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("access_level", accessLevel.ToString()),
            new Claim("name", name ?? string.Empty),
        };
        if (!string.IsNullOrEmpty(phone))
            claims.Add(new Claim("phone", phone));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
