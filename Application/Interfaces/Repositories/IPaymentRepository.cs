using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(long id);
    Task<List<Payment>> GetAllAsync();
    Task AddAsync(Payment payment);
    Task DeleteAsync(Payment payment);
    Task SaveChangesAsync();
}