using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface ITrainTypeRepository
{
    Task<TrainType?> GetByIdAsync(long id);
    Task<List<TrainType>> GetAllAsync();
    Task AddAsync(TrainType trainType);
    Task DeleteAsync(TrainType trainType);
    Task SaveChangesAsync();
}