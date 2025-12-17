using CourseProject.Dtos;

namespace CourseProject.Application.Interfaces.Services;

public interface IReportService
{
    Task<PaymentReportDto> GeneratePaymentReportAsync(
        DateTime? from,
        DateTime? to,
        int topK
    );
}
