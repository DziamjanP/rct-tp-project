using CourseProject.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using CourseProject.Services;
using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Dtos;
using CourseProject.Data;
using CourseProject.Application.Interfaces.Repositories;

namespace CourseProject.Application.Services;

public class TicketService : ITicketService
{
    private AppDbContext _db;
    private ITicketPricingService _pricing;
    private IPaymentRepository _payments;

    public TicketService(
        AppDbContext db,
        ITicketPricingService pricing,
        IPaymentRepository payments)
    {
        _db = db;
        _pricing = pricing;
        _payments = payments;
    }

    public async Task<TicketBooking> CreateTicketBookingAsync(BuyTicketDto dto, PricePolicy policy, Train train, PerkGroup? selectedPerk)
    {
        var price = _pricing.CalculateTicketPrice(policy, train.SourceId, train.DestinationId, selectedPerk);

        var lockRow = new TicketBooking
        {
            EntryId = dto.EntryId,
            UserId = dto.UserId,
            Paid = false,
            CreatedAt = DateTime.UtcNow,
            InvoiceId = Guid.NewGuid().ToString(),
            Sum = price
        };

        _db.TicketBookings.Add(lockRow);
        await _db.SaveChangesAsync();

        return lockRow;
    }

    public async Task<List<Ticket>> GetTicketsAsync(
        ClaimsPrincipal user,
        bool? admin_list,
        bool? hide_inactive)
    {
        var userId = long.Parse(
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var accessLevel = int.Parse(
            user.FindFirst("access_level")!.Value);

        var query = _db.Tickets
            .Include(t => t.Entry)
                .ThenInclude(e => e.Train)
                    .ThenInclude(t => t.Source)
            .Include(t => t.Entry)
                .ThenInclude(e => e.Train)
                    .ThenInclude(t => t.Destination)
            .AsQueryable();
        
        var adminList = admin_list ?? false;
        var hideInactive = hide_inactive ?? false;

        if (hideInactive)
        {
            query = query
                .Where(t => !t.Used)
                .Where(t => t.Entry!.ArrivalTime > DateTime.UtcNow);
        }

        if (adminList && accessLevel > 0)
            return await query.ToListAsync();

        return await query
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task UseTicketAsync(long ticketId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId)
            ?? throw new KeyNotFoundException();

        ticket.Used = true;
        await _db.SaveChangesAsync();
    }

    public async Task RefundTicketAsync(long ticketId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId)
            ?? throw new KeyNotFoundException();

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task<IResult> PayForBookingsAsync(
        ClaimsPrincipal user,
        List<long> bookingIds)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        var accessLevelClaim = user.FindFirst("access_level");

        if (userIdClaim == null || accessLevelClaim == null)
        {
            return Results.Unauthorized();
        }

        var userId = long.Parse(userIdClaim.Value);
        var accessLevel = int.Parse(accessLevelClaim.Value);

        if (bookingIds == null || bookingIds.Count == 0)
            return Results.BadRequest(new { message = "No booking IDs provided." } );

        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var locks = await _db.TicketBookings
                .Where(l => bookingIds.Contains(l.Id))
                .ToListAsync();

            if (locks.Count != bookingIds.Count)
                return Results.NotFound( new { message = "One or more bookings not found."});

            foreach (var lockEntry in locks)
            {
                if (lockEntry.UserId != userId)
                    return Results.Forbid();

                if (lockEntry.Paid)
                    continue;

                lockEntry.Paid = true;

                await _db.Tickets.AddAsync(new Ticket
                {
                    EntryId = lockEntry.EntryId,
                    UserId = lockEntry.UserId,
                    Used = false
                });

                await _db.Payments.AddAsync(new Payment
                {
                    BookingId = lockEntry.Id,
                    Successful = true, // simulate successful payments
                    Sum = lockEntry.Sum,
                    InvoiceId = lockEntry.InvoiceId,
                    DateTime = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Results.Ok();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
