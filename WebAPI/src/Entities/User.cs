
namespace WebAPI.Entities;
public class User : IdentityUser<Guid>
{
    public required Profile profile { get; set; }
}
