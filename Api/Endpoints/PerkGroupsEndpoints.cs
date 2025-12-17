using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class PerkGroupEndpoints
{
    public static void MapPerkGroups(this IEndpointRouteBuilder app)
    {
        app.MapGet("/perkgroups", async (IPerkGroupRepository perkGroups) => await perkGroups.GetAllAsync());


        app.MapGet("/perkgroups/{id:long}", async (IPerkGroupRepository perkGroups, long id) =>
            await perkGroups.GetByIdAsync(id) is PerkGroup u ? Results.Ok(u) : Results.NotFound());


        app.MapPost("/perkgroups", async (IPerkGroupRepository perkGroups, PerkGroup pg) =>
        {
            await perkGroups.AddAsync(pg);
            await perkGroups.SaveChangesAsync();
            return Results.Created($"/perkgroups/{pg.Id}", pg);
        }).RequireAuthorization("Admin");


        app.MapDelete("/perkgroups/{id:long}", async (IPerkGroupRepository perkGroups, long id) =>
        {
            var pg = await perkGroups.GetByIdAsync(id);
            if (pg == null) return Results.NotFound();

            await perkGroups.DeleteAsync(pg);
            await perkGroups.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}
