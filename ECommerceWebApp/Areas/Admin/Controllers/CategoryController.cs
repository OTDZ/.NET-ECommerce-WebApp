using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Model;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {

        private readonly ICategoryRepository _categoryRepo;

        // Dependency injection
        public CategoryController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index()
        {
            // Get all categories
            List<Category> categoryList = _categoryRepo.GetAll().ToList();

            // Pass category list to View
            return View(categoryList);
        }

        // Form for creating Category
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] // Adding to Db
        public IActionResult Create(Category obj)
        {

            // If field inputs are valid - No errors returned from Db table constraints
            if (ModelState.IsValid)
            {
                _categoryRepo.Add(obj);
                _categoryRepo.Save();

                // Adding notification info to TempData - TempData renders once
                TempData["success"] = "Category created successfully";

                // Go back to categories display
                return RedirectToAction("Index");
            }

            return View();

        }

        public IActionResult Edit(int categoryId)
        {

            Category category = _categoryRepo.Get(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost] // Adding to Db
        public IActionResult Edit(Category obj)
        {

            // If field inputs are valid - No errors returned from Db table constraints
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(obj);
                _categoryRepo.Save();

                // Adding notification info to TempData
                TempData["success"] = "Category updated successfully";

                // Go back to categories display
                return RedirectToAction("Index");
            }

            return View();

        }

        public IActionResult Delete(int categoryId)
        {

            Category category = _categoryRepo.Get(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Delete(Category obj)
        {
            _categoryRepo.Remove(obj);
            _categoryRepo.Save();

            // Adding notification info to TempData
            TempData["success"] = "Category deleted successfully";

            // Go back to categories display
            return RedirectToAction("Index");

        }


    }
}
