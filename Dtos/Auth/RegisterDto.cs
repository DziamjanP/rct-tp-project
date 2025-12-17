namespace CourseProject.Dtos;

public record RegisterDto(
    string Name,
    string? Surname,
    string Phone,
    string Password
);
