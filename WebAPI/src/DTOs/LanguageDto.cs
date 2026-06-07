namespace WebAPI.DTOs;

public sealed record class LanguageDto(Guid Id, string Name, string Proficiency, int Order);
