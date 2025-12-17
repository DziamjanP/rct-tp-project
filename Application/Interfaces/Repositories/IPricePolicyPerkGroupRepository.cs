using CourseProject.Models;

public interface IPricePolicyPerkGroupRepository
{
    Task<bool> ExistsAsync(long policyId, long perkGroupId);
    Task AddAsync(long policyId, long perkGroupId);
    Task RemoveAsync(long policyId, long perkGroupId);
    Task<List<PerkGroup>> GetPerksForPolicyAsync(long policyId);
    Task<List<PricePolicy>> GetPoliciesForPerkAsync(long perkGroupId);
    Task SaveChangesAsync();
}
