namespace CourseProject.Models;

public class TicketLock
{
    public long Id { get; set; }
    public long EntryId { get; set; }
    public long UserId { get; set; }
    public string? InvoiceId { get; set; }
    public bool Paid { get; set; }
    public decimal? Sum { get; set; }
    public DateTime CreatedAt { get; set; }

    public TimeTableEntry? Entry { get; set; }
    public User? User { get; set; }
}
