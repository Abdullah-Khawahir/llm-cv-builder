namespace WebAPI.DTOs;

public sealed record class ProjectDto(Guid Id, string Name, string Description, string Url, string TechStack, int Order);
