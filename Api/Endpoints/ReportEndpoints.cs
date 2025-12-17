using CourseProject.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace CourseProject.Api.Endpoints;

public static class ReportEndpoints
{
    public static void MapReports(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/report")
            .RequireAuthorization("Admin");

        group.MapGet("", async (
            DateTime? from,
            DateTime? to,
            int topK,
            IReportService reportService) =>
        {
            var report = await reportService
                .GeneratePaymentReportAsync(from, to, topK);

            return Results.Ok(report);
        });
    }
}
