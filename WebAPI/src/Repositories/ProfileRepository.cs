
namespace WebAPI.Repositories;

public sealed class ProfileRepository : IRepository<Profile>
{
    private readonly AppDbContext _context;
    public ProfileRepository(AppDbContext context) => _context = context;
    public async Task<IReadOnlyList<Profile>> GetAllAsync() => await _context.Profiles.AsNoTracking().ToListAsync().ConfigureAwait(false);
    public async Task<Profile?> GetByIdAsync(Guid id) => await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
    public async Task<Profile> AddAsync(Profile entity)
    {
        _context.Profiles.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task<Profile> UpdateAsync(Profile entity)
    {
        _context.Profiles.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity is not null)
        {
            _context.Profiles.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
