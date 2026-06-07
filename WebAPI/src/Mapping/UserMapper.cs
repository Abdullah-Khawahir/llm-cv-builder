namespace WebAPI.Mappers;

public static class UserMapper
{
    public static UserDto ToDTO(User user)
    {
        return new UserDto(user.Id.ToString(), user.Email!);
    }
}
public sealed record class UserDto(string Id, string Email);

