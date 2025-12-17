using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUsers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (
            IUserRepository users
        ) => await users.GetAllAsync());

        app.MapGet("/users/{id:long}", async (
            IUserRepository users, long id
        ) => await users.GetByIdAsync(id) is User u ? Results.Ok(u) : Results.NotFound());

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