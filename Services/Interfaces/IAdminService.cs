using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IAdminService
    {
        Task<Admin?> GetAdminByIdAsync(int id);
        Task<Admin?> GetAdminByCodeAsync(string adminCode);
        Task<Admin?> GetAdminByEmailAsync(string email);
        Task<Admin?> GetAdminByCodeOrEmailAsync(string userId);
        Task<bool> UpdateAdminAsync(Admin admin);
    }
}