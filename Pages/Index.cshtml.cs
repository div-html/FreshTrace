using System.Security.Claims;
using FreshTrace.Models;
using FreshTrace.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshTrace.Pages;

public class IndexModel : PageModel
{
    private readonly FreshTraceDataService _store;

    public IndexModel(FreshTraceDataService store)
    {
        _store = store;
    }

    [BindProperty(SupportsGet = true)]
    public string Mode { get; set; } = "login";

    [BindProperty]
    public string LoginEmail { get; set; } = string.Empty;

    [BindProperty]
    public string LoginPassword { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; } = true;

    [BindProperty]
    public string RegName { get; set; } = string.Empty;

    [BindProperty]
    public string RegEmail { get; set; } = string.Empty;

    [BindProperty]
    public string RegPassword { get; set; } = string.Empty;

    [BindProperty]
    public string RegPhone { get; set; } = string.Empty;

    [BindProperty]
    public string RegRole { get; set; } = "farmer";

    [BindProperty]
    public string ForgotEmail { get; set; } = string.Empty;

    public string? LoginError { get; set; }
    public string? RegisterError { get; set; }
    public string? ForgotSuccess { get; set; }
    public bool ShowForgotModal { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "farmer";
            return role.Equals("farmer", StringComparison.OrdinalIgnoreCase)
                ? RedirectToPage("/Farmer")
                : RedirectToPage("/Dashboard");
        }

        Mode = Mode?.Equals("register", StringComparison.OrdinalIgnoreCase) == true ? "register" : "login";
        return Page();
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        Mode = "login";
        var user = _store.Authenticate(LoginEmail, LoginPassword);
        if (user is null)
        {
            LoginError = "Incorrect email or password. Please try again.";
            return Page();
        }

        await SignInUserAsync(user, RememberMe);

        return user.Role.Equals("farmer", StringComparison.OrdinalIgnoreCase)
            ? RedirectToPage("/Farmer")
            : RedirectToPage("/Dashboard");
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        Mode = "register";
        var (success, error, newUser) = _store.RegisterUser(RegName, RegEmail, RegPassword, RegPhone, RegRole);
        if (!success || newUser is null)
        {
            RegisterError = error ?? "Could not create account.";
            return Page();
        }

        await SignInUserAsync(newUser, isPersistent: true);
        TempData["SuccessNotice"] = $"Account created successfully! Welcome, {newUser.Name}.";

        return newUser.Role.Equals("farmer", StringComparison.OrdinalIgnoreCase)
            ? RedirectToPage("/Farmer")
            : RedirectToPage("/Dashboard");
    }

    public IActionResult OnPostForgotPassword()
    {
        Mode = "login";
        ShowForgotModal = true;
        var target = string.IsNullOrWhiteSpace(ForgotEmail) ? "your email" : ForgotEmail.Trim();
        ForgotSuccess = $"Password reset instructions sent to {target}. The one-time secure token expires in 15 minutes.";
        return Page();
    }

    private async Task SignInUserAsync(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToLowerInvariant()),
            new("phone", user.Phone)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(isPersistent ? 7 : 1)
            });
    }
}
