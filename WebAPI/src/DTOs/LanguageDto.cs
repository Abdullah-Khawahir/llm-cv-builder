namespace WebAPI.DTOs;

public sealed record LanguageDto(Guid Id, string Name, string Proficiency, int Order);
