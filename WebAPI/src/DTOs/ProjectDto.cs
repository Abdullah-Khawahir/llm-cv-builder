namespace WebAPI.DTOs;

public sealed record ProjectDto(Guid Id, string Name, string Description, string Url, string TechStack, int Order);
