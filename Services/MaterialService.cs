using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _context;

        public MaterialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Material>> GetMaterialsByModuleIdAsync(int moduleId)
        {
            return await _context.Materials!
                .Where(m => m.ModuleID == moduleId && m.Status == "Active")
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Material?> GetMaterialByIdAsync(int materialId)
        {
            return await _context.Materials!
                .Include(m => m.Module)
                .FirstOrDefaultAsync(m => m.MaterialID == materialId && m.Status == "Active");
        }

        public async Task<bool> CreateMaterialAsync(Material material)
        {
            try
            {
                material.CreatedAt = DateTime.UtcNow;
                material.Status = "Active";
                _context.Materials!.Add(material);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateMaterialAsync(Material material)
        {
            try
            {
                _context.Materials!.Update(material);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMaterialAsync(int materialId)
        {
            try
            {
                var material = await _context.Materials.FindAsync(materialId);

                if (material == null)
                {
                    return false;
                }

                material.Status = "Removed";

                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}