using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Models.Cart;
using CinemaDashboard.Services;

namespace CinemaDashboard.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CheckoutController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: /Checkout
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Cart = cart;
            ViewBag.Total = _cartService.GetTotal();
            return View(new CheckoutViewModel());
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();

            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                ViewBag.Total = _cartService.GetTotal();
                return View("Index", model);
            }

            var order = new Order
            {
                CustomerName = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                TotalPrice = cart.Sum(i => i.LineTotal)
            };

            foreach (var item in cart)
            {
                order.Items.Add(new OrderItem
                {
                    MovieId = item.MovieId,
                    MovieName = item.MovieName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _cartService.ClearCart();

            return RedirectToAction(nameof(Confirmation), new { id = order.Id });
        }

        // GET: /Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
