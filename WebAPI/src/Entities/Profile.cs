namespace WebAPI.Entities;

public class Profile : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
