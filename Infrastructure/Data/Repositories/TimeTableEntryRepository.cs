using CourseProject.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using CourseProject.Models;
using CourseProject.Data;
using CourseProject.Dtos;

namespace CourseProject.Infrastructure.Data.Repositories;

public class TimeTableEntryRepository : ITimeTableEntryRepository
{
    private AppDbContext _db;

    public TimeTableEntryRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<TimeTableEntry?> GetByIdAsync(long id) => _db.TimeTableEntries.FindAsync(id).AsTask();

    public Task<TimetableSearchDto> GetDtoByIdAsync(long id) {
        var query =
            from e in _db.TimeTableEntries
            join t in _db.Trains on e.TrainId equals t.Id
            join s in _db.Stations on t.SourceId equals s.Id
            join d in _db.Stations on t.DestinationId equals d.Id
            join tt in _db.TrainTypes on t.TypeId equals tt.Id
            select new { e, t, s, d, tt };

        query = query.Where(x => x.e.Id == id);

        return query
            .Select(x => new TimetableSearchDto(
                x.e.Id,
                x.t.Id,
                x.tt.Name,
                x.s.Name,
                x.d.Name,
                x.e.DepartureTime,
                x.e.ArrivalTime,
                x.e.PricePolicyId
            ))
            .FirstAsync();
    }
    public Task<List<TimeTableEntry>> GetAllAsync() =>
        _db.TimeTableEntries.Include(e => e.Train).OrderBy(e => e.DepartureTime).ToListAsync();
    public Task<List<TimeTableEntry>> GetAfterAsync(DateTime after) =>
        _db.TimeTableEntries.Include(e => e.Train).Where(e => e.ArrivalTime >= after).OrderBy(e => e.DepartureTime).ToListAsync();

    public Task<List<TimetableSearchDto>> SearchAsync(
        long? fromStationId,
        long? toStationId,
        DateTime? after,
        DateTime? before
    ) {
        var query =
            from e in _db.TimeTableEntries
            join t in _db.Trains on e.TrainId equals t.Id
            join s in _db.Stations on t.SourceId equals s.Id
            join d in _db.Stations on t.DestinationId equals d.Id
            join tt in _db.TrainTypes on t.TypeId equals tt.Id
            select new { e, t, s, d, tt };

        if (after != null)
            after = after.Value.ToUniversalTime();

        if (before != null)
            before = before.Value.ToUniversalTime();


        if (fromStationId != null)
            query = query.Where(x => x.s.Id == fromStationId);

        if (toStationId != null)
            query = query.Where(x => x.d.Id == toStationId);

        if (after != null)
            query = query.Where(x => x.e.DepartureTime >= after);

        if (before != null)
            query = query.Where(x => x.e.ArrivalTime <= before);

        return query
            .Select(x => new TimetableSearchDto(
                x.e.Id,
                x.t.Id,
                x.tt.Name,
                x.s.Name,
                x.d.Name,
                x.e.DepartureTime,
                x.e.ArrivalTime,
                null
            ))
            .ToListAsync();
    }
    public async Task AddAsync(TimeTableEntry TimeTableEntry)
    {
        _db.TimeTableEntries.Add(TimeTableEntry);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TimeTableEntry TimeTableEntry)
    {
        _db.TimeTableEntries.Remove(TimeTableEntry);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}
