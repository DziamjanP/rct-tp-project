using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Dtos;
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
    public async Task UpdateAsync(long userId, UserUpdateDto userDto)
    {
        var user = _db.Users.SingleOrDefault(u => u.Id == userId);
        if (user != null)
        {
            if (userDto.Name != null)
            {
                user.Name = userDto.Name;
            }
            if (userDto.Surname != null)
            {
                user.Surname = userDto.Surname;
            }
            if (userDto.Phone != null)
            {
                user.Phone = userDto.Phone;
            }
            if (userDto.Passport != null)
            {
                user.Passport = userDto.Passport;
            }
            if (userDto.AccessLevel != null)
            {
                user.AccessLevel = userDto.AccessLevel ?? 0;
            }
            _db.Users.Update(user);
            _db.SaveChanges();
        }
    }

    public async Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
