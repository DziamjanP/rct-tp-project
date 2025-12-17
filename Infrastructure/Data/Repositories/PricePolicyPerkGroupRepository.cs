using CourseProject.Application.Interfaces.Repositories;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

public class PricePolicyPerkGroupRepository : IPricePolicyPerkGroupRepository
{
    private AppDbContext _db;

    public PricePolicyPerkGroupRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(long policyId, long perkGroupId)
    {
        return _db.PricePolicyPerkGroups
            .AnyAsync(x =>
                x.PricePolicyId == policyId &&
                x.PerkGroupId == perkGroupId);
    }

    public async Task AddAsync(long policyId, long perkGroupId)
    {
        if (await ExistsAsync(policyId, perkGroupId))
            return;

        _db.PricePolicyPerkGroups.Add(new PricePolicyPerkGroup
        {
            PricePolicyId = policyId,
            PerkGroupId = perkGroupId
        });
    }

    public async Task RemoveAsync(long policyId, long perkGroupId)
    {
        var entity = await _db.PricePolicyPerkGroups
            .FirstOrDefaultAsync(x =>
                x.PricePolicyId == policyId &&
                x.PerkGroupId == perkGroupId);

        if (entity != null)
            _db.PricePolicyPerkGroups.Remove(entity);
    }

    public Task<List<PerkGroup>> GetPerksForPolicyAsync(long policyId)
    {
        return _db.PricePolicyPerkGroups
            .Where(x => x.PricePolicyId == policyId)
            .Select(x => x.PerkGroup)
            .ToListAsync();
    }

    public Task<List<PricePolicy>> GetPoliciesForPerkAsync(long perkGroupId)
    {
        return _db.PricePolicyPerkGroups
            .Where(x => x.PerkGroupId == perkGroupId)
            .Select(x => x.PricePolicy)
            .ToListAsync();
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
