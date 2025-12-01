using System;
using System.Text.Json;

namespace CourseProject.Models;

public class TimeTableEntry
{
    public long Id { get; set; }
    public long TrainId { get; set; }
    public DateTime Departure { get; set; }
    public DateTime Arrival { get; set; }

    // temporary, will add M:N in future
    public JsonDocument? StopInfo { get; set; }

    public long? PricePolicyId { get; set; }

    public Train? Train { get; set; }
    public PricePolicy? PricePolicy { get; set; }
}
