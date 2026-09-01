using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

namespace WebApplication2.Pages
{
    public class TopModel : PageModel
    {
        private readonly AppDbContext _db;

        public TopModel(AppDbContext db)
        {
            _db = db;
        }

        public List<NumberPage> TopNumbers { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Query the top 100 most viewed numbers ordered by ViewCount
            TopNumbers = await _db.NumberPages
                .AsNoTracking()
                .OrderByDescending(n => n.ViewCount)
                .ThenBy(n => n.Id)
                .Take(100)
                .ToListAsync();
        }
    }
}