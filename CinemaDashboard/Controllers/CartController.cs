using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CinemaDashboard.Data;
using CinemaDashboard.Models.Cart;
using CinemaDashboard.Services;

namespace CinemaDashboard.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.Total = _cartService.GetTotal();
            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int movieId, int quantity = 1)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound();

            if (quantity < 1) quantity = 1;

            _cartService.AddToCart(new CartItem
            {
                MovieId = movie.Id,
                MovieName = movie.Name,
                MainImg = movie.MainImg,
                UnitPrice = movie.Price,
                Quantity = quantity
            });

            TempData["CartMessage"] = $"\"{movie.Name}\" added to cart.";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int movieId, int quantity)
        {
            _cartService.UpdateQuantity(movieId, quantity);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int movieId)
        {
            _cartService.RemoveFromCart(movieId);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }
    }
}
