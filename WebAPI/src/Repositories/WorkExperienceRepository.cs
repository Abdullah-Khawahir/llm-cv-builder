
namespace WebAPI.Repositories;

public class WorkExperienceRepository : IRepository<WorkExperience>
{
    private readonly ApplicationDbContext _context;
    public WorkExperienceRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<WorkExperience>> GetAllAsync() => await _context.WorkExperiences.AsNoTracking().ToListAsync();
    public async Task<WorkExperience?> GetByIdAsync(Guid id) => await _context.WorkExperiences.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    public async Task<WorkExperience> AddAsync(WorkExperience entity)
    {
        _context.WorkExperiences.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<WorkExperience> UpdateAsync(WorkExperience entity)
    {
        _context.WorkExperiences.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.WorkExperiences.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
