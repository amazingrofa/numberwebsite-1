using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Razor Pages and SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// 2. Register AppDbContext for Dependency Injection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=numbers.db"));

// 3. Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("IpRateLimit", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,                  // Max 10 requests
                Window = TimeSpan.FromSeconds(10),  // Per 10 seconds
                SegmentsPerWindow = 2,
                QueueLimit = 0
            });
    });
});

var app = builder.Build();

// 4. Ensure SQLite database is created on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 5. Configure HTTP Pipeline Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter(); // Must be placed between UseRouting and Map endpoints

app.MapRazorPages();
app.MapHub<ViewCountHub>("/viewCountHub");

app.Run();