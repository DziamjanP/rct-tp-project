using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(long id);
    Task<List<Ticket>> GetAllAsync();
    Task AddAsync(Ticket ticket);
    Task DeleteAsync(Ticket ticket);
    Task SaveChangesAsync();
}