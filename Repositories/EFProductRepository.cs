namespace WebBanHang.Repositories;
using WebBanHang.Models;
using Microsoft.EntityFrameworkCore;

public class EFProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public EFProductRepository (ProductDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // return await _context.Products.ToListAsync(); 
        return await _context.Products
    .Include(p => p.Category) // Include thông tin về category 
    .ToListAsync();

    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.Include(p => p.Category)
                                      .FirstOrDefaultAsync(p => p.Id == id);
    }

public async Task AddAsync(Product product)
{
    var categoryExists = await _context.Categories.FindAsync(product.CategoryId);
    if (categoryExists == null)
    {
        throw new Exception("CategoryId không hợp lệ! Không thể thêm sản phẩm.");
    }

    _context.Products.Add(product);
    await _context.SaveChangesAsync();
}


    public async Task UpdateAsync(Product product)
{
    var existingProduct = await _context.Products.FindAsync(product.Id);
    if (existingProduct != null)
    {
        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.ImageUrl = product.ImageUrl;

        await _context.SaveChangesAsync();
    }
}



    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)  // Kiểm tra null trước khi xóa
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

}