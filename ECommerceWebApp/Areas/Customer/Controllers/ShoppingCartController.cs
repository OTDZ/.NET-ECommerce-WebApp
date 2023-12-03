using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Model;
using ECommerce.Model.ViewModel;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace ECommerceWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartRepository _shoppingCartRepo;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IOrderHeaderRepository _orderHeaderRepo;
        private readonly IOrderDetailRepository _orderDetailRepo;

        public ShoppingCartController(IShoppingCartRepository shoppingCartRepo, IApplicationUserRepository userRepo, IOrderHeaderRepository orderHeaderRepo, IOrderDetailRepository orderDetailRepo)
        {
            _shoppingCartRepo = shoppingCartRepo;
            _userRepo = userRepo;
            _orderHeaderRepo = orderHeaderRepo;
            _orderDetailRepo = orderDetailRepo;
        }

        public IActionResult Index()
        {

            // Retrieving userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                // Get all items related to user
                ShoppingCartList = _shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new ()
            };

            // Order total
            foreach (var cart in shoppingCartVM.ShoppingCartList) {
                cart.Price = (cart.Product.Price * cart.Count);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
            }

            return View(shoppingCartVM);
        }

        public IActionResult Summary() 
        {

            // Retrieving userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                // Get all items related to user
                ShoppingCartList = _shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = _userRepo.Get(userId);

            // Set OrderHeader fields using user from database
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            // Order total
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = (cart.Product.Price * cart.Count);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        public IActionResult Summary(ShoppingCartVM shoppingCartVM) 
        {
			// Retrieving userId
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			// Get all items related to user
			shoppingCartVM.ShoppingCartList = _shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            // Populate OrderHeader fields
            shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.Carrier = "";
            shoppingCartVM.OrderHeader.SessionId = "";
            shoppingCartVM.OrderHeader.PaymentIntentId = "";
            shoppingCartVM.OrderHeader.TrackingNumber = "";

			// Order total
			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
				cart.Price = (cart.Product.Price * cart.Count);
				shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
			}

            // Set order and payment status to pending
            shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;

            // Add OrderHeader record to Db
            _orderHeaderRepo.Add(shoppingCartVM.OrderHeader);
            _orderHeaderRepo.Save();

            // Create corresponding OrderDetail for each item
            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.OrderHeaderId,
                    Price = item.Price,
                    Count = item.Count
                };
                // Add to Db
                _orderDetailRepo.Add(orderDetail);
                _orderDetailRepo.Save();
            }

            // Capture stripe payment
            var domain = "https://localhost:7135/";
			var options = new SessionCreateOptions
			{
				SuccessUrl = domain+$"customer/shoppingcart/orderconfirmation?orderId={shoppingCartVM.OrderHeader.OrderHeaderId}",
                CancelUrl = domain+"customer/shoppingcart/index",
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
			};

            // Populating LineItems with ordered products
            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "gbp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

			var service = new SessionService();
			Session session = service.Create(options);

            // PayIntId will be null as not generated yet
            _orderHeaderRepo.UpdateStripePaymentId(shoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
            _orderHeaderRepo.Save();

            // Redirect to Stripe for payment
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            return RedirectToAction("OrderConfirmation", new { orderId=shoppingCartVM.OrderHeader.OrderHeaderId });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            OrderHeader orderHeader = _orderHeaderRepo.Get(orderId);

            // Checking if stripe payment was successful by looking at session
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                // PaymentIntentId has been generated - update
                _orderHeaderRepo.UpdateStripePaymentId(orderId, session.Id, session.PaymentIntentId);
                // Update order and payment status
                _orderHeaderRepo.UpdateStatus(orderId, SD.StatusApproved, SD.PaymentStatusApproved);
                _orderHeaderRepo.Save();

			}
            HttpContext.Session.Clear();

            // Clear shopping carts
            List<ShoppingCart> shoppingCarts = _shoppingCartRepo.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            foreach (var cart in shoppingCarts) {
                _shoppingCartRepo.Remove(cart);
            }

            _shoppingCartRepo.Save();

            return View(orderId);
        }

        public IActionResult Plus(int shoppingCartId) 
        {
            var shoppingCartFromDb = _shoppingCartRepo.Get(u => u.ShoppingCartId == shoppingCartId);
            shoppingCartFromDb.Count += 1;
            _shoppingCartRepo.Update(shoppingCartFromDb);
            _shoppingCartRepo.Save();
            return RedirectToAction("Index");

        }

        public IActionResult Minus(int shoppingCartId)
        {
            var shoppingCartFromDb = _shoppingCartRepo.Get(u => u.ShoppingCartId == shoppingCartId);

            if (shoppingCartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _shoppingCartRepo.GetAll(u => u.ApplicationUserId == shoppingCartFromDb.ApplicationUserId).Count() - 1);
                _shoppingCartRepo.Remove(shoppingCartFromDb);
            }
            else {
                shoppingCartFromDb.Count -= 1;
                _shoppingCartRepo.Update(shoppingCartFromDb);
            }
            _shoppingCartRepo.Save();
            return RedirectToAction("Index");

        }

        public IActionResult Remove(int shoppingCartId)
        {
            var shoppingCartFromDb = _shoppingCartRepo.Get(u => u.ShoppingCartId == shoppingCartId);

            HttpContext.Session.SetInt32(SD.SessionCart,
                _shoppingCartRepo.GetAll(u => u.ApplicationUserId == shoppingCartFromDb.ApplicationUserId).Count() - 1);
            _shoppingCartRepo.Remove(shoppingCartFromDb);
            _shoppingCartRepo.Save();
            return RedirectToAction("Index");

        }

    }
}
