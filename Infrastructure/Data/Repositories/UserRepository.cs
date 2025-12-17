using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(long id) =>
        _db.Users.FindAsync(id).AsTask();

    public Task<User?> GetByPhoneAsync(string phone) =>
        _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);

    public Task<List<User>> GetAllAsync() =>
        _db.Users.ToListAsync();

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
