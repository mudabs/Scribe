using Scribe.Models;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;

namespace Scribe.Infrastructure
{
    public interface IBrandService
    {
        Task<List<Brand>> GetAllBrandsAsync();
    }

    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands.ToListAsync();
        }
        public async Task<List<Brand>> GetBrandsByIdAsync(int brandId)
        {
            return await _context.Brands.Where(b => b.Id == brandId).ToListAsync();
        }

    }
}
