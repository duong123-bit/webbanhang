using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Extensions;
using WebBanHang.Models;
using WebBanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace WebBanHang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProductDbContext _context;

        public ShoppingCartController(IProductRepository productRepository, UserManager<ApplicationUser> userManager, ProductDbContext context)
        {
            _productRepository = productRepository;
            _userManager = userManager;
            _context = context;
        }

        // Hiển thị trang checkout
        public IActionResult Checkout()
        {
            return View(new Order());
        }

        // Xử lý việc thanh toán
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            // Kiểm tra giỏ hàng trống
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.CustomerName = user.UserName; // Hoặc sử dụng user.FullName nếu có thuộc tính FullName
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng trong session sau khi thanh toán
            HttpContext.Session.Remove("Cart");

            // Chuyển đến trang xác nhận
            return View("OrderCompleted", order.Id);
        }

        // Phương thức lấy sản phẩm từ cơ sở dữ liệu
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductFromDatabase(productId);

            // Tạo item giỏ hàng
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };

            // Lấy giỏ hàng từ session hoặc tạo mới nếu chưa có
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);

            // Lưu giỏ hàng vào session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Chuyển hướng về trang giỏ hàng
            return RedirectToAction("Index");
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            // Lấy giỏ hàng từ session hoặc tạo mới nếu chưa có
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // Xóa sản phẩm khỏi giỏ
        public IActionResult RemoveFromCart(int productId)
        {
            // Lấy giỏ hàng từ session
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            if (cart != null)
            {
                // Xóa sản phẩm khỏi giỏ hàng
                cart.RemoveItem(productId);

                // Lưu lại giỏ hàng vào session sau khi xóa
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            // Chuyển hướng lại trang giỏ hàng
            return RedirectToAction("Index");
        }
    }
}
