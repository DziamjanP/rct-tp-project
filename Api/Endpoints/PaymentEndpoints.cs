using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

public static class PaymentEndpoints
{
    public static void MapPayments(this IEndpointRouteBuilder app)
    {

        app.MapGet("/payments", async (IPaymentRepository payments) => await payments.GetAllAsync()).RequireAuthorization();

        app.MapGet("/payments/{id:long}", async (IPaymentRepository payments, long id) =>
            await payments.GetByIdAsync(id) is Payment u ? Results.Ok(u) : Results.NotFound()).RequireAuthorization();

        app.MapDelete("/payments/{id:long}", async (IPaymentRepository payments, long id) =>
        {
            var p = await payments.GetByIdAsync(id);
            if (p == null) return Results.NotFound();

            await payments.DeleteAsync(p);
            await payments.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}