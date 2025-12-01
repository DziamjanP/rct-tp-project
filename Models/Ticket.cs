namespace CourseProject.Models;

public class Ticket
{
    public long Id { get; set; }
    public long EntryId { get; set; }
    public long UserId { get; set; }
    public bool Used { get; set; }

    public TimeTableEntry? Entry { get; set; }
    public User? User { get; set; }
}
