using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class StationEndpoints
{
    public static void MapStations(this IEndpointRouteBuilder app)
    {

        app.MapGet("/stations", async (IStationRepository stations) => await stations.GetAllAsync());


        app.MapGet("/stations/{id:long}", async (IStationRepository stations, long id) =>
            await stations.GetByIdAsync(id) is Station u ? Results.Ok(u) : Results.NotFound());

        app.MapPost("/stations", async (IStationRepository stations, Station s) =>
        {
            await stations.AddAsync(s);
            await stations.SaveChangesAsync();
            return Results.Created($"/stations/{s.Id}", s);
        }).RequireAuthorization("Admin");


        app.MapDelete("/stations/{id:long}", async (IStationRepository stations, long id) =>
        {

            var s = await stations.GetByIdAsync(id);
            if (s == null) return Results.NotFound();

            await stations.DeleteAsync(s);
            await stations.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}