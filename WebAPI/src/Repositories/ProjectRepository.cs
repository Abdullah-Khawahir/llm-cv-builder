namespace WebAPI.Repositories;

public class ProjectRepository : IRepository<Project>
{
    private readonly ApplicationDbContext _context;
    public ProjectRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Project>> GetAllAsync() => await _context.Projects.AsNoTracking().ToListAsync();
    public async Task<Project?> GetByIdAsync(Guid id) => await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    public async Task<Project> AddAsync(Project entity)
    {
        _context.Projects.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Project> UpdateAsync(Project entity)
    {
        _context.Projects.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
