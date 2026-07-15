using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEnrolledAsync(int studentId, int moduleId)
        {
            return await _context.Enrollments!
                .AnyAsync(e => e.StudentID == studentId && e.ModuleID == moduleId && e.Status == "Active");
        }

        public async Task<bool> EnrollStudentAsync(int studentId, int moduleId)
        {
            try
            {
                // Check if already enrolled
                if (await IsEnrolledAsync(studentId, moduleId))
                    return false;
                
                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null || module.Status != "Active")
                    return false;

                var enrollment = new Enrollment
                {
                    StudentID = studentId,
                    ModuleID = moduleId,
                    EnrolledDate = DateTime.UtcNow,
                    Status = "Active",
                    Progress = 0
                };

                _context.Enrollments!.Add(enrollment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DropEnrollmentAsync(int studentId, int moduleId)
        {
            try
            {
                var enrollment = await _context.Enrollments!
                    .FirstOrDefaultAsync(e => e.StudentID == studentId && e.ModuleID == moduleId);
                
                if (enrollment == null) return false;

                enrollment.Status = "Dropped";
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Module>> GetEnrolledModulesAsync(int studentId)
        {
            return await _context.Enrollments!
                .Where(e => e.StudentID == studentId && e.Status == "Active")
                .Include(e => e.Module)
                    .ThenInclude(m => m!.Lecturer)
                .Where(e => e.Module!.Status == "Active")
                .Select(e => e.Module!)
                .ToListAsync();
        }

        public async Task<List<Student>> GetEnrolledStudentsAsync(int moduleId)
        {
            return await _context.Enrollments!
                .Where(e => e.ModuleID == moduleId && e.Status == "Active")
                .Include(e => e.Student)
                .Select(e => e.Student!)
                .ToListAsync();
        }

        public async Task<List<Module>> GetAvailableModulesAsync(int studentId)
        {
            // Get modules student is NOT enrolled in
            var enrolledModuleIds = await _context.Enrollments!
                .Where(e => e.StudentID == studentId && e.Status == "Active")
                .Select(e => e.ModuleID)
                .ToListAsync();

            return await _context.Modules
                .Where(m => m.Status == "Active" && !enrolledModuleIds.Contains(m.ModuleID))
                .Include(m => m.Lecturer)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentAsync(int studentId, int moduleId)
        {
            return await _context.Enrollments!
                .FirstOrDefaultAsync(e => e.StudentID == studentId && e.ModuleID == moduleId);
        }

        public async Task<bool> UpdateProgressAsync(int studentId, int moduleId, int progress)
        {
            try
            {
                var enrollment = await GetEnrollmentAsync(studentId, moduleId);
                if (enrollment == null) return false;

                enrollment.Progress = progress;
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