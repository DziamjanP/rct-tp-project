using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Data;

public class ExpiredLockCleanup : BackgroundService
{
    private readonly IServiceProvider _services;
    private static readonly TimeSpan ExpireAfter = TimeSpan.FromMinutes(30);

    public ExpiredLockCleanup(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), token);

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var expiry = DateTime.UtcNow.Subtract(ExpireAfter);

            List<TicketLock> expiredLocks = await db.TicketLocks
                .Where(l => !l.Paid && l.CreatedAt < expiry)
                .ToListAsync(cancellationToken: token);

            if (expiredLocks.Count > 0)
            {
                db.TicketLocks.RemoveRange(expiredLocks);
                await db.SaveChangesAsync(token);
            }
        }
    }
}
