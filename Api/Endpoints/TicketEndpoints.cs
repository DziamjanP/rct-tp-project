using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Application.Interfaces.Services;
using CourseProject.Dtos;
using CourseProject.Models;
using CourseProject.Services;
using System.Security.Claims;

namespace CourseProject.Api.Endpoints;

public static class TicketEndpoints
{
    public static void MapTickets(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization("User");

        group.MapGet("/price", async (
            long entryId,
            long? perkGroupId,
            ITrainRepository trains,
            IPerkGroupRepository perkGroups,
            IPricePolicyRepository pricePolicies,
            ITimeTableEntryRepository timeTableEntries,
            IPricePolicyPerkGroupRepository pricePolicyPerkGroups,
            ITicketService ticketService,
            ITicketPricingService pricing) =>
        {
            var entry = await timeTableEntries.GetByIdAsync(entryId);
            if (entry == null) return Results.BadRequest();

            var policy = await pricePolicies.GetByIdAsync(entry.PricePolicyId);

            if (policy == null)
            {
                return Results.BadRequest("bad privacy policy for ticket, contact support");
            }

            PerkGroup? selectedPerk = null;
            if (perkGroupId != null)
            {
                selectedPerk = await perkGroups.GetByIdAsync(perkGroupId ?? 0);
                if (selectedPerk == null) return Results.BadRequest();
                PerkGroup? selectedPolicyPerk = null;
                if (await pricePolicyPerkGroups.ExistsAsync(policy.Id, perkGroupId ?? 0))
                {
                    selectedPolicyPerk = await perkGroups.GetByIdAsync(perkGroupId ?? 0);
                }
                if (selectedPolicyPerk == null) selectedPerk = null;
            }

            var train = entry.Train;
            train = await trains.GetByIdAsync(entry.TrainId);
            if (train == null)
            {
                return Results.BadRequest("bad train for ticket, contact support");
            }

            var price = pricing
                .CalculateTicketPrice(policy, train.SourceId, train.DestinationId, selectedPerk);

            return Results.Ok(new PriceEstimateResult(price));
        });

        group.MapPost("/buy", async (
            BuyTicketDto dto,
            ITrainRepository trains,
            IPerkGroupRepository perkGroups,
            IPricePolicyRepository pricePolicies,
            ITimeTableEntryRepository timeTableEntries,
            IPricePolicyPerkGroupRepository pricePolicyPerkGroups,
            ITicketService ticketService) =>
        {
            var entry = await timeTableEntries.GetByIdAsync(dto.EntryId);
            if (entry == null) return Results.BadRequest();

            var policy = await pricePolicies.GetByIdAsync(entry.PricePolicyId);

            if (policy == null)
            {
                return Results.BadRequest("bad privacy policy for ticket, contact support");
            }

            PerkGroup? selectedPerk = null;
            if (dto.PerkGroupId != null)
            {
                selectedPerk = await perkGroups.GetByIdAsync(dto.PerkGroupId ?? 0);
                if (selectedPerk == null) return Results.BadRequest();
                PerkGroup? selectedPolicyPerk = null;
                if (await pricePolicyPerkGroups.ExistsAsync(policy.Id, dto.PerkGroupId ?? 0))
                {
                    selectedPolicyPerk = await perkGroups.GetByIdAsync(dto.PerkGroupId ?? 0);
                }
                if (selectedPolicyPerk == null) selectedPerk = null;
            }

            var train = entry.Train;
            train = await trains.GetByIdAsync(entry.TrainId);
            if (train == null)
            {
                return Results.BadRequest("bad train for ticket, contact support");
            }

            var result = await ticketService.CreateTicketBookingAsync(dto, policy, train, selectedPerk);
            return Results.Ok(result);
        });

        group.MapGet("/tickets", async (
            ClaimsPrincipal user,
            bool? admin_list,
            bool? hide_inactive,
            ITicketService ticketService) =>
        {
            var tickets = await ticketService.GetTicketsAsync(
                user,
                admin_list ?? false,
                hide_inactive ?? false);

            return Results.Ok(tickets);
        });

        group.MapPost("/tickets/use/{id:long}", async (
            long id,
            ITicketService ticketService) =>
        {
            await ticketService.UseTicketAsync(id);
            return Results.Ok();
        })
        .RequireAuthorization("Inspector");

        group.MapPost("/tickets/refund/{id:long}", async (
            long id,
            ITicketService ticketService) =>
        {
            await ticketService.RefundTicketAsync(id);
            return Results.Ok();
        })
        .RequireAuthorization("Support");

        group.MapPost("/pay", async (
            PayBookingsRequest dto,
            ClaimsPrincipal user,
            ITicketService ticketService) =>
        {
            await ticketService.PayForBookingsAsync(
                user,
                dto.BookingIds
            );

            return Results.Ok();
        });
    }
}
