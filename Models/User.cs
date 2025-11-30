using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CourseProject.Models;

public class User
{
    public long Id { get; set; }
    public string Password { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Passport { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    [JsonIgnore]
    public ICollection<Ticket>? Tickets { get; set; }
    [JsonIgnore]
    public ICollection<TicketLock>? TicketLocks { get; set; }
}
