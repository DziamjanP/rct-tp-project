using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface ITicketBookingRepository
{
    Task<TicketBooking?> GetByIdAsync(long id);
    Task<List<TicketBooking>> GetAllAsync();
    Task<List<TicketBooking>> GetInactiveAsync();
    Task<List<TicketBooking>> GetByUserIdAsync(long userId);
    Task AddAsync(TicketBooking ticketBooking);
    Task DeleteAsync(TicketBooking ticketBooking);
    Task SaveChangesAsync();
}