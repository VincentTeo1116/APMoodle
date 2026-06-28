using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ApplicationDbContext _context;

        public ModuleService(ApplicationDbContext context, IEnrollmentService enrollmentService)
        {
            _context = context;
            _enrollmentService = enrollmentService;
        }

        // Get modules based on user role
        public async Task<List<Module>> GetModulesByUserRoleAsync(string userRole, int userId)
        {
            if (userRole == "lecturer")
            {
                return await GetModulesByLecturerAsync(userId);
            }
            else if (userRole == "student")
            {
                // Student sees ONLY enrolled modules
                return await _enrollmentService.GetEnrolledModulesAsync(userId);
            }
            else
            {
                // Admin sees all modules
                return await GetAllModulesAsync();
            }
        }

        // Get modules for a specific lecturer
        public async Task<List<Module>> GetModulesByLecturerAsync(int lecturerId)
        {
            return await _context.Modules
                .Where(m => m.LecturerID == lecturerId && m.Status == "Active")
                .Include(m => m.Lecturer)
                .OrderBy(m => m.ModuleCode)
                .ToListAsync();
        }

        // Get all modules
        public async Task<List<Module>> GetAllModulesAsync()
        {
            return await _context.Modules
                .Include(m => m.Lecturer)
                .OrderBy(m => m.ModuleCode)
                .ToListAsync();
        }

        // Get single module by ID
        public async Task<Module?> GetModuleByIdAsync(int moduleId)
        {
            return await _context.Modules
                .Include(m => m.Lecturer)
                .Include(m => m.Materials)
                .FirstOrDefaultAsync(m => m.ModuleID == moduleId);
        }

        // Create new module
        public async Task<bool> CreateModuleAsync(Module module)
        {
            try
            {
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Update module
        public async Task<bool> UpdateModuleAsync(Module module)
        {
            try
            {
                _context.Modules.Update(module);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Delete module
        public async Task<bool> DeleteModuleAsync(int moduleId)
        {
            try
            {
                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null) return false;

                // _context.Modules.Remove(module);
                module.Status = "Removed";
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Check if lecturer owns the module
        public async Task<bool> IsModuleOwnerAsync(int moduleId, int lecturerId)
        {
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.ModuleID == moduleId && m.LecturerID == lecturerId);

            return module != null;
        }

        public async Task<string> RegenerateInvitationCodeAsync(int moduleId)
        {
            var module = await _context.Modules.FindAsync(moduleId);
            if (module == null)
            {
                throw new Exception("Module not found");
            }

            var newCode = GenerateUniqueCode();

            // Ensure code is unique across all modules
            while (await _context.Modules.AnyAsync(m => m.InvitationCode == newCode))
            {
                newCode = GenerateUniqueCode();
            }

            module.InvitationCode = newCode;
            await _context.SaveChangesAsync();

            return newCode;
        }

        private string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task<bool> IsInvitationCodeExistsAsync(string code)
        {
            return await _context.Modules.AnyAsync(m => m.InvitationCode == code);
        }

        public async Task<Module?> GetModuleByInvitationCodeAsync(string invitationCode)
        {
            if (string.IsNullOrWhiteSpace(invitationCode))
                return null;

            return await _context.Modules
                .Include(m => m.Lecturer)
                .FirstOrDefaultAsync(m => m.InvitationCode == invitationCode);
        }

        public async Task<bool> ModuleExistsAsync(string moduleCode)
        {
            return await _context.Modules.AnyAsync(m => m.ModuleCode == moduleCode);
        }

        public async Task<string> GenerateNextModuleCodeAsync()
        {
            var last = await _context.Modules
                .OrderByDescending(m => m.ModuleCode)
                .FirstOrDefaultAsync();
            if (last == null || string.IsNullOrEmpty(last.ModuleCode))
                return "MDL001";
            var numPart = last.ModuleCode.Replace("MDL", "");
            if (int.TryParse(numPart, out int num))
                return $"MDL{(num + 1):D3}";
            return "MDL001";
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Modules.CountAsync();
        }
    }
}