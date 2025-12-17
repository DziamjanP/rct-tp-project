using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class TrainTypeRepository : ITrainTypeRepository
{
    private AppDbContext _db;

    public TrainTypeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<TrainType?> GetByIdAsync(long id) =>
        _db.TrainTypes.FindAsync(id).AsTask();

    public Task<List<TrainType>> GetAllAsync() =>
        _db.TrainTypes.ToListAsync();

    public async Task AddAsync(TrainType trainType)
    {
        _db.TrainTypes.Add(trainType);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TrainType trainType)
    {
        _db.TrainTypes.Remove(trainType);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
