namespace WebAPI;

public sealed class AppSettings
{
    [Required]
    public ConnectionStrings ConnectionStrings { get; set; } = default!;
}

public sealed record ConnectionStrings(string DefaultConnection);
