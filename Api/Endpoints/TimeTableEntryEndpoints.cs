using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class TimeTableEntryEndpoints
{
    public static void MapTimeTableEntries(this IEndpointRouteBuilder app)
    {

        app.MapGet("/timetable", async (
            ITimeTableEntryRepository timeTableEntries,
            DateTime? after
        ) =>
        {
            if (after.HasValue)
            {
                return await timeTableEntries.GetAfterAsync(after.Value);
            }
            else
            {
                return await timeTableEntries.GetAllAsync();   
            }
        });


        app.MapGet("/timetable/{id:long}", async (
            ITimeTableEntryRepository timeTableEntries,
            long id) => await timeTableEntries.GetByIdAsync(id)).RequireAuthorization();


        app.MapPost("/timetable", async (
            ITimeTableEntryRepository timeTableEntries,
            TimeTableEntry entry
        ) =>
        {
            await timeTableEntries.AddAsync(entry);
            await timeTableEntries.SaveChangesAsync();
            return Results.Created($"/timetable/{entry.Id}", entry);
        }).RequireAuthorization("Admin");


        app.MapDelete("/timetable/{id:long}", async (
            ITimeTableEntryRepository timeTableEntries,
            long id
        ) =>
        {
            var e = await timeTableEntries.GetByIdAsync(id);
            if (e == null) return Results.NotFound();

            await timeTableEntries.DeleteAsync(e);
            await timeTableEntries.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        
        app.MapGet("/timetable/search", async (
            ITimeTableEntryRepository timeTableEntries,
            long? fromStationId,
            long? toStationId,
            DateTime? after,
            DateTime? before) => Results.Ok(await timeTableEntries.SearchAsync(fromStationId, toStationId, after, before)));
    }
}
