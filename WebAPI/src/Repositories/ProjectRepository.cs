namespace WebAPI.Repositories;

public sealed class ProjectRepository : IRepository<Project>
{
    private readonly AppDbContext _context;
    public ProjectRepository(AppDbContext context) => _context = context;
    public async Task<IReadOnlyList<Project>> GetAllAsync() => await _context.Projects.AsNoTracking().ToListAsync().ConfigureAwait(false);
    public async Task<Project?> GetByIdAsync(Guid id) => await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
    public async Task<Project> AddAsync(Project entity)
    {
        _context.Projects.Add(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task<Project> UpdateAsync(Project entity)
    {
        _context.Projects.Update(entity);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id).ConfigureAwait(false);
        if (entity != null)
        {
            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
