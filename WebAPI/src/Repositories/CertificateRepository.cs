namespace WebAPI.Repositories;

public sealed class CertificateRepository : IRepository<Certificate>
{
    private readonly ApplicationDbContext _context;
    public CertificateRepository(ApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<Certificate>> GetAllAsync() => await _context.Certificates.AsNoTracking().ToListAsync();
    public async Task<Certificate?> GetByIdAsync(Guid id) => await _context.Certificates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    public async Task<Certificate> AddAsync(Certificate entity)
    {
        _context.Certificates.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Certificate> UpdateAsync(Certificate entity)
    {
        _context.Certificates.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Certificates.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
