
namespace WebAPI.Repositories;

public sealed class WorkExperienceRepository : IRepository<WorkExperience>
{
    private readonly ApplicationDbContext _context;
    public WorkExperienceRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<WorkExperience>> GetAllAsync() => await _context.WorkExperiences.AsNoTracking().ToListAsync().ConfigureAwait(false);
    public async Task<WorkExperience?> GetByIdAsync(Guid id) => await _context.WorkExperiences.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
    public async Task<WorkExperience> AddAsync(WorkExperience entity)
    {
        _context.WorkExperiences.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task<WorkExperience> UpdateAsync(WorkExperience entity)
    {
        _context.WorkExperiences.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity is not null)
        {
            _context.WorkExperiences.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
