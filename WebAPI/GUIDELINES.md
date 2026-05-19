**Guidelines for Backend Development – C# .NET Web API**

---

### 1️⃣ Entity Definition
- Keep domain entities under `src/Entities/`.
- Every entity must include the audit fields **CreatedAt**, **UpdatedAt** and **DeletedAt** (nullable) for soft‑delete and filtering.
- Use **`Guid.CreateVersion7()`** for primary keys.

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
}
```

---

### 2️⃣ DTOs & Mapping Functions
- DTOs live in `src/Dtos/` (one file per DTO).
- Naming: **`<Entity><Purpose>Dto`** (e.g., `UserDto`).
- Request objects: **`<Entity><Action>Request`** (e.g., `UserCreateRequest`).
- Provide a static mapping class per entity with extension methods:

```csharp
public static class UserMappings
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    public static User ToEntity(this UserCreateRequest req) => new()
    {
        Id = Guid.CreateVersion7(),
        Email = req.Email,
        Name = req.Name,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    public static void ApplyUpdate(this User user, UserUpdateRequest req)
    {
        user.Email = req.Email;
        user.Name = req.Name;
        user.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
```

---

### 3️⃣ DbContext – Entity Set
- Add a `DbSet<T>` for each entity in **`ApplicationDbContext`**.
- Configure soft‑delete filters in `OnModelCreating`:

```csharp
public DbSet<User> Users => Set<User>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    // repeat for other entities…
}
```

---

### 4️⃣ Repository Pattern (CRUD)
- Interface `I<User>Repository` and concrete `UserRepository`.
- All methods are async and return `Task<T>`.
- Throw **`!!`** (C# 12 “!‑bang”) for not‑found cases:

```csharp
public async Task<User> GetAsync(Guid id, CancellationToken ct = default) =>
    await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
        ?? throw new NotFoundException($"User {id} not found")!!;
```
- Generate weak ETag from `UpdatedAt` for caching.

---

### 5️⃣ Services (Business Logic)
- Services live in `src/Services/` and receive repositories via DI.
- Inject `ILogger<T>` and log start, end and errors.

```csharp
public class UserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<UserService> _log;

    public UserService(IUserRepository repo, ILogger<UserService> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task<UserDto> CreateAsync(UserCreateRequest req, CancellationToken ct = default)
    {
        _log.LogInformation("Creating user {Email}", req.Email);
        var user = req.ToEntity();
        await _repo.AddAsync(user, ct);
        return user.ToDto();
    }
    // Update, Delete (soft), Get, List …
}
```

---

### 6️⃣ Controllers – Endpoints
- Controllers in `src/Controllers/`.
- Use `[ApiController]`, `[Route("api/[controller]")]`.
- Return `IActionResult` and set ETag / If‑None‑Match headers.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _svc;
    private readonly ILogger<UsersController> _log;

    public UsersController(UserService svc, ILogger<UsersController> log)
    {
        _svc = svc;
        _log = log;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(UserCreateRequest req, CancellationToken ct)
    {
        var dto = await _svc.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _svc.GetAsync(id, ct);
        var eTag = $"\"{dto.UpdatedAt.ToUnixTimeSeconds()}\"";
        Response.Headers.ETag = eTag;
        if (Request.Headers.IfNoneMatch.Any(v => v == eTag))
            return StatusCode(StatusCodes.Status304NotModified);
        return Ok(dto);
    }
    // PUT, PATCH, DELETE (soft) with similar ETag handling
}
```

---

### 7️⃣ Cross‑Cutting Concerns
- Global `ProblemDetails` middleware for converting `!!` exceptions to proper API errors.
- Enable ASP.NET Core **Response Caching** middleware; still emit ETag headers per action.
- Consistent logging in all services and controllers.

---

### 8️⃣ Folder Layout
```
src/
├─ Controllers/
│   └─ UsersController.cs
├─ Entities/
│   └─ User.cs
├─ Dtos/
│   ├─ UserDto.cs
│   ├─ UserSummaryDto.cs
│   ├─ UserCreateRequest.cs
│   ├─ UserUpdateRequest.cs
│   └─ UserMappings.cs
├─ Repositories/
│   ├─ IUserRepository.cs
│   └─ UserRepository.cs
├─ Services/
│   └─ UserService.cs
└─ Databases/
    └─ ApplicationDbContext.cs
```

---

### 9️⃣ Future‑Proofing
- Add a new entity by following the same pipeline: **Entity → DTOs → Mappings → DbSet + filter → Repository → Service → Controller**.
- Keep audit fields, ETag handling and logging uniform.

---

*These guidelines keep the backend safe, observable, cache‑friendly and aligned with modern C# language features.*