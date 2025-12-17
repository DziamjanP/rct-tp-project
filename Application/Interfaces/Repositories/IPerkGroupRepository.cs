using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface IPerkGroupRepository
{
    Task<PerkGroup?> GetByIdAsync(long id);
    Task<List<PerkGroup>> GetAllAsync();
    Task AddAsync(PerkGroup perkGroup);
    Task DeleteAsync(PerkGroup perkGroup);
    Task SaveChangesAsync();
}