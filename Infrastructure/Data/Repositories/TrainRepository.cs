using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class TrainRepository : ITrainRepository
{
    private AppDbContext _db;

    public TrainRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Train?> GetByIdAsync(long id) =>
        _db.Trains.FindAsync(id).AsTask();

    public Task<List<Train>> GetAllAsync() =>
        _db.Trains.Include(t => t.Source).Include(t => t.Destination).Include(t => t.Type).ToListAsync();

    public async Task AddAsync(Train train)
    {
        _db.Trains.Add(train);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Train train)
    {
        _db.Trains.Remove(train);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
