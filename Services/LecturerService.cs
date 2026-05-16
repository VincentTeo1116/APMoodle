using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class LecturerService : ILecturerService
    {
        private readonly ApplicationDbContext _context;

        public LecturerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Lecturer?> GetLecturerByIdAsync(int id)
        {
            return await _context.Lecturers.FindAsync(id);
        }

        public async Task<Lecturer?> GetLecturerByCodeAsync(string lecturerCode)
        {
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerCode == lecturerCode);
        }

        public async Task<Lecturer?> GetLecturerByEmailAsync(string email)
        {
            return await _context.Lecturers.FirstOrDefaultAsync(l => l.Email == email);
        }

        public async Task<Lecturer?> GetLecturerByCodeOrEmailAsync(string userId)
        {
            return await _context.Lecturers
                .FirstOrDefaultAsync(l => l.LecturerCode == userId || l.Email == userId);
        }

        public async Task<bool> LecturerExistsAsync(string email)
        {
            return await _context.Lecturers.AnyAsync(l => l.Email == email);
        }

        public async Task<bool> UpdateLecturerAsync(Lecturer lecturer)
        {
            try
            {
                var existingLecturer = await _context.Lecturers.FindAsync(lecturer.LecturerID);
                if (existingLecturer == null) return false;

                // Only update allowed fields
                existingLecturer.Name = lecturer.Name;
                existingLecturer.PhoneNumber = lecturer.PhoneNumber;
                existingLecturer.DOB = lecturer.DOB;
                existingLecturer.Gender = lecturer.Gender;
                existingLecturer.Department = lecturer.Department;
                existingLecturer.ProfilePic = lecturer.ProfilePic;
                existingLecturer.RegisteredDate = DateTime.SpecifyKind(existingLecturer.RegisteredDate, DateTimeKind.Utc);

                _context.Lecturers.Update(existingLecturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                return false;
            }
        }
    }
}