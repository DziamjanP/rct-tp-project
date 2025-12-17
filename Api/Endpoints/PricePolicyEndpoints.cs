using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Models;

namespace CourseProject.Api.Endpoints;

public static class PricePolicyEndpoints
{
    public static void MapPricePolicies(this IEndpointRouteBuilder app)
    {

        app.MapGet("/pricepolicies", async (IPricePolicyRepository pricePolicies) => await pricePolicies.GetAllAsync()).RequireAuthorization("Support");


        app.MapGet("/pricepolicies/{id:long}", async (IPricePolicyRepository pricePolicies, long id) =>
            await pricePolicies.GetByIdAsync(id) is PricePolicy u ? Results.Ok(u) : Results.NotFound()).RequireAuthorization();


        app.MapPost("/pricepolicies", async (IPricePolicyRepository pricePolicies, PricePolicy p) =>
        {
            await pricePolicies.AddAsync(p);
            await pricePolicies.SaveChangesAsync();
            return Results.Created($"/pricepolicies/{p.Id}", p);
        }).RequireAuthorization("Admin");


        app.MapDelete("/pricepolicies/{id:long}", async (IPricePolicyRepository pricePolicies, long id) =>
        {
            var p = await pricePolicies.GetByIdAsync(id);
            if (p == null) return Results.NotFound();

            await pricePolicies.DeleteAsync(p);
            await pricePolicies.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}
