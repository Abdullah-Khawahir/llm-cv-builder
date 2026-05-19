






namespace WebAPI.Repositories;

public class LanguageRepository : IRepository<Language>
{
    private readonly ApplicationDbContext _context;
    public LanguageRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Language>> GetAllAsync() => await _context.Languages.AsNoTracking().ToListAsync();
    public async Task<Language?> GetByIdAsync(Guid id) => await _context.Languages.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
    public async Task<Language> AddAsync(Language entity)
    {
        _context.Languages.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Language> UpdateAsync(Language entity)
    {
        _context.Languages.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Languages.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
