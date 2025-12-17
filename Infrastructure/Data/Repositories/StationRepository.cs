using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class StationRepository : IStationRepository
{
    private AppDbContext _db;

    public StationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Station?> GetByIdAsync(long id) =>
        _db.Stations.FindAsync(id).AsTask();

    public Task<List<Station>> GetAllAsync() =>
        _db.Stations.ToListAsync();

    public async Task AddAsync(Station station)
    {
        _db.Stations.Add(station);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Station station)
    {
        _db.Stations.Remove(station);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
