using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
namespace WebAPI.Controllers;

[ApiController]
[Route("/api/auth")]
public sealed class AuthController(
        UserManager<User> userManager,
        IOptions<AppSettings> appSettings,
        ILogger<AuthController> logger
        ) : ControllerBase
{
    private UserManager<User> _userManager { get; } = userManager;
    private AppSettings _appSettings { get; } = appSettings.Value;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> NewUser(UserRegisterModel model)
    {
        _logger.LogInformation("Attempting to register new user with email: {Email}", model.Email);
        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            _logger.LogWarning("Registration failed: User with email {Email} already exists", model.Email);
            throw AppException.BadRequest("User already exists");
        }

        var user = new User
        {
            Email = model.Email,
            UserName = model.Email
        };

        var res = await _userManager.CreateAsync(user, model.Password);

        if (!res.Succeeded)
        {
            var errors = String.Join(", ", res.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed for {Email}: {Errors}", model.Email, errors);
            throw AppException.BadRequest(errors);
        }

        _logger.LogInformation("Successfully registered new user {Email}", model.Email);

        var savedUser = await _userManager.FindByEmailAsync(model.Email)
            ?? throw AppException.NotFound($"User with Email {model.Email}");

        return Ok(UserMapper.ToDTO(savedUser));
    }

    [HttpPost("login")]
    public async Task<ActionResult<JwtTokenResponse>> Login(UserLoginModel model)
    {
        _logger.LogInformation("Attempting login for user: {Email}", model.Email);
        var user = await _userManager.FindByEmailAsync(model.Email)
            ?? throw AppException.BadRequest("Invalid credentials");

        var valid = await _userManager.CheckPasswordAsync(user, model.Password);

        if (!valid)
        {
            _logger.LogWarning("Login failed: Invalid password for user {Email}", model.Email);
            throw AppException.BadRequest("Invalid credentials");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = JwtGenerator.GenerateToken(user, roles, _appSettings);
        _logger.LogInformation("Successfully logged in user {Email}", model.Email);
        return Ok(new JwtTokenResponse(token));
    }
}

public sealed record class JwtTokenResponse(string Token);
public sealed record class UserLoginModel(string Email, string Password);
public sealed record class UserRegisterModel(string Email, string Password);
