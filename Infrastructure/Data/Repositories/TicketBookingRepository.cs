using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class TicketBookingRepository : ITicketBookingRepository
{
    private AppDbContext _db;

    public TicketBookingRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<TicketBooking?> GetByIdAsync(long id) =>
        _db.TicketBookings.FindAsync(id).AsTask();

    public Task<List<TicketBooking>> GetAllAsync() =>
        _db.TicketBookings.ToListAsync();
    public Task<List<TicketBooking>> GetInactiveAsync() =>
        _db.TicketBookings.Where(l => l.Paid == false).OrderByDescending(l => l.CreatedAt).ToListAsync();
    public Task<List<TicketBooking>> GetByUserIdAsync(long userId) =>
        _db.TicketBookings.Where(l => l.UserId == userId)
            .Include(l => l.Entry)
            .ThenInclude(e => e.Train)
            .ThenInclude(t => t.Source)
            .Include(l => l.Entry)
            .ThenInclude(e => e.Train)
            .ThenInclude(t => t.Destination)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    public async Task AddAsync(TicketBooking TicketBooking)
    {
        _db.TicketBookings.Add(TicketBooking);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TicketBooking TicketBooking)
    {
        _db.TicketBookings.Remove(TicketBooking);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
