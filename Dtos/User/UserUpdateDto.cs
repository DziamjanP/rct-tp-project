namespace CourseProject.Dtos;

public record UserUpdateDto(
    string? Phone, 
    string? Name,
    string? Passport,
    string? Surname,
    int? AccessLevel
);
