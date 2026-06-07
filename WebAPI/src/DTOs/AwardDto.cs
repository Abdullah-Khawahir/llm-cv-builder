namespace WebAPI.DTOs;

public sealed record class AwardDto(Guid Id, string Title, DateTime DateAwarded, string Description, int Order);
