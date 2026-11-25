using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FreshTrace.Models;

namespace FreshTrace.Services;

/// <summary>
/// Thread-safe application state and domain service for FreshTrace (users, farms, batches, shipments, traceability).
/// Persists live state to data/freshtrace-state.json.
/// </summary>
public sealed class FreshTraceDataService
{
    private readonly object _gate = new();
    private readonly string _stateFilePath;
    private readonly FreshTraceState _state;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Dictionary<string, string> DefaultTraceableBatches = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BATCH-2026-APL-098"] = "Royal Delicious Apples • Harvested at Green Valley Farm (Shimla, HP) on Nov 10, 2026 • Cold-chain transit maintained at 3.4 °C • Verified Grade A (98% Freshness).",
        ["BATCH-2026-TOM-441"] = "Organic Vine Tomatoes • Harvested at Pine Ridge Estate (Solan, HP) on Nov 11, 2026 • Cold-chain transit maintained at 4.1 °C • Verified Pesticide-Free (95% Freshness).",
        ["BATCH-2026-CRT-204"] = "Himalayan Baby Carrots • Harvested at Highland Organic Co-op on Nov 14, 2026 • Direct chilled dispatch at 3.1 °C • Verified Organic (99% Freshness)."
    };

    private static readonly Dictionary<string, RoleDashboardConfig> RoleConfigs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["farmer"] = new RoleDashboardConfig
        {
            Badge = "Farmer Portal",
            Subtitle = "Manage your harvest batches, farm parcels, and dispatch readiness.",
            CtaLabel = "+ Create Batch",
            CtaMessage = "New harvest batch registration initiated.",
            Stats =
            [
                new("Registered Farms", "2 Farms", "Green Valley & Pine Ridge"),
                new("Active Batches", "8 Batches", "Tracked in system"),
                new("In Stock", "14,200 kg", "Ready for dispatch"),
                new("Pass Rate", "100%", "Grade A quality")
            ],
            TableTitle = "Recent Harvest Batches",
            Columns = ["Batch Code", "Product", "Origin", "Harvest Date", "Quantity", "Status"],
            Rows =
            [
                new("BATCH-2026-APL-098", "Royal Delicious Apples", "Green Valley Farm", "Nov 10, 2026", "2,400 kg", "Ready for Dispatch", "ready"),
                new("BATCH-2026-TOM-441", "Organic Vine Tomatoes", "Pine Ridge Estate", "Nov 11, 2026", "1,200 kg", "In Transit", "transit"),
                new("BATCH-2026-CRT-204", "Himalayan Baby Carrots", "Green Valley Farm", "Nov 14, 2026", "950 kg", "Ready for Dispatch", "ready")
            ]
        },
        ["admin"] = new RoleDashboardConfig
        {
            Badge = "Admin Command Center",
            Subtitle = "Oversee supply chain compliance, registered stakeholders, and audit integrity.",
            CtaLabel = "+ Add Stakeholder",
            CtaMessage = "Stakeholder onboarding workflow opened.",
            Stats =
            [
                new("Total Stakeholders", "148 Users", "Farmers, distributors & retailers"),
                new("Active Batches", "312 Batches", "Live across 5 regions"),
                new("Compliance Score", "99.4%", "SDG 12 & IEEE 830 verified"),
                new("Open Alerts", "2 Alerts", "Minor cold-chain variance")
            ],
            TableTitle = "System Entities & Compliance Overview",
            Columns = ["Record ID", "Entity", "Role", "Region", "Last Audit", "Status"],
            Rows =
            [
                new("ENT-FRM-101", "Green Valley Organic Farm", "Farmer", "Himachal Pradesh", "Nov 16, 2026", "Verified", "ready"),
                new("ENT-DST-204", "ColdChain Reefer Fleet #04", "Distributor", "North Corridor", "Nov 17, 2026", "Active Transit", "transit"),
                new("ENT-RTL-309", "FreshMart Metro Superstore", "Retailer", "New Delhi", "Nov 17, 2026", "Verified", "ready"),
                new("ENT-ALT-012", "Solan Cold Storage Unit B", "Distributor", "Solan, HP", "Nov 18, 2026", "Temp Warning", "warn")
            ]
        },
        ["retailer"] = new RoleDashboardConfig
        {
            Badge = "Retailer Storefront Hub",
            Subtitle = "Track incoming deliveries, shelf freshness scores, and store inventory.",
            CtaLabel = "+ Log Receipt",
            CtaMessage = "Incoming shipment verification scanner opened.",
            Stats =
            [
                new("Incoming Deliveries", "5 Shipments", "2 arriving today"),
                new("Fresh Stock On Shelf", "6,850 kg", "Avg. 96% freshness"),
                new("Expiring Soon", "3 Lots", "Prioritize markdown"),
                new("Waste Reduced", "34%", "Month-over-month")
            ],
            TableTitle = "Store Inventory & Incoming Lots",
            Columns = ["Batch Code", "Product", "Supplier", "Freshness", "Shelf Stock", "Status"],
            Rows =
            [
                new("BATCH-2026-APL-098", "Royal Delicious Apples", "Green Valley Farm", "98% Fresh", "1,120 kg", "On Shelf", "ready"),
                new("BATCH-2026-TOM-441", "Organic Vine Tomatoes", "ColdChain Fleet #04", "94% Fresh", "640 kg", "Arriving Today", "transit"),
                new("BATCH-2026-SPN-112", "Farm-Fresh Baby Spinach", "Nashik Greens Co-op", "86% Fresh", "185 kg", "Sell Soon", "warn")
            ]
        },
        ["distributor"] = new RoleDashboardConfig
        {
            Badge = "Distributor Cold-Chain Hub",
            Subtitle = "Monitor refrigerated fleet telemetry, active routes, and warehouse handoffs.",
            CtaLabel = "+ Dispatch Shipment",
            CtaMessage = "New cold-chain dispatch route created.",
            Stats =
            [
                new("Active Fleet", "12 Trucks", "100% GPS online"),
                new("Avg. Reefer Temp", "3.8 °C", "Optimal 2–4 °C band"),
                new("In-Transit Load", "28,400 kg", "Across 6 active routes"),
                new("On-Time Delivery", "97.8%", "Last 30 days")
            ],
            TableTitle = "Live Cold-Chain Shipments",
            Columns = ["Shipment ID", "Batch & Product", "Route", "Reefer Temp", "ETA", "Status"],
            Rows =
            [
                new("SHP-8841", "Royal Delicious Apples (APL-098)", "Shimla → Delhi Hub", "3.4 °C", "2h 15m", "In Transit", "transit"),
                new("SHP-8845", "Organic Vine Tomatoes (TOM-441)", "Solan → Chandigarh", "4.1 °C", "Delivered", "Delivered", "ready"),
                new("SHP-8849", "Baby Spinach Lots (SPN-112)", "Nashik → Mumbai Hub", "5.2 °C", "4h 40m", "Check Cooling", "warn")
            ]
        },
        ["consumer"] = new RoleDashboardConfig
        {
            Badge = "Consumer Traceability Portal",
            Subtitle = "Verify food origin, inspect farm-to-fork journeys, and check live freshness scores.",
            CtaLabel = "Scan QR Code",
            CtaMessage = "Camera QR scanner ready. You can also enter a batch code below to trace provenance.",
            Stats =
            [
                new("Verified Scans", "14 Items", "100% authentic origin"),
                new("Avg. Freshness", "97 / 100", "Harvested within 48h"),
                new("Cold-Chain Safe", "100%", "Zero temp breaches"),
                new("Trusted Farms", "6 Farms", "Certified organic")
            ],
            TableTitle = "Recently Verified Produce",
            Columns = ["Batch Code", "Product", "Origin Farm", "Harvested", "Freshness", "Verification"],
            Rows =
            [
                new("BATCH-2026-APL-098", "Royal Delicious Apples", "Green Valley Farm, Shimla", "Nov 10, 2026", "98% Fresh", "Verified Origin", "ready"),
                new("BATCH-2026-TOM-441", "Organic Vine Tomatoes", "Pine Ridge Estate, Solan", "Nov 11, 2026", "95% Fresh", "Verified Origin", "ready"),
                new("BATCH-2026-CRT-204", "Himalayan Baby Carrots", "Highland Organic Co-op", "Nov 14, 2026", "99% Fresh", "Verified Origin", "ready")
            ]
        }
    };

    public FreshTraceDataService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "data");
        Directory.CreateDirectory(dataDir);
        _stateFilePath = Path.Combine(dataDir, "freshtrace-state.json");
        _state = LoadOrSeedState();
    }

    private FreshTraceState LoadOrSeedState()
    {
        FreshTraceState? loaded = null;
        if (File.Exists(_stateFilePath))
        {
            try
            {
                var json = File.ReadAllText(_stateFilePath);
                loaded = JsonSerializer.Deserialize<FreshTraceState>(json, JsonOptions);
            }
            catch
            {
                loaded = null;
            }
        }

        var state = loaded ?? new FreshTraceState();
        EnsureSeedData(state);
        SaveStateUnsafe(state);
        return state;
    }

    private static void EnsureSeedData(FreshTraceState state)
    {
        var defaultUsers = new List<AppUser>
        {
            new()
            {
                UserId = "U0002",
                Name = "Ramesh Patel",
                Email = "farmer.ramesh@greenvalley.com",
                PasswordHash = HashPassword("Password123!"),
                Phone = "+91 98160 44219",
                Role = "farmer",
                Facility = "Green Valley Organic Farm"
            },
            new()
            {
                UserId = "U0001",
                Name = "Chief Administrator",
                Email = "admin@freshtrace.org",
                PasswordHash = HashPassword("Password123!"),
                Phone = "+91 98100 00001",
                Role = "admin",
                Facility = "FreshTrace Central Operations"
            },
            new()
            {
                UserId = "U0005",
                Name = "Ananya Sharma",
                Email = "retailer@freshmart.com",
                PasswordHash = HashPassword("Password123!"),
                Phone = "+91 98201 55412",
                Role = "retailer",
                Facility = "FreshMart Metro Superstore"
            },
            new()
            {
                UserId = "U0004",
                Name = "Vikram Singh",
                Email = "distributor@coldchain.com",
                PasswordHash = HashPassword("Password123!"),
                Phone = "+91 98711 33098",
                Role = "distributor",
                Facility = "ColdChain Reefer Fleet #04"
            },
            new()
            {
                UserId = "U0007",
                Name = "Priya Nair",
                Email = "consumer@freshtrace.org",
                PasswordHash = HashPassword("Password123!"),
                Phone = "+91 98450 77821",
                Role = "consumer",
                Facility = "Consumer Traceability Portal"
            }
        };

        var existingEmails = new HashSet<string>(state.Users.Select(u => u.Email), StringComparer.OrdinalIgnoreCase);
        foreach (var demo in defaultUsers)
        {
            if (!existingEmails.Contains(demo.Email))
            {
                state.Users.Add(demo);
            }
        }

        if (state.Farms.Count == 0)
        {
            state.Farms.AddRange(
            [
                new FarmParcel
                {
                    Id = "FARM-01",
                    Name = "Green Valley Organic Farm",
                    District = "Shimla District, Himachal Pradesh",
                    AreaAcres = 15.5,
                    Certification = "Certified Organic",
                    Crops = ["Royal Delicious Apples", "Baby Carrots", "Organic Pears"]
                },
                new FarmParcel
                {
                    Id = "FARM-02",
                    Name = "Pine Ridge Estate",
                    District = "Solan Valley, Himachal Pradesh",
                    AreaAcres = 8.5,
                    Certification = "GAP Certified",
                    Crops = ["Organic Vine Tomatoes", "Strawberries", "Baby Spinach"]
                }
            ]);
        }

        if (state.Batches.Count == 0)
        {
            state.Batches.AddRange(
            [
                new HarvestBatch
                {
                    Code = "BATCH-2026-APL-098",
                    Product = "Royal Delicious Apples",
                    Category = "Fresh Fruits",
                    Farm = "Green Valley Organic Farm",
                    HarvestDate = "2026-11-10",
                    QuantityKg = 2400,
                    Grade = "Grade A (Export)",
                    TempC = "3.4 °C",
                    Status = "Ready for Dispatch",
                    Badge = "ready",
                    QrToken = "QR-FT-881923-APL"
                },
                new HarvestBatch
                {
                    Code = "BATCH-2026-TOM-441",
                    Product = "Organic Vine Tomatoes",
                    Category = "Vegetables",
                    Farm = "Pine Ridge Estate",
                    HarvestDate = "2026-11-11",
                    QuantityKg = 1200,
                    Grade = "Grade A+ (Organic)",
                    TempC = "8.5 °C",
                    Status = "In Transit",
                    Badge = "transit",
                    QrToken = "QR-FT-441029-TOM"
                },
                new HarvestBatch
                {
                    Code = "BATCH-2026-CRT-204",
                    Product = "Himalayan Baby Carrots",
                    Category = "Vegetables",
                    Farm = "Green Valley Organic Farm",
                    HarvestDate = "2026-11-14",
                    QuantityKg = 950,
                    Grade = "Grade A (Export)",
                    TempC = "3.1 °C",
                    Status = "Ready for Dispatch",
                    Badge = "ready",
                    QrToken = "QR-FT-204811-CRT"
                },
                new HarvestBatch
                {
                    Code = "BATCH-2026-STR-315",
                    Product = "Highland Strawberries",
                    Category = "Berries",
                    Farm = "Pine Ridge Estate",
                    HarvestDate = "2026-11-17",
                    QuantityKg = 420,
                    Grade = "Grade A+ (Organic)",
                    TempC = "2.4 °C",
                    Status = "Inspection Pending",
                    Badge = "warn",
                    QrToken = "QR-FT-315902-STR"
                },
                new HarvestBatch
                {
                    Code = "BATCH-2026-PER-119",
                    Product = "Bartlett Mountain Pears",
                    Category = "Fresh Fruits",
                    Farm = "Green Valley Organic Farm",
                    HarvestDate = "2026-11-06",
                    QuantityKg = 1650,
                    Grade = "Grade A (Export)",
                    TempC = "3.8 °C",
                    Status = "Delivered",
                    Badge = "ready",
                    QrToken = "QR-FT-119480-PER"
                }
            ]);
        }

        if (state.Shipments.Count == 0)
        {
            state.Shipments.AddRange(
            [
                new ColdChainShipment
                {
                    Id = "SHP-8841",
                    BatchCode = "BATCH-2026-TOM-441",
                    Product = "Organic Vine Tomatoes",
                    QuantityKg = 1200,
                    Origin = "Pine Ridge Estate, Solan",
                    Destination = "Delhi Cold-Chain Hub",
                    Carrier = "ColdChain Fleet #04",
                    Vehicle = "HP-01-EE-4501",
                    TempC = "4.1 °C",
                    Eta = "2h 15m",
                    Status = "In Transit",
                    Badge = "transit",
                    DispatchedAt = "Today, 11:20 AM"
                },
                new ColdChainShipment
                {
                    Id = "SHP-8832",
                    BatchCode = "BATCH-2026-PER-119",
                    Product = "Bartlett Mountain Pears",
                    QuantityKg = 1650,
                    Origin = "Green Valley Organic Farm, Shimla",
                    Destination = "FreshMart Metro Hub, Chandigarh",
                    Carrier = "GreenLine Reefer #02",
                    Vehicle = "HP-12-BB-3209",
                    TempC = "3.8 °C",
                    Eta = "Delivered",
                    Status = "Delivered",
                    Badge = "ready",
                    DispatchedAt = "Nov 08, 2026"
                }
            ]);
        }
    }

    private void SaveStateUnsafe(FreshTraceState state)
    {
        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            File.WriteAllText(_stateFilePath, json);
        }
        catch
        {
            // Ignore write errors in restricted environments while keeping in-memory state consistent
        }
    }

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2${Convert.ToHexString(salt)}${Convert.ToHexString(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;

        if (!storedHash.StartsWith("pbkdf2$", StringComparison.Ordinal))
        {
            return storedHash == password;
        }

        var parts = storedHash.Split('$');
        if (parts.Length != 3) return false;

        byte[] salt = Convert.FromHexString(parts[1]);
        byte[] expected = Convert.FromHexString(parts[2]);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    public AppUser? Authenticate(string email, string password)
    {
        var cleanEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        lock (_gate)
        {
            var user = _state.Users.FirstOrDefault(u => u.Email.Equals(cleanEmail, StringComparison.OrdinalIgnoreCase));
            if (user is null) return null;
            return VerifyPassword(password ?? string.Empty, user.PasswordHash) ? user : null;
        }
    }

    public (bool Success, string? Error, AppUser? User) RegisterUser(string name, string email, string password, string phone, string role)
    {
        var cleanEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        var cleanName = (name ?? string.Empty).Trim();
        var cleanPhone = (phone ?? string.Empty).Trim();
        var cleanRole = string.IsNullOrWhiteSpace(role) ? "farmer" : role.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(cleanName) || string.IsNullOrWhiteSpace(cleanEmail))
        {
            return (false, "Name and email address are required.", null);
        }

        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            return (false, "Password should be at least 6 characters long.", null);
        }

        lock (_gate)
        {
            if (_state.Users.Any(u => u.Email.Equals(cleanEmail, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "An account with this email address already exists.", null);
            }

            var newUser = new AppUser
            {
                UserId = $"U{Random.Shared.Next(1000, 9999)}",
                Name = cleanName,
                Email = cleanEmail,
                PasswordHash = HashPassword(password),
                Phone = cleanPhone,
                Role = cleanRole,
                Status = "active",
                Verified = true,
                Facility = $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cleanRole)} Operations",
                CreatedAt = DateTime.UtcNow
            };

            _state.Users.Add(newUser);
            _state.ProfilesByEmail[cleanEmail] = new ProducerProfile
            {
                FarmerName = cleanName,
                Phone = string.IsNullOrWhiteSpace(cleanPhone) ? "+91 98160 44219" : cleanPhone
            };

            SaveStateUnsafe(_state);
            return (true, null, newUser);
        }
    }

    public AppUser? FindUserByEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        lock (_gate)
        {
            return _state.Users.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public ProducerProfile GetProfile(string? email, string? fallbackName = null, string? fallbackPhone = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(email) && _state.ProfilesByEmail.TryGetValue(email, out var profile))
            {
                return new ProducerProfile
                {
                    FarmerName = profile.FarmerName,
                    Phone = profile.Phone,
                    FarmName = profile.FarmName,
                    FarmArea = profile.FarmArea,
                    FarmDistrict = profile.FarmDistrict,
                    FarmCoords = profile.FarmCoords,
                    GeoStatusMsg = profile.GeoStatusMsg
                };
            }

            var user = string.IsNullOrWhiteSpace(email)
                ? null
                : _state.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            return new ProducerProfile
            {
                FarmerName = fallbackName ?? user?.Name ?? "Ramesh Patel",
                Phone = fallbackPhone ?? user?.Phone ?? "+91 98160 44219"
            };
        }
    }

    public void UpdateProfile(string email, ProducerProfile updated)
    {
        if (string.IsNullOrWhiteSpace(email)) return;
        lock (_gate)
        {
            var current = _state.ProfilesByEmail.TryGetValue(email, out var existing)
                ? existing
                : new ProducerProfile();

            if (!string.IsNullOrWhiteSpace(updated.FarmerName))
                current.FarmerName = updated.FarmerName.Trim();
            if (updated.Phone is not null)
                current.Phone = updated.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(updated.FarmName))
                current.FarmName = updated.FarmName.Trim();
            if (!string.IsNullOrWhiteSpace(updated.FarmArea))
                current.FarmArea = updated.FarmArea.Trim();
            if (!string.IsNullOrWhiteSpace(updated.FarmDistrict))
                current.FarmDistrict = updated.FarmDistrict.Trim();
            if (!string.IsNullOrWhiteSpace(updated.FarmCoords))
                current.FarmCoords = updated.FarmCoords.Trim();
            if (!string.IsNullOrWhiteSpace(updated.GeoStatusMsg))
                current.GeoStatusMsg = updated.GeoStatusMsg.Trim();

            _state.ProfilesByEmail[email] = current;

            var user = _state.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user is not null)
            {
                user.Name = current.FarmerName;
                user.Phone = current.Phone;
            }

            SaveStateUnsafe(_state);
        }
    }

    public List<FarmParcel> GetFarms()
    {
        lock (_gate)
        {
            return _state.Farms.Select(f => new FarmParcel
            {
                Id = f.Id,
                Name = f.Name,
                District = f.District,
                AreaAcres = f.AreaAcres,
                Certification = f.Certification,
                Crops = new List<string>(f.Crops)
            }).ToList();
        }
    }

    public List<HarvestBatch> GetBatches()
    {
        lock (_gate)
        {
            return _state.Batches.Select(b => new HarvestBatch
            {
                Code = b.Code,
                Product = b.Product,
                Category = b.Category,
                Farm = b.Farm,
                HarvestDate = b.HarvestDate,
                QuantityKg = b.QuantityKg,
                Grade = b.Grade,
                TempC = b.TempC,
                Status = b.Status,
                Badge = b.Badge,
                QrToken = b.QrToken
            }).ToList();
        }
    }

    public List<ColdChainShipment> GetShipments()
    {
        lock (_gate)
        {
            return _state.Shipments.Select(s => new ColdChainShipment
            {
                Id = s.Id,
                BatchCode = s.BatchCode,
                Product = s.Product,
                QuantityKg = s.QuantityKg,
                Origin = s.Origin,
                Destination = s.Destination,
                Carrier = s.Carrier,
                Vehicle = s.Vehicle,
                TempC = s.TempC,
                Eta = s.Eta,
                Status = s.Status,
                Badge = s.Badge,
                DispatchedAt = s.DispatchedAt
            }).ToList();
        }
    }

    public HarvestBatch? CreateBatch(string product, string category, string farm, string harvestDate, int quantityKg, string grade, double tempC, string status)
    {
        var cleanProduct = (product ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(cleanProduct) || quantityKg <= 0) return null;

        var lettersOnly = new string(cleanProduct.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        var prefix = lettersOnly.Length >= 3 ? lettersOnly[..3] : lettersOnly.PadRight(3, 'X');
        var randNum = Random.Shared.Next(100, 1000);
        var tokenNum = Random.Shared.Next(100000, 1000000);
        var code = $"BATCH-2026-{prefix}-{randNum}";

        lock (_gate)
        {
            var resolvedFarm = string.IsNullOrWhiteSpace(farm)
                ? (_state.Farms.FirstOrDefault()?.Name ?? "Green Valley Organic Farm")
                : farm.Trim();

            var created = new HarvestBatch
            {
                Code = code,
                Product = cleanProduct,
                Category = string.IsNullOrWhiteSpace(category) ? "Fresh Fruits" : category.Trim(),
                Farm = resolvedFarm,
                HarvestDate = string.IsNullOrWhiteSpace(harvestDate) ? "2026-11-18" : harvestDate.Trim(),
                QuantityKg = quantityKg,
                Grade = string.IsNullOrWhiteSpace(grade) ? "Grade A (Export)" : grade.Trim(),
                TempC = $"{tempC.ToString("0.0", CultureInfo.InvariantCulture)} °C",
                Status = string.IsNullOrWhiteSpace(status) ? "Ready for Dispatch" : status.Trim(),
                Badge = status == "Ready for Dispatch" ? "ready" : "warn",
                QrToken = $"QR-FT-{tokenNum}-{prefix}"
            };

            _state.Batches.Insert(0, created);
            SaveStateUnsafe(_state);
            return created;
        }
    }

    public bool DispatchBatch(string batchCode)
    {
        if (string.IsNullOrWhiteSpace(batchCode)) return false;

        lock (_gate)
        {
            var target = _state.Batches.FirstOrDefault(b => b.Code.Equals(batchCode.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target is null) return false;

            target.Status = "In Transit";
            target.Badge = "transit";

            var existingShipment = _state.Shipments.FirstOrDefault(s => s.BatchCode.Equals(target.Code, StringComparison.OrdinalIgnoreCase));
            if (existingShipment is not null)
            {
                existingShipment.Status = "In Transit";
                existingShipment.Badge = "transit";
                existingShipment.Eta = "3h 40m";
                existingShipment.DispatchedAt = "Just now";
            }
            else
            {
                var shpNum = Random.Shared.Next(8850, 8990);
                _state.Shipments.Insert(0, new ColdChainShipment
                {
                    Id = $"SHP-{shpNum}",
                    BatchCode = target.Code,
                    Product = target.Product,
                    QuantityKg = target.QuantityKg,
                    Origin = target.Farm,
                    Destination = "Delhi Cold-Chain Hub",
                    Carrier = "ColdChain Fleet #04",
                    Vehicle = "HP-01-EE-4501",
                    TempC = target.TempC,
                    Eta = "3h 40m",
                    Status = "In Transit",
                    Badge = "transit",
                    DispatchedAt = "Just now"
                });
            }

            SaveStateUnsafe(_state);
            return true;
        }
    }

    public bool MarkShipmentDelivered(string shipmentId, string batchCode)
    {
        if (string.IsNullOrWhiteSpace(shipmentId)) return false;

        lock (_gate)
        {
            var shipment = _state.Shipments.FirstOrDefault(s => s.Id.Equals(shipmentId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (shipment is null) return false;

            shipment.Status = "Delivered";
            shipment.Badge = "ready";
            shipment.Eta = "Delivered";

            var batch = _state.Batches.FirstOrDefault(b => b.Code.Equals(batchCode.Trim(), StringComparison.OrdinalIgnoreCase));
            if (batch is not null)
            {
                batch.Status = "Delivered";
                batch.Badge = "ready";
            }

            SaveStateUnsafe(_state);
            return true;
        }
    }

    public FarmParcel? AddFarm(string name, string district, double areaAcres, string certification, string cropsCsv)
    {
        var cleanName = (name ?? string.Empty).Trim();
        var cleanDistrict = (district ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(cleanName) || string.IsNullOrEmpty(cleanDistrict) || areaAcres <= 0)
            return null;

        var crops = (cropsCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

        if (crops.Count == 0)
        {
            crops.Add("Seasonal Produce");
        }

        lock (_gate)
        {
            var created = new FarmParcel
            {
                Id = $"FARM-{(_state.Farms.Count + 1):00}",
                Name = cleanName,
                District = cleanDistrict,
                AreaAcres = areaAcres,
                Certification = string.IsNullOrWhiteSpace(certification) ? "Certified Organic" : certification.Trim(),
                Crops = crops
            };

            _state.Farms.Add(created);
            SaveStateUnsafe(_state);
            return created;
        }
    }

    public RoleDashboardConfig GetRoleDashboard(string role)
    {
        var key = (role ?? "farmer").Trim().ToLowerInvariant();
        return RoleConfigs.TryGetValue(key, out var cfg) ? cfg : RoleConfigs["farmer"];
    }

    public (bool Found, string Message) LookupTraceability(string? batchCode)
    {
        var key = (batchCode ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(key))
        {
            return (false, "Please enter a batch code (e.g. BATCH-2026-APL-098).");
        }

        if (DefaultTraceableBatches.TryGetValue(key, out var staticSummary))
        {
            return (true, staticSummary);
        }

        lock (_gate)
        {
            var batch = _state.Batches.FirstOrDefault(b => b.Code.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (batch is not null)
            {
                return (true, $"{batch.Product} • Harvested at {batch.Farm} on {FormatDisplayDate(batch.HarvestDate)} • Storage maintained at {batch.TempC} • {batch.Grade} ({batch.Status}).");
            }
        }

        return (false, $"No provenance record found for \"{key}\". Try BATCH-2026-APL-098, BATCH-2026-TOM-441, or BATCH-2026-CRT-204.");
    }

    public static string FormatDisplayDate(string? isoDate)
    {
        if (string.IsNullOrWhiteSpace(isoDate)) return "—";
        if (DateTime.TryParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
        }
        return isoDate;
    }

    public static string GetInitials(string? name)
    {
        var parts = (name ?? "Farmer").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "FR";
        if (parts.Length == 1) return parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant();
        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }

    public static string GetBadgeCssClass(string? badge) => badge switch
    {
        "transit" => "status-badge-transit",
        "warn" => "status-badge-warn",
        _ => "status-badge-ready"
    };
}
