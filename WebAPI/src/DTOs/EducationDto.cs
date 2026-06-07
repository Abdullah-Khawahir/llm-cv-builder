namespace WebAPI.DTOs;

public sealed record class EducationDto(Guid Id, string Institution, string Degree, int StartYear, int? EndYear, string Details, int Order);
