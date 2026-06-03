namespace WebAPI.DTOs;

public sealed record EducationDto(Guid Id, string Institution, string Degree, int StartYear, int? EndYear, string Details, int Order);
