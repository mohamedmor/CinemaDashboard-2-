using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using CinemaDashboard.Helpers;
using CinemaDashboard.Models.Cart;

namespace CinemaDashboard.Services
{
    public class CartService
    {
        private const string CartSessionKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public List<CartItem> GetCart()
        {
            return Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
        }

        public void SaveCart(List<CartItem> cart)
        {
            Session.SetObjectAsJson(CartSessionKey, cart);
        }

        public void AddToCart(CartItem newItem)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.MovieId == newItem.MovieId);

            if (existing != null)
            {
                existing.Quantity += newItem.Quantity;
            }
            else
            {
                cart.Add(newItem);
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int movieId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.MovieId == movieId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
            SaveCart(cart);
        }

        public void RemoveFromCart(int movieId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.MovieId == movieId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }

        public int GetItemCount()
        {
            return GetCart().Sum(i => i.Quantity);
        }

        public decimal GetTotal()
        {
            return GetCart().Sum(i => i.LineTotal);
        }
    }
}
