
using Microsoft.Extensions.Options;

[ApiController]
[Route("/api/auth")]
public sealed class AuthController(
        UserManager<User> userManager,
        IOptions<AppSettings> appSettings
        ) : ControllerBase
{
    private UserManager<User> _userManager { get; } = userManager;
    public AppSettings _appSettings { get; } = appSettings.Value;

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> NewUser(UserRegisterModel model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) is not null)
            throw AppException.BadRequest("User already exists");

        var user = new User
        {
            Email = model.Email,
            UserName = model.Email
        };

        var res = await _userManager.CreateAsync(user, model.Password);

        if (!res.Succeeded)
        {
            throw AppException.BadRequest(String.Join(", ", res.Errors.Select(e => e.Description)));
        }

        return Ok(UserMapper.ToDTO(await _userManager.FindByEmailAsync(model.Email)!));
    }

    [HttpPost("login")]
    public async Task<ActionResult<JwtTokenResponse>> Login(UserLoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email)
            ?? throw AppException.BadRequest("Invalid credentials");

        var valid = await _userManager.CheckPasswordAsync(user, model.Password);

        if (!valid)
            throw AppException.BadRequest("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var token = JwtGenerator.GenerateToken(user, roles, _appSettings);
        return Ok(new JwtTokenResponse(token));
    }

}
public sealed record JwtTokenResponse(string Token);
public sealed record UserLoginModel(string Email, string Password);
public sealed record UserRegisterModel(string Email, string Password);
