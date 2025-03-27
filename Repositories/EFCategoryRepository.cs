namespace WebBanHang.Repositories;
using WebBanHang.Models;
using Microsoft.EntityFrameworkCore;

public class EFCategoryRepository : ICategoryRepository
{
    private readonly ProductDbContext _context;

    public EFCategoryRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }



    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)  // Kiểm tra null trước khi xóa
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}