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
public class FarmerModel : PageModel
{
    private readonly FreshTraceDataService _store;

    public FarmerModel(FreshTraceDataService store)
    {
        _store = store;
    }

    [BindProperty(SupportsGet = true)]
    public string Section { get; set; } = "dashboard";

    [BindProperty(SupportsGet = true)]
    public string Status { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string ShipmentStatus { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string Q { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? Qr { get; set; }

    // Create Batch Modal fields
    [BindProperty]
    public string NewProduct { get; set; } = string.Empty;

    [BindProperty]
    public string NewCategory { get; set; } = "Fresh Fruits";

    [BindProperty]
    public string NewFarm { get; set; } = "Green Valley Organic Farm";

    [BindProperty]
    public string NewHarvestDate { get; set; } = "2026-11-18";

    [BindProperty]
    public int NewQuantity { get; set; } = 800;

    [BindProperty]
    public string NewGrade { get; set; } = "Grade A (Export)";

    [BindProperty]
    public double NewTemp { get; set; } = 3.5;

    [BindProperty]
    public string NewStatus { get; set; } = "Ready for Dispatch";

    // Add Farm Modal fields
    [BindProperty]
    public string AddFarmName { get; set; } = string.Empty;

    [BindProperty]
    public string AddFarmDistrict { get; set; } = string.Empty;

    [BindProperty]
    public double AddFarmArea { get; set; }

    [BindProperty]
    public string AddFarmCert { get; set; } = "Certified Organic";

    [BindProperty]
    public string AddFarmCrops { get; set; } = string.Empty;

    // Profile Edit fields
    [BindProperty]
    public string ProfileName { get; set; } = "Ramesh Patel";

    [BindProperty]
    public string ProfilePhone { get; set; } = "+91 98160 44219";

    [BindProperty]
    public string ProfileFarmName { get; set; } = "Green Valley Organic Farm";

    [BindProperty]
    public string ProfileFarmArea { get; set; } = "15.5 Acres";

    [BindProperty]
    public string ProfileFarmDistrict { get; set; } = "Shimla District, Himachal Pradesh";

    [BindProperty]
    public string ProfileFarmCoords { get; set; } = "31.1048° N, 77.1734° E";

    // View state
    public string UserEmail { get; private set; } = "farmer.ramesh@greenvalley.com";
    public string DisplayName { get; private set; } = "Ramesh Patel";
    public bool IsRailCollapsed { get; private set; }
    public string? ToastMessage { get; private set; }

    public List<FarmParcel> Farms { get; private set; } = new();
    public List<HarvestBatch> Batches { get; private set; } = new();
    public List<HarvestBatch> FilteredBatches { get; private set; } = new();
    public List<ColdChainShipment> Shipments { get; private set; } = new();
    public List<ColdChainShipment> FilteredShipments { get; private set; } = new();
    public HarvestBatch? QrBatch { get; private set; }

    // Derived KPIs
    public double TotalAcres { get; private set; }
    public List<HarvestBatch> ReadyBatches { get; private set; } = new();
    public int ReadyStockKg { get; private set; }
    public int TotalVolumeKg { get; private set; }
    public int PendingCount { get; private set; }
    public int PassRate { get; private set; }
    public List<ColdChainShipment> ActiveShipments { get; private set; } = new();
    public int InTransitKg { get; private set; }

    public static readonly string[] StatusTabs =
        ["All", "Ready for Dispatch", "In Transit", "Inspection Pending", "Delivered"];

    public static readonly string[] ShipmentTabs =
        ["All", "In Transit", "Delivered"];

    public IActionResult OnGet()
    {
        var role = (User.FindFirstValue(ClaimTypes.Role) ?? "farmer").ToLowerInvariant();
        if (role != "farmer")
        {
            return RedirectToPage("/Dashboard");
        }

        LoadWorkspaceState();
        return Page();
    }

    public IActionResult OnPostToggleRail(string? returnSection, string? returnStatus, string? returnShipmentStatus, string? returnQ)
    {
        var current = HttpContext.Session.GetString("FarmerRailCollapsed") == "true";
        HttpContext.Session.SetString("FarmerRailCollapsed", (!current).ToString().ToLowerInvariant());

        return RedirectToPage("/Farmer", new
        {
            section = NormalizeSection(returnSection),
            status = returnStatus,
            shipmentStatus = returnShipmentStatus,
            q = returnQ
        });
    }

    public IActionResult OnPostCreateBatch(string? returnSection)
    {
        var created = _store.CreateBatch(
            NewProduct,
            NewCategory,
            NewFarm,
            NewHarvestDate,
            NewQuantity,
            NewGrade,
            NewTemp,
            NewStatus);

        if (created is not null)
        {
            TempData["ToastMsg"] = $"Harvest batch {created.Code} ({created.Product}, {created.QuantityKg:N0} kg) logged with QR provenance token.";
        }

        return RedirectToPage("/Farmer", new { section = NormalizeSection(returnSection) });
    }

    public IActionResult OnPostDispatchBatch(string batchCode, string? returnSection, string? returnStatus, string? returnShipmentStatus, string? returnQ)
    {
        if (_store.DispatchBatch(batchCode))
        {
            TempData["ToastMsg"] = $"Batch {batchCode} dispatched to Cold-Chain Transit. Live tracking added to Shipments.";
        }

        return RedirectToPage("/Farmer", new
        {
            section = NormalizeSection(returnSection),
            status = returnStatus,
            shipmentStatus = returnShipmentStatus,
            q = returnQ
        });
    }

    public IActionResult OnPostDeliverShipment(string shipmentId, string batchCode, string? returnShipmentStatus)
    {
        if (_store.MarkShipmentDelivered(shipmentId, batchCode))
        {
            TempData["ToastMsg"] = $"Shipment {shipmentId} ({batchCode}) confirmed delivered to hub.";
        }

        return RedirectToPage("/Farmer", new
        {
            section = "shipments",
            shipmentStatus = returnShipmentStatus
        });
    }

    public IActionResult OnPostAddFarm(string? returnSection)
    {
        var created = _store.AddFarm(AddFarmName, AddFarmDistrict, AddFarmArea, AddFarmCert, AddFarmCrops);
        if (created is not null)
        {
            TempData["ToastMsg"] = $"Farm parcel \"{created.Name}\" ({created.AreaAcres} Acres) registered to your producer account.";
        }

        return RedirectToPage("/Farmer", new { section = NormalizeSection(returnSection) });
    }

    public IActionResult OnPostSaveProfile()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? "farmer.ramesh@greenvalley.com";
        _store.UpdateProfile(email, new ProducerProfile
        {
            FarmerName = ProfileName,
            Phone = ProfilePhone,
            FarmName = ProfileFarmName,
            FarmArea = ProfileFarmArea,
            FarmDistrict = ProfileFarmDistrict,
            FarmCoords = ProfileFarmCoords
        });

        TempData["ToastMsg"] = "Producer profile and farm parcel details saved successfully.";
        return RedirectToPage("/Farmer", new { section = "profile" });
    }

    public async Task<IActionResult> OnPostSignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }

    private void LoadWorkspaceState()
    {
        Section = NormalizeSection(Section);
        Status = string.IsNullOrWhiteSpace(Status) ? "All" : Status;
        ShipmentStatus = string.IsNullOrWhiteSpace(ShipmentStatus) ? "All" : ShipmentStatus;
        IsRailCollapsed = HttpContext.Session.GetString("FarmerRailCollapsed") == "true";

        UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "farmer.ramesh@greenvalley.com";
        var dbUser = _store.FindUserByEmail(UserEmail);
        var profile = _store.GetProfile(UserEmail, dbUser?.Name ?? User.FindFirstValue(ClaimTypes.Name), dbUser?.Phone ?? User.FindFirstValue("phone"));

        ProfileName = profile.FarmerName;
        ProfilePhone = profile.Phone;
        ProfileFarmName = profile.FarmName;
        ProfileFarmArea = profile.FarmArea;
        ProfileFarmDistrict = profile.FarmDistrict;
        ProfileFarmCoords = profile.FarmCoords;
        DisplayName = string.IsNullOrWhiteSpace(ProfileName) ? (dbUser?.Name ?? "Farmer") : ProfileName;

        Farms = _store.GetFarms();
        Batches = _store.GetBatches();
        Shipments = _store.GetShipments();

        if (Farms.Count > 0)
        {
            NewFarm = Farms[0].Name;
        }

        var query = (Q ?? string.Empty).Trim();
        FilteredBatches = Batches.Where(b =>
        {
            bool statusMatch = Status == "All" || b.Status.Equals(Status, StringComparison.OrdinalIgnoreCase);
            bool queryMatch = string.IsNullOrEmpty(query)
                || b.Code.Contains(query, StringComparison.OrdinalIgnoreCase)
                || b.Product.Contains(query, StringComparison.OrdinalIgnoreCase)
                || b.Farm.Contains(query, StringComparison.OrdinalIgnoreCase)
                || b.Category.Contains(query, StringComparison.OrdinalIgnoreCase);
            return statusMatch && queryMatch;
        }).ToList();

        FilteredShipments = Shipments.Where(s =>
            ShipmentStatus == "All" || s.Status.Equals(ShipmentStatus, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        TotalAcres = Farms.Sum(f => f.AreaAcres);
        ReadyBatches = Batches.Where(b => b.Status == "Ready for Dispatch").ToList();
        ReadyStockKg = ReadyBatches.Sum(b => b.QuantityKg);
        TotalVolumeKg = Batches.Sum(b => b.QuantityKg);
        PendingCount = Batches.Count(b => b.Status == "Inspection Pending");
        var gradeACount = Batches.Count(b => b.Grade.Contains("Grade A", StringComparison.OrdinalIgnoreCase));
        PassRate = Batches.Count > 0 ? (int)Math.Round((double)gradeACount / Batches.Count * 100.0) : 100;

        ActiveShipments = Shipments.Where(s => s.Status == "In Transit").ToList();
        InTransitKg = ActiveShipments.Sum(s => s.QuantityKg);

        if (!string.IsNullOrWhiteSpace(Qr))
        {
            QrBatch = Batches.FirstOrDefault(b => b.Code.Equals(Qr.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        ToastMessage = (TempData["ToastMsg"] as string) ?? (TempData["SuccessNotice"] as string);
    }

    public static string NormalizeSection(string? section)
    {
        return section?.ToLowerInvariant() switch
        {
            "batches" => "batches",
            "shipments" => "shipments",
            "profile" => "profile",
            _ => "dashboard"
        };
    }

    public static int GetJourneyStageIndex(string? status) => status switch
    {
        "Delivered" => 3,
        "In Transit" => 2,
        "Ready for Dispatch" => 1,
        _ => 0
    };
}
