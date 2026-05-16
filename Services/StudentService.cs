using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _context.Students!.FindAsync(id);
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students!.ToListAsync();
        }

        public async Task<Student?> GetStudentByCodeOrEmailAsync(string userId)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.StudentCode == userId || s.Email == userId);
        }

        public async Task<Student?> GetStudentByCodeAsync(string studentCode)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
        }

        public async Task<Student?> GetStudentByEmailAsync(string email)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<string> GenerateNextStudentCodeAsync()
        {
            var lastStudent = await _context.Students!
                .OrderByDescending(s => s.StudentCode)
                .FirstOrDefaultAsync();

            if (lastStudent == null || string.IsNullOrEmpty(lastStudent.StudentCode))
            {
                return "ST00001";
            }

            if (lastStudent.StudentCode.Length >= 2)
            {
                var numberPart = lastStudent.StudentCode.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    var nextNumber = lastNumber + 1;
                    return $"ST{nextNumber:D5}";  // 5 digits
                }
            }

            return "ST00001";
        }

        public async Task<bool> CreateStudentAsync(Student student)
        {
            try
            {
                // Generate code if not provided
                if (string.IsNullOrEmpty(student.StudentCode))
                {
                    student.StudentCode = await GenerateNextStudentCodeAsync();
                }

                _context.Students!.Add(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating student: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            try
            {
                var existingStudent = await _context.Students!.FindAsync(student.StudentID);
                if (existingStudent == null) return false;

                // Only update allowed fields
                existingStudent.Name = student.Name;
                existingStudent.PhoneNumber = student.PhoneNumber;
                existingStudent.DOB = student.DOB;
                existingStudent.Gender = student.Gender;
                existingStudent.ProfilePic = student.ProfilePic;
                // Convert RegisteredDate to UTC
                existingStudent.RegisteredDate = DateTime.SpecifyKind(existingStudent.RegisteredDate, DateTimeKind.Utc);

                _context.Students.Update(existingStudent);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                var student = await _context.Students!.FindAsync(id);
                if (student == null) return false;

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StudentExistsAsync(string email)
        {
            return await _context.Students!.AnyAsync(s => s.Email == email);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Students!.CountAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _context.Students!.CountAsync(s => s.Status == "Pending");
        }

        public async Task<List<Student>> GetPendingStudentsAsync()
        {
            return await _context.Students!.Where(s => s.Status == "Pending").ToListAsync();
        }
    }
}