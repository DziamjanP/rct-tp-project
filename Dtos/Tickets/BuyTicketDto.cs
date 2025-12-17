namespace CourseProject.Dtos;

public record BuyTicketDto(
    long UserId,
    long EntryId,
    long? PerkGroupId
);
