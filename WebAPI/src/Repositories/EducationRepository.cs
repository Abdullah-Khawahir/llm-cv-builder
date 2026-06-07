namespace WebAPI.Repositories;

public sealed class EducationRepository : IRepository<Education>
{
    private readonly ApplicationDbContext _context;
    public EducationRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Education>> GetAllAsync() => await _context.Educations.AsNoTracking().ToListAsync().ConfigureAwait(false);
    public async Task<Education?> GetByIdAsync(Guid id) => await _context.Educations.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
    public async Task<Education> AddAsync(Education entity)
    {
        _context.Educations.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task<Education> UpdateAsync(Education entity)
    {
        _context.Educations.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity is not null)
        {
            _context.Educations.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
