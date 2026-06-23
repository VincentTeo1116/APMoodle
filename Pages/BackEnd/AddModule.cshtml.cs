using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class AddModuleModel : PageModel
    {
        private readonly IModuleService _moduleService;
        private readonly ILecturerService _lecturerService;

        public AddModuleModel(IModuleService moduleService, ILecturerService lecturerService)
        {
            _moduleService = moduleService;
            _lecturerService = lecturerService;
        }

        [BindProperty]
        public string ModuleCode { get; set; } = string.Empty;

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public int LecturerID { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        public List<Lecturer> Lecturers { get; set; } = new();
        public bool ShowSuccessPopup { get; set; }

        public async Task OnGetAsync()
        {
            Lecturers = await _lecturerService.GetAllLecturersAsync();
            if (TempData["ShowSuccessPopup"] != null && (bool)TempData["ShowSuccessPopup"])
                ShowSuccessPopup = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(ModuleCode) || string.IsNullOrWhiteSpace(Name) || LecturerID == 0)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                Lecturers = await _lecturerService.GetAllLecturersAsync();
                return Page();
            }
            if (EndDate < StartDate)
            {
                TempData["ErrorMessage"] = "End date must be after start date.";
                Lecturers = await _lecturerService.GetAllLecturersAsync();
                return Page();
            }

            StartDate = DateTime.SpecifyKind(StartDate, DateTimeKind.Utc);
            EndDate = DateTime.SpecifyKind(EndDate, DateTimeKind.Utc);

            var module = new Module
            {
                ModuleCode = ModuleCode.Trim(),
                Name = Name.Trim(),
                Description = Description?.Trim(),
                LecturerID = LecturerID,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = "Active",
                InvitationCode = GenerateInvitationCode()
            };

            var success = await _moduleService.CreateModuleAsync(module);
            if (success)
            {
                TempData["ShowSuccessPopup"] = true;
                return RedirectToPage();
            }
            TempData["ErrorMessage"] = "Failed to create module. Please try again.";
            Lecturers = await _lecturerService.GetAllLecturersAsync();
            return Page();
        }

        private string GenerateInvitationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rand = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}