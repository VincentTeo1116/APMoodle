using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface ILecturerService
    {
        Task<Lecturer?> GetLecturerByCodeOrEmailAsync(string userId);
        Task<Lecturer?> GetLecturerByIdAsync(int id);
        Task<bool> LecturerExistsAsync(string email);
    }
}