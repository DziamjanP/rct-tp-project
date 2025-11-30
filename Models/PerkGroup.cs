namespace CourseProject.Models;

using System.Text.Json.Serialization;

public class PerkGroup
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? FixedPrice { get; set; }
    public decimal? Discount { get; set; }

    // normalized M:N mapping
    [JsonIgnore]
    public ICollection<PricePolicyPerkGroup>? PricePolicies { get; set; }
}
