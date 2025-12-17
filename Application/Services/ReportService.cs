using CourseProject.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using CourseProject.Data;
using CourseProject.Dtos;

namespace CourseProject.Application.Services;

public class ReportService : IReportService
{
    private AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentReportDto> GeneratePaymentReportAsync(
        DateTime? from,
        DateTime? to,
        int topK)
    {
        var query = _db.Payments
            .Include(p => p.Lock!)
                .ThenInclude(l => l.Entry!)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(p => p.DateTime >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.DateTime <= to.Value);

        var payments = await query.ToListAsync();

        var total = payments.Count;
        var successful = payments.Count(p => p.Successful);

        var revenue = payments
            .Where(p => p.Successful && p.Sum.HasValue)
            .Sum(p => p.Sum!.Value);

        var avg = successful == 0
            ? 0
            : payments
                .Where(p => p.Successful && p.Sum.HasValue)
                .Average(p => p.Sum!.Value);

        var topTrains = payments
            .Where(p => p.Lock?.Entry != null)
            .GroupBy(p => p.Lock!.Entry!.TrainId)
            .Select(g => new TopTrainDto
            {
                TrainId = g.Key,
                PaymentsCount = g.Count()
            })
            .OrderByDescending(x => x.PaymentsCount)
            .Take(topK)
            .ToList();

        return new PaymentReportDto
        {
            TotalPayments = total,
            SuccessfulPayments = successful,
            TotalRevenue = revenue,
            AveragePayment = avg,
            TopTrains = topTrains
        };
    }
}
