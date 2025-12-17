namespace CourseProject.Dtos;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    long UserId,
    int AccessLevel,
    string Name,
    string? Surname,
    string? Passport,
    string? Phone
);
