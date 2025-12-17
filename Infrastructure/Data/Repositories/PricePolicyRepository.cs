using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class PricePolicyRepository : IPricePolicyRepository
{
    private AppDbContext _db;

    public PricePolicyRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<PricePolicy?> GetByIdAsync(long id) =>
        _db.PricePolicies.FindAsync(id).AsTask();

    public Task<List<PricePolicy>> GetAllAsync() =>
        _db.PricePolicies.ToListAsync();

    public async Task AddAsync(PricePolicy perkGroup)
    {
        _db.PricePolicies.Add(perkGroup);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(PricePolicy perkGroup)
    {
        _db.PricePolicies.Remove(perkGroup);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
