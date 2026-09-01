using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Hubs;

namespace WebApplication2.Pages
{
    [EnableRateLimiting("IpRateLimit")]
    public class NumberModel : PageModel
    {

        private readonly AppDbContext _db;
        private readonly IHubContext<ViewCountHub> _hubContext;

        public NumberModel(AppDbContext db, IHubContext<ViewCountHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        public long NumberId { get; set; }
        public long ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            NumberId = id;
            string cookieKey = $"Viewed_Number_{id}";
            bool alreadyViewed = Request.Cookies.ContainsKey(cookieKey);

            var page = await _db.NumberPages.FirstOrDefaultAsync(n => n.Id == id);

            if (page == null)
            {
                page = new NumberPage { Id = id, ViewCount = 1, CreatedAt = DateTime.UtcNow };
                _db.NumberPages.Add(page);
                await _db.SaveChangesAsync();

                Response.Cookies.Append(cookieKey, "true", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(24),
                    HttpOnly = true
                });
            }
            else
            {
                if (!alreadyViewed)
                {
                    page.ViewCount++;
                    await _db.SaveChangesAsync();

                    Response.Cookies.Append(cookieKey, "true", new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddHours(24),
                        HttpOnly = true
                    });

                    await _hubContext.Clients.Group(id.ToString()).SendAsync("UpdateCount", page.ViewCount);
                }
            }

            ViewCount = page.ViewCount;
            CreatedAt = page.CreatedAt;

            return Page();
        }
    }
}