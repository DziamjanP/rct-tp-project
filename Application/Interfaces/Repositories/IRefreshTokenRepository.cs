using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetValidAsync(string token);
    Task RevokeAsync(RefreshToken token);
    Task SaveChangesAsync();
}
