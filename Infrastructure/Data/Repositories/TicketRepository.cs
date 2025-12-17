using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class TicketRepository : ITicketRepository
{
    private AppDbContext _db;

    public TicketRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Ticket?> GetByIdAsync(long id) =>
        _db.Tickets.FindAsync(id).AsTask();

    public Task<List<Ticket>> GetAllAsync() =>
        _db.Tickets.ToListAsync();

    public async Task AddAsync(Ticket ticket)
    {
        _db.Tickets.Add(ticket);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Ticket ticket)
    {
        _db.Tickets.Remove(ticket);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
