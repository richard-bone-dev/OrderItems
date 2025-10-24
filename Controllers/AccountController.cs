using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _customerManager;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> customerManager,
        IAntiforgery antiforgery,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _customerManager = customerManager;
        _antiforgery = antiforgery;
        _logger = logger;
    }

    // GET: /Account/GetCsrfToken
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Json(new { token = tokens.RequestToken });
    }

    // GET: /Account/_LoginPartial
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult _LoginPartial()
    {
        return PartialView("_LoginPartial");
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            if (IsFetchRequest())
                return Json(new { success = false, error = "Invalid input." });

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in: {Email}", model.Email);

            if (IsFetchRequest())
                return Json(new { success = true, redirectUrl = returnUrl });

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", model.Email);
            return Json(new { success = false, error = "Account locked. Try again later." });
        }

        if (IsFetchRequest())
            return Json(new { success = false, error = "Invalid login attempt." });

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        if (IsFetchRequest() || Request.Headers["Accept"].ToString().Contains("json", StringComparison.OrdinalIgnoreCase))
            return Json(new { success = true });

        // Fallback to redirect for non-AJAX
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        if (IsFetchRequest())
            return Unauthorized(new { success = false, error = "Access denied." });

        return View();
    }

    // Helper
    private bool IsFetchRequest()
    {
        return Request.Headers["X-Requested-With"] == "Fetch";
    }
}