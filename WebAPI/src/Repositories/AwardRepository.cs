namespace WebAPI.Repositories;

public sealed class AwardRepository : IRepository<Award>
{
    private readonly AppDbContext _context;

    public AwardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Award>> GetAllAsync()
    {
        return await _context.Awards.AsNoTracking().ToListAsync().ConfigureAwait(false);
    }

    public async Task<Award?> GetByIdAsync(Guid id)
    {
        return await _context.Awards.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);
    }

    public async Task<Award> AddAsync(Award entity)
    {
        _context.Awards.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public async Task<Award> UpdateAsync(Award entity)
    {
        _context.Awards.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity is not null)
        {
            _context.Awards.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
