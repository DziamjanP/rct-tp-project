using System.Security.Claims;
using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Dtos;
using CourseProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUsers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (
            IUserRepository users
        ) => await users.GetAllAsync());

        app.MapGet("/users/{id:long}", async (
            IUserRepository users,
            ClaimsPrincipal userClaims,
            long id
        ) =>
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier);
            var accessLevelClaim = userClaims.FindFirst("access_level");

            if (userIdClaim == null || accessLevelClaim == null)
                return Results.Unauthorized();

            var userId = long.Parse(userIdClaim.Value);
            var accessLevel = int.Parse(accessLevelClaim.Value);
            if (userId == id || accessLevel > 0)
            {
                var user = await users.GetByIdAsync(id);
                if (user == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new UserResponseDto(
                    Id: user.Id,
                    Name: user.Name,
                    Surname: user.Surname,
                    Passport: user.Passport,
                    Phone: user.Phone,
                    AccessLevel: user.AccessLevel
                ));
            }
            else
            {
                return Results.Unauthorized();
            }

            
        });

        app.MapPut("/users/{id:long}", async (
            IUserRepository users,
            ClaimsPrincipal user,
            long id,
            [FromBody] UserUpdateDto dto
        ) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var accessLevelClaim = user.FindFirst("access_level");

            if (userIdClaim == null || accessLevelClaim == null)
                return Results.Unauthorized();

            var userId = long.Parse(userIdClaim.Value);
            var accessLevel = int.Parse(accessLevelClaim.Value);
            if (userId == id || accessLevel > 0)
            {
                await users.UpdateAsync(userId, dto);
            }
            else
            {
                return Results.Unauthorized();
            }
            return Results.Ok();
        });

        app.MapDelete("/users/{id:long}", async (IUserRepository users, long id) =>
        {
            var u = await users.GetByIdAsync(id);
            if (u == null) return Results.NotFound();

            await users.DeleteAsync(u);
            await users.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}