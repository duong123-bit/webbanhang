using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WebBanHang.Repositories;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ProductDbContext _context;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ProductDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        // Display product list with optional search
        public async Task<IActionResult> Index(string search = "")
        {
            string z = search;
            if (string.IsNullOrEmpty(search))
            {
                var applicationDbContext = _context.Products.Include(p => p.Category);
                ViewBag.search = search;
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                IEnumerable<Product> dssearch = _context.Products.Include(p => p.Category);
                List<Product> ds = new List<Product>();
                foreach (var i in dssearch)
                {
                    string a1 = i.Description.ToUpper();
                    if (a1.Contains(z.ToUpper()))
                    {
                        ds.Add(i);
                        continue;
                    }

                    string a2 = i.Name.ToUpper();
                    if (a2.Contains(z.ToUpper()))
                    {
                        ds.Add(i);
                        continue;
                    }

                    string a3 = i.Category.Name.ToUpper();
                    if (a3.Contains(search.ToUpper()))
                    {
                        ds.Add(i);
                        continue;
                    }
                }
                ViewBag.search = search;
                return View(ds);
            }
        }

        // Show the product details
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // Show the add product form
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        // Handle the add product action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(product);
            }

            // Handle image upload
            if (imageUrl != null && imageUrl.Length > 0)
            {
                product.ImageUrl = await SaveImage(imageUrl); // Save the image and get its path
            }
            else
            {
                // If no image is uploaded, set the default image
                product.ImageUrl = "/images/default.jpg";
            }

            await _productRepository.AddAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // Show the edit product form
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // Handle the update product action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);

                // Retain the existing image if no new image is uploaded
                if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    // Save the new image
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;

                await _productRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // Show the delete product confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // Process the product deletion (POST)
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Helper method to save images
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return "/images/default.jpg"; // Use default image if no file is uploaded

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/{fileName}";
        }
    }
}
