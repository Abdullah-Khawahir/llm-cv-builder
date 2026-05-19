namespace WebAPI.Repositories;

public class SkillRepository : IRepository<Skill>
{
    private readonly ApplicationDbContext _context;
    public SkillRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Skill>> GetAllAsync() => await _context.Skills.AsNoTracking().ToListAsync();
    public async Task<Skill?> GetByIdAsync(Guid id) => await _context.Skills.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    public async Task<Skill> AddAsync(Skill entity)
    {
        _context.Skills.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Skill> UpdateAsync(Skill entity)
    {
        _context.Skills.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Skills.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
