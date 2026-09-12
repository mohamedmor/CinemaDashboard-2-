using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Helpers;

namespace CinemaDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string UploadFolder = "actors";

        public ActorController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.OrderBy(a => a.Name).ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actor model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                model.Img = await FileUploadHelper.SaveFileAsync(_env, model.ImgFile, UploadFolder);
            }

            _context.Actors.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Actor model)
        {
            if (id != model.Id) return NotFound();

            var existing = await _context.Actors.FindAsync(id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Img = existing.Img;
                return View(model);
            }

            existing.Name = model.Name;

            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                FileUploadHelper.DeleteFile(_env, UploadFolder, existing.Img);
                existing.Img = await FileUploadHelper.SaveFileAsync(_env, model.ImgFile, UploadFolder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            FileUploadHelper.DeleteFile(_env, UploadFolder, actor.Img);

            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
