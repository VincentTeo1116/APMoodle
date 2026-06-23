using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IStudentService
    {
        // Basic CRUD operations
        Task<Student?> GetStudentByIdAsync(int id);
        Task<Student?> GetStudentByEmailAsync(string email);
        Task<Student?> GetStudentByCodeAsync(string studentCode); 
        Task<Student?> GetStudentByCodeOrEmailAsync(string userId);
        Task<List<Student>> GetAllStudentsAsync();
        Task<bool> CreateStudentAsync(Student student);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
        
        // Check if student exists
        Task<bool> StudentExistsAsync(string email);
        
        // Generate custom code
        Task<string> GenerateNextStudentCodeAsync(); 

        // Statistics
        Task<int> GetTotalCountAsync();
        Task<int> GetPendingCountAsync();
        Task<List<Student>> GetPendingStudentsAsync();
        Task<List<Student>> SearchStudentsAsync(string searchTerm);
        Task<bool> UpdatePasswordAsync(int studentId, string newHashedPassword);
    }
}