using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IEnrollmentService
    {
        // Check if student is enrolled in a module
        Task<bool> IsEnrolledAsync(int studentId, int moduleId);
        
        // Enroll student in a module
        Task<bool> EnrollStudentAsync(int studentId, int moduleId);
        
        // Drop student from a module
        Task<bool> DropEnrollmentAsync(int studentId, int moduleId);
        
        // Get all modules a student is enrolled in
        Task<List<Module>> GetEnrolledModulesAsync(int studentId);
        
        // Get all students enrolled in a module (for lecturer)
        Task<List<Student>> GetEnrolledStudentsAsync(int moduleId);
        
        // Get available modules (not yet enrolled)
        Task<List<Module>> GetAvailableModulesAsync(int studentId);
        
        // Get enrollment record
        Task<Enrollment?> GetEnrollmentAsync(int studentId, int moduleId);
        
        // Update progress
        Task<bool> UpdateProgressAsync(int studentId, int moduleId, int progress);
    }
}