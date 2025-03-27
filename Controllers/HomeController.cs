using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Repositories;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<HomeController> _logger;

        // Constructor nhận vào cả IProductRepository và ILogger<HomeController>
        public HomeController(IProductRepository productRepository, ILogger<HomeController> logger)
        {
            _productRepository = productRepository;  // Gán _productRepository
            _logger = logger;  // Gán _logger
        }

        // Phương thức Index lấy danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả sản phẩm từ ProductRepository
            var products = await _productRepository.GetAllAsync();
            // Trả về view với danh sách sản phẩm
            return View(products);
        }

        // Phương thức Privacy (dành cho trang Privacy)
        public IActionResult Privacy()
        {
            return View();
        }

        // Phương thức Error để xử lý lỗi
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
