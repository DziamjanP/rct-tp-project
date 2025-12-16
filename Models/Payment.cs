using System;

namespace CourseProject.Models;

public class Payment
{
    public long Id { get; set; }
    public long LockId { get; set; }
    public required string InvoiceId { get; set; } = null!;
    public bool Successful { get; set; }
    public DateTime DateTime { get; set; }
    public decimal? Sum { get; set; }

    public TicketLock Lock { get; set; } = null!;
}
