using CourseProject.Dtos;
using CourseProject.Models;
using System.Security.Claims;

namespace CourseProject.Application.Interfaces.Services;

public interface ITicketService
{
    Task<TicketBooking> CreateTicketBookingAsync(BuyTicketDto dto, PricePolicy policy, Train train, PerkGroup? selectedPerk);

    Task<List<Ticket>> GetTicketsAsync(
        ClaimsPrincipal user,
        bool? admin_list,
        bool? hide_inactive
    );

    Task UseTicketAsync(long ticketId);

    Task RefundTicketAsync(long ticketId);

    Task<IResult> PayForBookingsAsync(
        ClaimsPrincipal user,
        List<long> bookingIds
    );
}
