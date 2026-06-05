using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalAuthController(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IOptions<AppSettings> appSettingsOptions) : ControllerBase
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly AppSettings _appSettings = appSettingsOptions.Value;

    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "ExternalAuth");

        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return BadRequest("Error loading external login information from Google.");

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return BadRequest("Email claim not received from Google.");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new User { UserName = email, Email = email };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded) return BadRequest(createResult.Errors);

            var linkResult = await _userManager.AddLoginAsync(user, info);
            if (!linkResult.Succeeded) return BadRequest(linkResult.Errors);
        }
        else
        {
            var userLogins = await _userManager.GetLoginsAsync(user);
            var isGoogleLinked = userLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey);

            if (!isGoogleLinked)
            {
                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded) return BadRequest(linkResult.Errors);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = JwtGenerator.GenerateToken(user, roles, _appSettings);

        return Ok(new { Token = token });
    }
}
