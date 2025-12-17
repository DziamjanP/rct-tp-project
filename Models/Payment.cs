using System;

namespace CourseProject.Models;

public class Payment
{
    public long Id { get; set; }
    public long BookingId { get; set; }
    public required string InvoiceId { get; set; } = null!;
    public bool Successful { get; set; }
    public DateTime DateTime { get; set; }
    public decimal? Sum { get; set; }

    public TicketBooking Lock { get; set; } = null!;
}
