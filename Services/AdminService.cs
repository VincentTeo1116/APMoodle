using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        public async Task<Admin?> GetAdminByCodeAsync(string adminCode)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.AdminCode == adminCode);
        }

        public async Task<Admin?> GetAdminByEmailAsync(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Admin?> GetAdminByCodeOrEmailAsync(string userId)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.AdminCode == userId || a.Email == userId);
        }

        public async Task<bool> UpdateAdminAsync(Admin admin)
        {
            try
            {
                var existingAdmin = await _context.Admins.FindAsync(admin.AdminID);
                if (existingAdmin == null) return false;

                // Only update allowed fields
                existingAdmin.Name = admin.Name;
                existingAdmin.PhoneNumber = admin.PhoneNumber;
                existingAdmin.DOB = admin.DOB;
                existingAdmin.Gender = admin.Gender;
                existingAdmin.ProfilePic = admin.ProfilePic;

                _context.Admins.Update(existingAdmin);
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