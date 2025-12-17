using CourseProject.Models;

public interface IPricePolicyRepository
{
    Task<PricePolicy?> GetByIdAsync(long id);
    Task<List<PricePolicy>> GetAllAsync();
    Task AddAsync(PricePolicy PricePolicy);
    Task DeleteAsync(PricePolicy PricePolicy);
    Task SaveChangesAsync();
}
