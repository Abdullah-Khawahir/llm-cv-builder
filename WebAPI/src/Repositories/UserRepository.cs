namespace WebAPI.Repositories;

public sealed class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync((u) => u.Id == userId).ConfigureAwait(false);
    }
    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId).ConfigureAwait(false);
        if (user is not null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
