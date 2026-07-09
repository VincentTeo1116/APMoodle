using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class UserListModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserListModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<UserViewModel> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int StudentCount { get; set; }
        public int LecturerCount { get; set; }
        public int AdminCount { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch students
            var students = await _context.Students
                .Select(s => new
                {
                    s.StudentID,
                    s.Name,
                    s.Email,
                    s.StudentCode,
                    s.Status,
                    s.ProfilePic,
                    s.RegisteredDate,
                    s.PhoneNumber,
                    s.Gender
                })
                .ToListAsync();

            var studentViewModels = students.Select(s => new UserViewModel
            {
                Id = s.StudentID,
                Name = s.Name,
                Email = s.Email,
                UserCode = s.StudentCode,
                Role = "Student",
                Status = s.Status,
                ProfilePic = s.ProfilePic,
                RegisteredDate = s.RegisteredDate,
                PhoneNumber = s.PhoneNumber,
                Gender = s.Gender,
                Department = null,
                AvatarColor = GetAvatarColor("Student"),
                Initials = GetInitials(s.Name),
                RoleIcon = "bi-person"
            }).ToList();

            // Fetch lecturers
            var lecturers = await _context.Lecturers
                .Select(l => new
                {
                    l.LecturerID,
                    l.Name,
                    l.Email,
                    l.LecturerCode,
                    l.Status,
                    l.ProfilePic,
                    l.RegisteredDate,
                    l.PhoneNumber,
                    l.Gender,
                    l.Department
                })
                .ToListAsync();

            var lecturerViewModels = lecturers.Select(l => new UserViewModel
            {
                Id = l.LecturerID,
                Name = l.Name,
                Email = l.Email,
                UserCode = l.LecturerCode,
                Role = "Lecturer",
                Status = l.Status,
                ProfilePic = l.ProfilePic,
                RegisteredDate = l.RegisteredDate,
                PhoneNumber = l.PhoneNumber,
                Gender = l.Gender,
                Department = l.Department,
                AvatarColor = GetAvatarColor("Lecturer"),
                Initials = GetInitials(l.Name),
                RoleIcon = "bi-person-workspace"
            }).ToList();

            var admins = await _context.Admins
                .Select(a => new
                {
                    a.AdminID,
                    a.Name,
                    a.Email,
                    a.AdminCode,
                    a.Status,
                    a.ProfilePic,
                    a.PhoneNumber,
                    a.Gender
                })
                .ToListAsync();

            var adminViewModels = admins.Select(a => new UserViewModel
            {
                Id = a.AdminID,
                Name = a.Name,
                Email = a.Email,
                UserCode = a.AdminCode,
                Role = "Admin",
                Status = a.Status,
                ProfilePic = a.ProfilePic,
                RegisteredDate = null,
                PhoneNumber = a.PhoneNumber,
                Gender = a.Gender,
                Department = null,
                AvatarColor = GetAvatarColor("Admin"),
                Initials = GetInitials(a.Name),
                RoleIcon = "bi-shield"
            }).ToList();

            // Combine all users
            Users = studentViewModels
                .Concat(lecturerViewModels)
                .Concat(adminViewModels)
                .OrderBy(u => u.Name)
                .ToList();

            // Calculate stats
            TotalCount = Users.Count;
            StudentCount = studentViewModels.Count;
            LecturerCount = lecturerViewModels.Count;
            AdminCount = adminViewModels.Count;
        }

        private static string GetAvatarColor(string role)
        {
            return role switch
            {
                "Student" => "#3b82f6",
                "Lecturer" => "#f59e0b",
                "Admin" => "#ef4444",
                _ => "#64748b"
            };
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            var parts = name.Trim().Split(' ');
            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
            return name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Department { get; set; }
        public string AvatarColor { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string RoleIcon { get; set; } = string.Empty;
    }
}