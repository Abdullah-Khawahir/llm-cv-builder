namespace WebAPI.Repositories;

public sealed class LanguageRepository : IRepository<Language>
{
    private readonly ApplicationDbContext _context;
    public LanguageRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Language>> GetAllAsync() => await _context.Languages.AsNoTracking().ToListAsync().ConfigureAwait(false);
    public async Task<Language?> GetByIdAsync(Guid id) => await _context.Languages.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
    public async Task<Language> AddAsync(Language entity)
    {
        _context.Languages.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task<Language> UpdateAsync(Language entity)
    {
        _context.Languages.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity is not null)
        {
            _context.Languages.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
