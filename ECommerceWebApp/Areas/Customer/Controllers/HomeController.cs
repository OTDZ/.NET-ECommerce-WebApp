using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Model;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ECommerceWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepo;
        private readonly IShoppingCartRepository _shoppingCartRepo;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepo, IShoppingCartRepository shoppingCartRepo)
        {
            _logger = logger;
            _productRepo = productRepo;
            _shoppingCartRepo = shoppingCartRepo;
        }

        public IActionResult Index()
        {
            // Retrieving userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // If logged in get session item count
            if (claim != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _shoppingCartRepo.GetAll(u => u.ApplicationUserId == claim.Value).Count());
            }

            IEnumerable<Product> productList = _productRepo.GetAll();

            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            Product product = _productRepo.Get(productId);

            ShoppingCart shoppingCart = new()
            {
                Product = product,
                ProductId = productId,
                Count = 1
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // Retrieving userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            // Check if cart and product already exist
            ShoppingCart shoppingCartFromDb = _shoppingCartRepo.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
            if (shoppingCartFromDb != null)
            {
                // Update count
                shoppingCartFromDb.Count += shoppingCart.Count;
                _shoppingCartRepo.Update(shoppingCartFromDb);
                _shoppingCartRepo.Save();
            }
            else {
                // Create new shoppingCart
                _shoppingCartRepo.Add(shoppingCart);
                _shoppingCartRepo.Save();
                // Add to session
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart updated successfully";

            _shoppingCartRepo.Save();

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}