using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class TrainTypeEndpoints
{
    public static void MapTrainTypes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/traintypes", async (ITrainTypeRepository trainTypes) => await trainTypes.GetAllAsync());

        app.MapGet("/traintypes/{id:long}", async (ITrainTypeRepository trainTypes, long id) =>
            await trainTypes.GetByIdAsync(id) is TrainType u ? Results.Ok(u) : Results.NotFound());

        app.MapPost("/traintypes", async (ITrainTypeRepository trainTypes, TrainType t) =>
        {
            await trainTypes.AddAsync(t);
            await trainTypes.SaveChangesAsync();
            return Results.Created($"/traintypes/{t.Id}", t);
        }).RequireAuthorization("Admin");

        app.MapDelete("/traintypes/{id:long}", async (ITrainTypeRepository trainTypes, long id) =>
        {
            var t = await trainTypes.GetByIdAsync(id);
            if (t == null) return Results.NotFound();

            await trainTypes.DeleteAsync(t);
            await trainTypes.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}