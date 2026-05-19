namespace WebAPI.Repositories;

public class UserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync((u) => u.Id == userId);
    }
    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user is not null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
