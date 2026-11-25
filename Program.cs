using FreshTrace.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Persist data protection keys inside workspace data/keys when possible
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "data", "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("FreshTrace");

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
        options.Cookie.Name = "FreshTrace.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<FreshTraceDataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Lightweight C# API endpoints for programmatic traceability lookup and health checks
app.MapGet("/health", () => Results.Ok(new { status = "healthy", platform = "FreshTrace C# ASP.NET Core" }));

app.MapGet("/api/trace/{batchCode}", (string batchCode, FreshTraceDataService store) =>
{
    var result = store.LookupTraceability(batchCode);
    return Results.Ok(new
    {
        batchCode = batchCode.Trim().ToUpperInvariant(),
        found = result.Found,
        summary = result.Message
    });
});

app.Run();
