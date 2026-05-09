using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IAdminService
    {
        Task<Admin?> GetAdminByCodeOrEmailAsync(string userId);
        Task<Admin?> GetAdminByIdAsync(int id);
    }
}