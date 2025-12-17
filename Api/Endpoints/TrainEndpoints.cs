using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class TrainEndpoints
{
    public static void MapTrains(this IEndpointRouteBuilder app)
    {

        app.MapGet("/trains", async (ITrainRepository trains) => await trains.GetAllAsync());


        app.MapGet("/trains/{id:long}", async (ITrainRepository trains, long id) =>
            await trains.GetByIdAsync(id) is Train u ? Results.Ok(u) : Results.NotFound());


        app.MapPost("/trains", async (ITrainRepository trains, Train t) =>
        {
            await trains.AddAsync(t);
            await trains.SaveChangesAsync();
            return Results.Created($"/trains/{t.Id}", t);
        }).RequireAuthorization("Admin");


        app.MapDelete("/trains/{id:long}", async (ITrainRepository trains, long id) =>
        {
            var t = await trains.GetByIdAsync(id);
            if (t == null) return Results.NotFound();

            await trains.DeleteAsync(t);
            await trains.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");

    }
}