using System;

namespace CourseProject.Models;

public class Payment
{
    public long Id { get; set; }
    public long LockId { get; set; }
    public string? InvoiceId { get; set; }
    public bool Successful { get; set; }
    public DateTime DateTime { get; set; }
    public decimal? Sum { get; set; }

    public TicketLock? Lock { get; set; }
}
