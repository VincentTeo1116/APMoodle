using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface ILecturerService
    {
        Task<Lecturer?> GetLecturerByIdAsync(int id);
        Task<Lecturer?> GetLecturerByCodeAsync(string lecturerCode);
        Task<Lecturer?> GetLecturerByEmailAsync(string email);
        Task<Lecturer?> GetLecturerByCodeOrEmailAsync(string userId);
        Task<bool> LecturerExistsAsync(string email);
        Task<bool> UpdateLecturerAsync(Lecturer lecturer);
    }
}