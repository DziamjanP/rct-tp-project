namespace CourseProject.Dtos;

public record UserResponseDto(
    long Id,
    string Name,
    string? Surname,
    string? Passport,
    string? Phone,
    int AccessLevel
);
