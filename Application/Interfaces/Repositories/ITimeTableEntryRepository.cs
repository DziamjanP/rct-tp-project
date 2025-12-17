using CourseProject.Dtos;
using CourseProject.Models;

namespace CourseProject.Application.Interfaces.Repositories;

public interface ITimeTableEntryRepository
{
    Task<TimeTableEntry?> GetByIdAsync(long id);
    Task<TimetableSearchDto> GetDtoByIdAsync(long id);
    Task<List<TimetableSearchDto>> SearchAsync(long? fromStationId, long? toStationId, DateTime? after, DateTime? before);
    Task<List<TimeTableEntry>> GetAllAsync();
    Task<List<TimeTableEntry>> GetAfterAsync(DateTime after);
    Task AddAsync(TimeTableEntry timeTableEntry);
    Task DeleteAsync(TimeTableEntry timeTableEntry);
    Task SaveChangesAsync();
}