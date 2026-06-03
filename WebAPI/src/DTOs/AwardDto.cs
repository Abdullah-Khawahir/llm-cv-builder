namespace WebAPI.DTOs;

public sealed record AwardDto(Guid Id, string Title, DateTime DateAwarded, string Description, int Order);
