using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class PerkGroupRepository : IPerkGroupRepository
{
    private AppDbContext _db;

    public PerkGroupRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<PerkGroup?> GetByIdAsync(long id) =>
        _db.PerkGroups.FindAsync(id).AsTask();

    public Task<List<PerkGroup>> GetAllAsync() =>
        _db.PerkGroups.ToListAsync();

    public async Task AddAsync(PerkGroup perkGroup)
    {
        _db.PerkGroups.Add(perkGroup);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(PerkGroup perkGroup)
    {
        _db.PerkGroups.Remove(perkGroup);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
