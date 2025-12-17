using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByPhoneAsync(string phone);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task DeleteAsync(User user);
    Task SaveChangesAsync();
}