using System.Text.Json.Serialization;

namespace FreshTrace.Models;

public sealed class AppUser
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "farmer";
    public string Status { get; set; } = "active";
    public bool Verified { get; set; } = true;
    public string Facility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class FarmParcel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public double AreaAcres { get; set; }
    public string Certification { get; set; } = "Certified Organic";
    public List<string> Crops { get; set; } = new();
}

public sealed class HarvestBatch
{
    public string Code { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Category { get; set; } = "Fresh Fruits";
    public string Farm { get; set; } = string.Empty;
    public string HarvestDate { get; set; } = string.Empty;
    public int QuantityKg { get; set; }
    public string Grade { get; set; } = "Grade A (Export)";
    public string TempC { get; set; } = "3.5 °C";
    public string Status { get; set; } = "Ready for Dispatch";
    public string Badge { get; set; } = "ready";
    public string QrToken { get; set; } = string.Empty;
}

public sealed class ColdChainShipment
{
    public string Id { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int QuantityKg { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public string TempC { get; set; } = string.Empty;
    public string Eta { get; set; } = string.Empty;
    public string Status { get; set; } = "In Transit";
    public string Badge { get; set; } = "transit";
    public string DispatchedAt { get; set; } = string.Empty;
}

public sealed class ProducerProfile
{
    public string FarmerName { get; set; } = "Ramesh Patel";
    public string Phone { get; set; } = "+91 98160 44219";
    public string FarmName { get; set; } = "Green Valley Organic Farm";
    public string FarmArea { get; set; } = "15.5 Acres";
    public string FarmDistrict { get; set; } = "Shimla District, Himachal Pradesh";
    public string FarmCoords { get; set; } = "31.1048° N, 77.1734° E";
    public string GeoStatusMsg { get; set; } = string.Empty;
}

public sealed class FreshTraceState
{
    public List<AppUser> Users { get; set; } = new();
    public List<FarmParcel> Farms { get; set; } = new();
    public List<HarvestBatch> Batches { get; set; } = new();
    public List<ColdChainShipment> Shipments { get; set; } = new();
    public Dictionary<string, ProducerProfile> ProfilesByEmail { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record RoleStatCard(string Title, string Value, string Sub);

public sealed record RoleTableRow(string Code, string Col2, string Col3, string Col4, string Col5, string Status, string Badge);

public sealed class RoleDashboardConfig
{
    public string Badge { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string CtaLabel { get; init; } = string.Empty;
    public string CtaMessage { get; init; } = string.Empty;
    public List<RoleStatCard> Stats { get; init; } = new();
    public string TableTitle { get; init; } = string.Empty;
    public List<string> Columns { get; init; } = new();
    public List<RoleTableRow> Rows { get; init; } = new();
}
