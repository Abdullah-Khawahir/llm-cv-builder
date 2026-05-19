namespace WebAPI.Repositories;

public class UserProfileRepository : IRepository<UserProfile>
{
    private readonly ApplicationDbContext _context;
    public UserProfileRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<UserProfile>> GetAllAsync() => await _context.UserProfiles.AsNoTracking().ToListAsync();
    public async Task<UserProfile?> GetByIdAsync(Guid id) => await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    public async Task<UserProfile> AddAsync(UserProfile entity)
    {
        _context.UserProfiles.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<UserProfile> UpdateAsync(UserProfile entity)
    {
        _context.UserProfiles.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.UserProfiles.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
