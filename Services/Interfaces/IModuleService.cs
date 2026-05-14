using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IModuleService
    {
        // Get modules by role
        Task<List<Module>> GetModulesByUserRoleAsync(string userRole, int userId);
        
        // Get single module with details
        Task<Module?> GetModuleByIdAsync(int moduleId);
        
        // Get modules for a specific lecturer
        Task<List<Module>> GetModulesByLecturerAsync(int lecturerId);
        
        // Get all modules (for admin/student)
        Task<List<Module>> GetAllModulesAsync();
        
        // CRUD operations
        Task<bool> CreateModuleAsync(Module module);
        Task<bool> UpdateModuleAsync(Module module);
        Task<bool> DeleteModuleAsync(int moduleId);
        
        // Check if lecturer owns the module
        Task<bool> IsModuleOwnerAsync(int moduleId, int lecturerId);
        Task<string> RegenerateInvitationCodeAsync(int moduleId);

        Task<bool> IsInvitationCodeExistsAsync(string code);
    }
}