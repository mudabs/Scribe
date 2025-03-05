using Scribe.Models;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;

namespace Scribe.Infrastructure
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
    }

    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
    }
}
