using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface IStationRepository
{
    Task<Station?> GetByIdAsync(long id);
    Task<List<Station>> GetAllAsync();
    Task AddAsync(Station Station);
    Task DeleteAsync(Station Station);
    Task SaveChangesAsync();
}