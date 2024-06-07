using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Project_WebMVC.Models;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;

namespace Project_WebMVC.Controllers
{
    public class ShoppingController : Controller
    {
        private readonly MusicStoreContext _context;
        private readonly IWebHostEnvironment _environment;

        public ShoppingController(MusicStoreContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index(int genreId, string currentTitle, int page = 0)
        {
            const int PageSize = 3; // you can always do something more elegant to set this
            List<SelectListItem> genres = new SelectList(_context.Genres, "GenreId", "Name", genreId).ToList();
            genres.Insert(0, new SelectListItem { Value = "0", Text = "All" });
            ViewData["CurrentTitle"] = currentTitle;
            ViewData["GenreIds"] = genres;

            var musicStoreContext = _context.Albums.Include(a => a.Artist).Include(a => a.Genre)
                .Where(a => a.GenreId == (genreId == 0 ? a.GenreId : genreId)
                && a.Title.Contains(currentTitle ?? "")).ToList();
            var count = musicStoreContext.Count();
            var data = musicStoreContext.Skip(page * PageSize).Take(PageSize).ToList();

            this.ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);

            this.ViewBag.Page = page;

            return this.View(data);
            return View(musicStoreContext);



        }
        public IActionResult Add(Album album)
        {
            try {
            var cartItem = _context.Carts.Where(
                   c => c.CartId == HttpContext.Session.GetString("UserName")
                   && c.AlbumId == album.AlbumId).FirstOrDefault();
                
                if (cartItem == null)
                    {
                        cartItem = new Cart
                        {
                            AlbumId = album.AlbumId,
                            CartId = HttpContext.Session.GetString("UserName"),
                            Count = 1,
                            DateCreated = DateTime.Now

                        };

                        _context.Carts.Add(cartItem);
                    
                    // Create a new cart item if no cart item exists
                   
                }
                else
                {
                // If the item does exist in the cart, then add one to the quantity
                    cartItem.Count++;
                    
                }

                // Save changes
                _context.SaveChanges();
            }
            catch (Exception)
            {

            }

            return RedirectToAction("Cart", "Shopping");
        }
        public IActionResult Remove(int id)
        {
            var cartItem = _context.Carts.SingleOrDefault(
                 cart => cart.CartId == HttpContext.Session.GetString("UserName")
                 && cart.RecordId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    _context.Carts.Remove(cartItem);
                }

                // Save changes
                _context.SaveChanges();
            }


            return RedirectToAction("Cart", "Shopping");
        }


        public IActionResult CheckOut()
        {
            List<Cart> carts = _context.Carts.Include(a => a.Album).Where(c => c.CartId == HttpContext.Session.GetString("UserName")).ToList();
            var user = _context.Users.Where(a => a.UserName == HttpContext.Session.GetString("UserName")).FirstOrDefault();
            //cart.AddToCart(album);
            //var carts = cart.GetCartItems();
            var Total = (from cartItems in carts

                         select cartItems.Count * cartItems.Album.Price).Sum();
            ViewData["FirstName"] = user.FirstName;
            ViewData["LastName"] = user.LastName;
            ViewData["State"] = user.State;
            ViewData["Country"] = user.Country;
            ViewData["City"] = user.City;
            ViewData["State"] = user.State;
            ViewData["Phone"] = user.Phone;
            ViewData["Email"] = user.Email;
            ViewData["OrderDate"] = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["ToTal"] = Total;
            return View();


        }
        public void EmptyCart()
        {
            var cartItems = _context.Carts.Where(cart => cart.CartId == HttpContext.Session.GetString("UserName"));

            foreach (var cartItem in cartItems)
            {
                _context.Carts.Remove(cartItem);
            }

            // Save changes
            _context.SaveChanges();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut([Bind("OrderId,OrderDate,PromoCode,UserName,FirstName,LastName,Price,Address,City,State,Country,Phone,Email,Total")] Order order)
        {

            EmptyCart();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));

        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Cart()
        {

            ShoppingCart cart = new ShoppingCart();
            //Album album = _context.Albums.Where(a => a.AlbumId == AlbumId).FirstOrDefault();

            List<Cart> carts = _context.Carts.Include(a => a.Album).Where(c => c.CartId == HttpContext.Session.GetString("UserName")).ToList();

            //cart.AddToCart(album);
            //var carts = cart.GetCartItems();
            ViewBag.Total = (from cartItems in carts

                             select cartItems.Count * cartItems.Album.Price).Sum();


            return View(carts);
        }
        public async Task<IActionResult> AddToCart(int id)
        {
            try
            {
                var cartItem = _context.Carts.Where(
                   c => c.CartId == HttpContext.Session.GetString("UserName")
                   && c.AlbumId == id).FirstOrDefault();

                if (cartItem == null)
                {
                    // Create a new cart item if no cart item exists
                    cartItem = new Cart
                    {
                        AlbumId = id,
                        CartId = HttpContext.Session.GetString("UserName"),
                        Count = 1,
                        DateCreated = DateTime.Now
                    };

                    _context.Carts.Add(cartItem);
                }
                else
                {
                    // If the item does exist in the cart, then add one to the quantity
                    cartItem.Count++;
                }

                // Save changes
                _context.SaveChanges();

            }
            catch (Exception)
            {

            }
            
            return RedirectToAction("Index", "Shopping");

        }
    }

}
