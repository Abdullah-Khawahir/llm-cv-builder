
namespace WebAPI.Repositories;

public sealed class ProfileRepository : IRepository<Profile>
{
    private readonly ApplicationDbContext _context;
    public ProfileRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Profile>> GetAllAsync() => await _context.Profiles.AsNoTracking().ToListAsync();
    public async Task<Profile?> GetByIdAsync(Guid id) => await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    public async Task<Profile> AddAsync(Profile entity)
    {
        _context.Profiles.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Profile> UpdateAsync(Profile entity)
    {
        _context.Profiles.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Profiles.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
