using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IMaterialService
    {
        Task<List<Material>> GetMaterialsByModuleIdAsync(int moduleId);
        Task<Material?> GetMaterialByIdAsync(int materialId);
        Task<bool> CreateMaterialAsync(Material material);
        Task<bool> UpdateMaterialAsync(Material material);
        Task<bool> DeleteMaterialAsync(int materialId);
    }
}