using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Application.Interfaces.Services;
using CourseProject.Dtos;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            IAuthService auth,
            IUserRepository users,
            RegisterDto dto) =>
        {
            try
            {
                var existing = await users.GetByPhoneAsync(dto.Phone);
                if (existing != null)
                    return Results.BadRequest(new { message = "User already exists" });

                return Results.Ok(await auth.RegisterAsync(dto));
            }
            catch (InvalidOperationException e)
            {
                return Results.Conflict(e.Message);
            }
        });

        app.MapPost("/auth/login", async (
            IAuthService auth,
            LoginDto dto,
            IUserRepository users) =>
        {
            try
            {
                User? user = await users.GetByPhoneAsync(dto.Phone);
                if (user == null)
                {
                    return Results.NotFound(new { message = "user not found" });
                }
                return Results.Ok(await auth.LoginAsync(dto));
            }
            catch
            {
                return Results.Unauthorized();
            }
        });

        
        app.MapPost("/auth/refresh", async (
            IAuthService auth,
            RefreshTokenDto dto) =>
        {
            try
            {
                var (access, refresh) = await auth.RefreshAsync(dto.RefreshToken);
                return Results.Ok(new { accessToken = access, refreshToken = refresh });
            }
            catch
            {
                return Results.Unauthorized();
            }
        });
    }
}
