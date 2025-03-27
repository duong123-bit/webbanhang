using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ProductDbContext _context;

        public ProductController(ProductDbContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index(string search)
        {
            // Tìm kiếm sản phẩm theo tên hoặc ID
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) || p.Id.ToString().Contains(search));
            }

            // Truyền giá trị tìm kiếm vào ViewData để giữ lại giá trị khi trang được tải lại
            ViewData["SearchQuery"] = search;

            return View(await products.ToListAsync());
        }

        // Action hiển thị form thêm sản phẩm
        public IActionResult Add()
        {
            ViewBag.Categories = _context.Categories.ToList();  // Truyền danh sách category
            return View();
        }

        // Action xử lý thêm sản phẩm vào database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));  // Redirect to product list after adding
            }

            // Nếu không hợp lệ, trả về form với lỗi
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // Action hiển thị form chỉnh sửa sản phẩm
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.Categories.ToList();  // Truyền danh sách category
            return View(product);
        }

        // Action xử lý chỉnh sửa sản phẩm trong database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // Action xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Xử lý xóa sản phẩm khỏi database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
