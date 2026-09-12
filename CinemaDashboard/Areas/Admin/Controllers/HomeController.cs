using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaDashboard.Data;

namespace CinemaDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MoviesCount = await _context.Movies.CountAsync();
            ViewBag.CinemasCount = await _context.Cinemas.CountAsync();
            ViewBag.ActorsCount = await _context.Actors.CountAsync();
            ViewBag.CategoriesCount = await _context.Categories.CountAsync();

            var latestMovies = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.DateTime)
                .Take(5)
                .ToListAsync();

            return View(latestMovies);
        }
    }
}
