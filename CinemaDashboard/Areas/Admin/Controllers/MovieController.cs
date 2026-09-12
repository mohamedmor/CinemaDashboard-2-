using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Models.Enums;
using CinemaDashboard.Helpers;

namespace CinemaDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string UploadFolder = "movies";

        public MovieController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.DateTime)
                .ToListAsync();

            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie model)
        {
            ModelState.Remove(nameof(Movie.Category));
            ModelState.Remove(nameof(Movie.Cinema));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model.CategoryId, model.CinemaId, model.SelectedActorIds);
                return View(model);
            }

            if (model.MainImgFile != null && model.MainImgFile.Length > 0)
            {
                model.MainImg = await FileUploadHelper.SaveFileAsync(_env, model.MainImgFile, UploadFolder);
            }

            if (model.SubImageFiles != null)
            {
                foreach (var file in model.SubImageFiles.Where(f => f != null && f.Length > 0))
                {
                    var stored = await FileUploadHelper.SaveFileAsync(_env, file, UploadFolder);
                    model.SubImages.Add(new MovieImage { ImagePath = stored });
                }
            }

            if (model.SelectedActorIds != null)
            {
                foreach (var actorId in model.SelectedActorIds.Distinct())
                {
                    model.MovieActors.Add(new MovieActor { ActorId = actorId });
                }
            }

            _context.Movies.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.SubImages)
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            movie.SelectedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList();

            await PopulateDropdownsAsync(movie.CategoryId, movie.CinemaId, movie.SelectedActorIds);
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie model)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove(nameof(Movie.Category));
            ModelState.Remove(nameof(Movie.Cinema));

            var existing = await _context.Movies
                .Include(m => m.SubImages)
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model.CategoryId, model.CinemaId, model.SelectedActorIds);
                return View(model);
            }

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Price = model.Price;
            existing.Status = model.Status;
            existing.DateTime = model.DateTime;
            existing.CategoryId = model.CategoryId;
            existing.CinemaId = model.CinemaId;

            if (model.MainImgFile != null && model.MainImgFile.Length > 0)
            {
                FileUploadHelper.DeleteFile(_env, UploadFolder, existing.MainImg);
                existing.MainImg = await FileUploadHelper.SaveFileAsync(_env, model.MainImgFile, UploadFolder);
            }

            if (model.SubImageFiles != null && model.SubImageFiles.Any(f => f != null && f.Length > 0))
            {
                foreach (var file in model.SubImageFiles.Where(f => f != null && f.Length > 0))
                {
                    var stored = await FileUploadHelper.SaveFileAsync(_env, file, UploadFolder);
                    existing.SubImages.Add(new MovieImage { ImagePath = stored, MovieId = existing.Id });
                }
            }

            // Rebuild the cast list
            _context.MovieActors.RemoveRange(existing.MovieActors);
            if (model.SelectedActorIds != null)
            {
                foreach (var actorId in model.SelectedActorIds.Distinct())
                {
                    existing.MovieActors.Add(new MovieActor { MovieId = existing.Id, ActorId = actorId });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.SubImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            FileUploadHelper.DeleteFile(_env, UploadFolder, movie.MainImg);
            foreach (var img in movie.SubImages)
            {
                FileUploadHelper.DeleteFile(_env, UploadFolder, img.ImagePath);
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Movie/DeleteSubImage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubImage(int id, int movieId)
        {
            var img = await _context.MovieImages.FindAsync(id);
            if (img != null)
            {
                FileUploadHelper.DeleteFile(_env, UploadFolder, img.ImagePath);
                _context.MovieImages.Remove(img);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = movieId });
        }

        private async Task PopulateDropdownsAsync(int? selectedCategoryId = null, int? selectedCinemaId = null, System.Collections.Generic.List<int> selectedActorIds = null)
        {
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedCategoryId);
            ViewBag.Cinemas = new SelectList(await _context.Cinemas.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedCinemaId);

            var actors = await _context.Actors.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Actors = actors;
            ViewBag.SelectedActorIds = selectedActorIds ?? new System.Collections.Generic.List<int>();

            ViewBag.StatusList = new SelectList(System.Enum.GetValues(typeof(MovieStatus)));
        }
    }
}
