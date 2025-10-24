using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (TempData.TryGetValue("StatusMessage", out var statusMessage))
        {
            ViewData["StatusMessage"] = statusMessage;
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await _signInManager.PasswordSignInAsync(
            viewModel.UserName,
            viewModel.Password,
            viewModel.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToLocal(viewModel.ReturnUrl, () =>
            {
                TempData["StatusMessage"] = "You have been signed in.";
                return RedirectToAction(nameof(Login));
            });
        }

        if (result.RequiresTwoFactor)
        {
            ModelState.AddModelError(string.Empty, "Two-factor authentication is not configured.");
            return View(viewModel);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "User account is locked.");
            return View(viewModel);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = new IdentityUser
        {
            UserName = viewModel.UserName,
            Email = viewModel.Email,
        };

        var result = await _userManager.CreateAsync(user, viewModel.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: viewModel.RememberMe);
            return RedirectToLocal(viewModel.ReturnUrl, () =>
            {
                TempData["StatusMessage"] = "Your account has been created and you are signed in.";
                return RedirectToAction(nameof(Login));
            });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return RedirectToLocal(returnUrl, () =>
        {
            TempData["StatusMessage"] = "You have been signed out.";
            return RedirectToAction(nameof(Login));
        });
    }

    private IActionResult RedirectToLocal(string? returnUrl, Func<IActionResult> fallback)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        if (!string.IsNullOrWhiteSpace(successMessage))
        {
            TempData["StatusMessage"] = successMessage;
        }

        return fallback();
    }

    public sealed record LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; init; }

        public string? ReturnUrl { get; init; }
    }

    public sealed record RegisterViewModel
    {
        [Required]
        [StringLength(256, MinimumLength = 3)]
        [Display(Name = "User name")]
        public string UserName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; init; }

        public string? ReturnUrl { get; init; }
    }
}
