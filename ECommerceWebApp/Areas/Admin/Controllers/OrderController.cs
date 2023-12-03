using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Model;
using ECommerce.Model.ViewModel;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{

		private readonly IOrderHeaderRepository _orderHeaderRepo;
		private readonly IOrderDetailRepository _orderDetailRepo;
        private readonly IShoppingCartRepository _shoppingCartRepo;
        [BindProperty]
        public OrderVM orderVM { get; set; }

        public OrderController(IOrderHeaderRepository orderHeaderRepo, IOrderDetailRepository orderDetailRepo, IShoppingCartRepository shoppingCartRepo)
		{
			_orderHeaderRepo = orderHeaderRepo;
			_orderDetailRepo = orderDetailRepo;
            _shoppingCartRepo = shoppingCartRepo;
		}

		public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderHeaderId)
        {

			orderVM = new()
			{
				OrderHeader = _orderHeaderRepo.Get(u => u.OrderHeaderId == orderHeaderId, includeProperties:"ApplicationUser"),
				OrderDetail = _orderDetailRepo.GetAll(u => u.OrderHeaderId == orderHeaderId, includeProperties:"Product")
			};

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
			var orderHeaderFromDb = _orderHeaderRepo.Get(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId);
			orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
			orderHeaderFromDb.State = orderVM.OrderHeader.State;
			orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
			if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier)) { 
				orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
			}
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
			_orderHeaderRepo.Update(orderHeaderFromDb);
			_orderHeaderRepo.Save();

			TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction("Details", new { orderHeaderId = orderHeaderFromDb.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ProcessOrder()
		{
			_orderHeaderRepo.UpdateStatus(orderVM.OrderHeader.OrderHeaderId, SD.StatusInProcess);
			_orderHeaderRepo.Save();

            TempData["Success"] = "Order Status Updated Successfully";

            return RedirectToAction("Details", new { orderHeaderId = orderVM.OrderHeader.OrderHeaderId });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

			// Update fields
			var orderHeaderFromDb = _orderHeaderRepo.Get(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId);
			orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
			orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;

            _orderHeaderRepo.UpdateStatus(orderVM.OrderHeader.OrderHeaderId, SD.StatusShipped);
			orderHeaderFromDb.ShippingDate = DateTime.Now;

            _orderHeaderRepo.Update(orderHeaderFromDb);
            _orderHeaderRepo.Save();

            TempData["Success"] = "Order Shipped Successfully";

            return RedirectToAction("Details", new { orderHeaderId = orderVM.OrderHeader.OrderHeaderId });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {

            // Update fields
            var orderHeaderFromDb = _orderHeaderRepo.Get(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId);

            // If customer paid - administer refund
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _orderHeaderRepo.UpdateStatus(orderVM.OrderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                // Else - update order status to cancelled
                _orderHeaderRepo.UpdateStatus(orderVM.OrderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusCancelled);
            }

            _orderHeaderRepo.Update(orderHeaderFromDb);
            _orderHeaderRepo.Save();

            TempData["Success"] = "Order Cancelled Successfully";

            return RedirectToAction("Details", new { orderHeaderId = orderVM.OrderHeader.OrderHeaderId });

        }

        #region API CALLS

        [HttpGet]
		public IActionResult GetAll()
		{

			IEnumerable<OrderHeader> orderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
                // If Admin or Employee retrieve all orders
                orderList = _orderHeaderRepo.GetAll(null, includeProperties: "ApplicationUser").ToList();
            }
			// Display orders 
			else
			{
                // If Customer retrieve only their orders
                var claimsIdentity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

				orderList = _orderHeaderRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();

            }
			
			// Returns list of orders in JSON format
			return Json(new { data = orderList });
		}

		#endregion

	}
}
