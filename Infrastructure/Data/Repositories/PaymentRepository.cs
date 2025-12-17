using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Infrastructure.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private AppDbContext _db;

    public PaymentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Payment?> GetByIdAsync(long id) =>
        _db.Payments.FindAsync(id).AsTask();

    public Task<List<Payment>> GetAllAsync() =>
        _db.Payments.ToListAsync();

    public async Task AddAsync(Payment payment)
    {
        _db.Payments.Add(payment);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Payment payment)
    {
        _db.Payments.Remove(payment);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
