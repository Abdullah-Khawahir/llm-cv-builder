






namespace WebAPI.Repositories;

public class EducationRepository : IRepository<Education>
{
    private readonly ApplicationDbContext _context;
    public EducationRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Education>> GetAllAsync() => await _context.Educations.AsNoTracking().ToListAsync();
    public async Task<Education?> GetByIdAsync(Guid id) => await _context.Educations.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    public async Task<Education> AddAsync(Education entity)
    {
        _context.Educations.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Education> UpdateAsync(Education entity)
    {
        _context.Educations.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Educations.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
