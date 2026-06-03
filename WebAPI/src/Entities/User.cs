namespace WebAPI.Entities;

public sealed class User : IdentityUser<Guid>
{
    public Profile profile { get; set; } = default!;
}
