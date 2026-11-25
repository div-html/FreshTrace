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
public class ProfileModel : PageModel
{
    private readonly FreshTraceDataService _store;

    public ProfileModel(FreshTraceDataService store)
    {
        _store = store;
    }

    [BindProperty]
    public string FarmerName { get; set; } = "Ramesh Patel";

    [BindProperty]
    public string Phone { get; set; } = "+91 98160 44219";

    [BindProperty]
    public string FarmName { get; set; } = "Green Valley Organic Farm";

    [BindProperty]
    public string FarmArea { get; set; } = "15.5 Acres";

    [BindProperty]
    public string FarmDistrict { get; set; } = "Shimla District, Himachal Pradesh";

    [BindProperty]
    public string FarmCoords { get; set; } = "31.1048° N, 77.1734° E";

    public string UserEmail { get; private set; } = "farmer.ramesh@greenvalley.com";
    public string BackUrl { get; private set; } = "/Dashboard";
    public string? SaveSuccessMessage { get; private set; }

    public void OnGet()
    {
        LoadIdentity();
        var profile = _store.GetProfile(UserEmail, FarmerName, Phone);
        FarmerName = profile.FarmerName;
        Phone = profile.Phone;
        FarmName = profile.FarmName;
        FarmArea = profile.FarmArea;
        FarmDistrict = profile.FarmDistrict;
        FarmCoords = profile.FarmCoords;
        SaveSuccessMessage = TempData["ProfileSaved"] as string;
    }

    public IActionResult OnPostSave()
    {
        LoadIdentity();
        _store.UpdateProfile(UserEmail, new ProducerProfile
        {
            FarmerName = FarmerName,
            Phone = Phone,
            FarmName = FarmName,
            FarmArea = FarmArea,
            FarmDistrict = FarmDistrict,
            FarmCoords = FarmCoords
        });

        TempData["ProfileSaved"] = "Farm details and producer profile saved successfully!";
        return RedirectToPage("/Profile");
    }

    public async Task<IActionResult> OnPostSignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }

    private void LoadIdentity()
    {
        UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "farmer.ramesh@greenvalley.com";
        var role = (User.FindFirstValue(ClaimTypes.Role) ?? "farmer").ToLowerInvariant();
        BackUrl = role == "farmer" ? "/Farmer" : "/Dashboard";
        var dbUser = _store.FindUserByEmail(UserEmail);
        FarmerName = dbUser?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Ramesh Patel";
        Phone = dbUser?.Phone ?? User.FindFirstValue("phone") ?? "+91 98160 44219";
    }
}
