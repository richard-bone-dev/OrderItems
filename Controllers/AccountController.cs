using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("Login")]
    [AllowAnonymous]
    public IActionResult LoginInstructions()
    {
        return Ok(new
        {
            Message = "Submit credentials via POST /Account/Login with JSON payload { \"userName\": \"...\", \"password\": \"...\", \"rememberMe\": false }"
        });
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return NoContent();
        }

        if (result.RequiresTwoFactor)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { Error = "Two-factor authentication is not configured." });
        }

        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, new { Error = "User account is locked." });
        }

        return Unauthorized(new { Error = "Invalid username or password." });
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);

        return Created($"/Account/{user.Id}", new { user.Id, user.UserName, user.Email });
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    public sealed record LoginRequest
    {
        [Required]
        public string UserName { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;

        public bool RememberMe { get; init; }
    }

    public sealed record RegisterRequest
    {
        [Required]
        [StringLength(256, MinimumLength = 3)]
        public string UserName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;

        public bool RememberMe { get; init; }
    }
}
