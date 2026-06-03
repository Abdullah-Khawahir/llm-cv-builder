namespace WebAPI.DTOs;

public sealed record WorkExperienceDto(
    Guid Id,
    string Company,
    string Position,
    DateTime StartDate,
    DateTime? EndDate,
    string Description,
    int Order
    );
