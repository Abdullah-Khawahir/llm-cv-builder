using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalAuthController(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IOptions<AppSettings> appSettingsOptions,
    ILogger<ExternalAuthController> logger
    ) : ControllerBase
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly AppSettings _appSettings = appSettingsOptions.Value;
    private readonly ILogger<ExternalAuthController> _logger = logger;

    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
    {
        _logger.LogInformation("Initiating Google login flow");
        var redirectUrl = Url.Action(nameof(GoogleCallback), "ExternalAuth");

        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        _logger.LogInformation("Processing Google authentication callback");
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Google callback failed: External login information not found");
            return BadRequest("Error loading external login information from Google.");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Google callback failed: Email claim not received for user {ProviderKey}", info.ProviderKey);
            return BadRequest("Email claim not received from Google.");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            _logger.LogInformation("Creating new user account for Google login: {Email}", email);
            user = new User { UserName = email, Email = email };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user account for {Email}: {Errors}", email, String.Join(", ", createResult.Errors.Select(e => e.Description)));
                return BadRequest(createResult.Errors);
            }

            var linkResult = await _userManager.AddLoginAsync(user, info);
            if (!linkResult.Succeeded)
            {
                _logger.LogError("Failed to link Google account to user {Email}: {Errors}", email, String.Join(", ", linkResult.Errors.Select(e => e.Description)));
                return BadRequest(linkResult.Errors);
            }
        }
        else
        {
            var userLogins = await _userManager.GetLoginsAsync(user);
            var isGoogleLinked = userLogins.Any(l =>
                    string.Equals(l.LoginProvider, info.LoginProvider, StringComparison.Ordinal)
                        && string.Equals(l.ProviderKey, info.ProviderKey, StringComparison.Ordinal)
                        );

            if (!isGoogleLinked)
            {
                _logger.LogInformation("Linking Google account to existing user {Email}", email);
                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded)
                {
                    _logger.LogError("Failed to link Google account to existing user {Email}: {Errors}", email, String.Join(", ", linkResult.Errors.Select(e => e.Description)));
                    return BadRequest(linkResult.Errors);
                }
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = JwtGenerator.GenerateToken(user, roles, _appSettings);
        _logger.LogInformation("Successfully authenticated user {Email} via Google", email);

        return Ok(new { Token = token });
    }
}
