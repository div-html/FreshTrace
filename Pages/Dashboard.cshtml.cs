using System.Security.Claims;
using FreshTrace.Models;
using FreshTrace.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshTrace.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly FreshTraceDataService _store;

    public DashboardModel(FreshTraceDataService store)
    {
        _store = store;
    }

    public string UserName { get; private set; } = "Stakeholder";
    public string ActiveRole { get; private set; } = "admin";
    public RoleDashboardConfig RoleConfig { get; private set; } = new();

    [BindProperty]
    public string ConsumerBatchQuery { get; set; } = "BATCH-2026-APL-098";

    public string? ConsumerTraceResult { get; private set; }
    public string? SuccessNotice { get; private set; }
    public string? CtaNotice { get; private set; }

    public IActionResult OnGet()
    {
        var role = (User.FindFirstValue(ClaimTypes.Role) ?? "farmer").ToLowerInvariant();
        if (role == "farmer")
        {
            return RedirectToPage("/Farmer");
        }

        LoadDashboardState(role);
        SuccessNotice = TempData["SuccessNotice"] as string;
        CtaNotice = TempData["CtaNotice"] as string;
        return Page();
    }

    public IActionResult OnPostVerifyBatch()
    {
        var role = (User.FindFirstValue(ClaimTypes.Role) ?? "consumer").ToLowerInvariant();
        LoadDashboardState(role);

        var (_, message) = _store.LookupTraceability(ConsumerBatchQuery);
        ConsumerTraceResult = message;
        return Page();
    }

    public IActionResult OnPostTriggerCta()
    {
        var role = (User.FindFirstValue(ClaimTypes.Role) ?? "admin").ToLowerInvariant();
        var cfg = _store.GetRoleDashboard(role);
        TempData["CtaNotice"] = cfg.CtaMessage;
        return RedirectToPage("/Dashboard");
    }

    public async Task<IActionResult> OnPostSignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }

    private void LoadDashboardState(string role)
    {
        ActiveRole = role;
        var email = User.FindFirstValue(ClaimTypes.Email);
        var dbUser = _store.FindUserByEmail(email);
        UserName = dbUser?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Stakeholder";
        RoleConfig = _store.GetRoleDashboard(role);
    }
}
