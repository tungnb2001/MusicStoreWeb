using Microsoft.AspNetCore.Mvc;
using Project_WebMVC.Models;
using System.Diagnostics;

namespace Project_WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string UserName, string Password)
        {
            MusicStoreContext context = new MusicStoreContext();
            User user1 = context.Users.Where(u => u.UserName == UserName
                    && u.Password == Password).FirstOrDefault();

            if (user1 == null)
                return View();
            else
            {
                HttpContext.Session.SetString("UserName", UserName);
                HttpContext.Session.SetInt32("Role", user1.Role);

                ShoppingCart cart = ShoppingCart.GetCart(HttpContext);
                cart.MigrateCart(HttpContext);

                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult Logout()
        {
            SignOut();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
