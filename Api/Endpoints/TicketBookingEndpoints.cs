using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;
using System.Security.Claims;

namespace CourseProject.Api.Endpoints;

public static class TicketBookingEndpoints
{
    public static void MapTicketBookings(this IEndpointRouteBuilder app)
    {
        app.MapGet("/ticketlocks", async (
            ClaimsPrincipal user,
            ITicketBookingRepository ticketBookings,
            bool? admin_list = false,
            bool? hide_inactive = false
        ) => {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var accessLevelClaim = user.FindFirst("access_level");

            if (userIdClaim == null || accessLevelClaim == null)
                return Results.Unauthorized();

            var userId = long.Parse(userIdClaim.Value);
            var accessLevel = int.Parse(accessLevelClaim.Value);

            var adminList = admin_list ?? false;
            var hideInactive = hide_inactive ?? false;

            if (adminList && accessLevel > 0)
            {
                if (hideInactive)
                    return Results.Ok(await ticketBookings.GetInactiveAsync());    
                return Results.Ok(
                    await ticketBookings.GetAllAsync()
                );
            }

            return Results.Ok(
                await ticketBookings.GetByUserIdAsync(userId)
            );
        }).RequireAuthorization();


        app.MapGet("/ticketlocks/{id:long}", async (
            ITicketBookingRepository ticketBookings,
            long id
        ) =>
            await ticketBookings.GetByIdAsync(id) is TicketBooking u ? Results.Ok(u) : Results.NotFound()).RequireAuthorization();

        app.MapDelete("/ticketlocks/{id:long}", async (ITicketBookingRepository ticketBookings, long id) =>
        {
            var l = await ticketBookings.GetByIdAsync(id);
            if (l == null) return Results.NotFound();

            await ticketBookings.DeleteAsync(l);
            await ticketBookings.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}