namespace CourseProject.Models;

public class PricePolicyPerkGroup
{
    public long PricePolicyId { get; set; }
    public long PerkGroupId { get; set; }

    public PricePolicy PricePolicy { get; set; } = null!;
    public PerkGroup PerkGroup { get; set; } = null!;
}
