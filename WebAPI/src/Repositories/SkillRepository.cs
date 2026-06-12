namespace WebAPI.Repositories;

public sealed class SkillRepository : IRepository<Skill>
{
    private readonly AppDbContext _context;
    public SkillRepository(AppDbContext context) => _context = context;
    public async Task<IReadOnlyList<Skill>> GetAllAsync() => await _context.Skills.AsNoTracking().ToListAsync().ConfigureAwait(false);
    public async Task<Skill?> GetByIdAsync(Guid id) => await _context.Skills.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
    public async Task<Skill> AddAsync(Skill entity)
    {
        _context.Skills.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task<Skill> UpdateAsync(Skill entity)
    {
        _context.Skills.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity is not null)
        {
            _context.Skills.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
