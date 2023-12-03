using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Model;
using ECommerce.Model.ViewModel;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {

        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // DI
        public ProductController(IProductRepository productRepo, ICategoryRepository categoryRepo, IWebHostEnvironment webHostEnvironment)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _webHostEnvironment = webHostEnvironment;

        }

        public IActionResult Index()
        {

            var productList = _productRepo.GetAll();

            return View(productList);
        }

        // Combining Create with Update
        public IActionResult Upsert(int productId) 
        {

            // Initialize ProductVM
            ProductVM productVM = new ProductVM();

            // Same for Create and Update - Getting list of categories
            productVM.CategoryList = _categoryRepo.GetAll().Select(u=> new SelectListItem 
            { 
                Text = u.Name,
                Value = u.CategoryId.ToString()
            });

            // If id not present - Create
            if (productId == null || productId == 0)
            {
                productVM.Product = new Product();
            }
            // If id present - Update
            else {
                productVM.Product = _productRepo.Get(productId);
            }

            return View(productVM);

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile file) {

            // If field inputs are valid - No errors returned from Db table constraints
            if (ModelState.IsValid)
            {

                string wwwRootPath = _webHostEnvironment.WebRootPath;
                // If a file has been uploaded
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Generate random file name
                    string productPath = Path.Combine(wwwRootPath, @"images\product"); // Find product images folder

                    // If Product already has an image
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl)) 
                    {
                        // Delete old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath)) 
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    // Save file to folder
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Set ImageUrl property
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                // Create operation
                if (productVM.Product.ProductId == 0)
                {
                    if (file == null) {
                        // If no file uploaded set property to empty string
                        productVM.Product.ImageUrl = "";
                    }

                    _productRepo.Add(productVM.Product);
                }
                else 
                {
                    _productRepo.Update(productVM.Product);
                }
                
                _productRepo.Save();

                // Adding notification info to TempData - TempData renders once, used to store one time messages
                TempData["success"] = "Product created successfully";

                // Go back to categories display
                return RedirectToAction("Index");
            }
            // If not valid
            else 
            {
                productVM.CategoryList = _categoryRepo.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                });

                // Display upsert page
                return View(productVM);

            }

        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll() 
        {
            var productList = _productRepo.GetAll();
            // Returns list of products in JSON format
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {

            var product = _productRepo.Get(id);

            if (product == null) {
                return Json(new { success = false, message = "Delete unsuccessful" });
            }

            // Delete image
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _productRepo.Remove(product);
            _productRepo.Save();

            // Returns success message
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion

    }
}
