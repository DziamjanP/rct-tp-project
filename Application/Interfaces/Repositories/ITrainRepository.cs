using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface ITrainRepository
{
    Task<Train?> GetByIdAsync(long id);
    Task<List<Train>> GetAllAsync();
    Task AddAsync(Train train);
    Task DeleteAsync(Train train);
    Task SaveChangesAsync();
}