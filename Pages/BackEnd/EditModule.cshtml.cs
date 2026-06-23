using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class EditModuleModel : PageModel
    {
        private readonly IModuleService _moduleService;
        private readonly ILecturerService _lecturerService;

        public EditModuleModel(IModuleService moduleService, ILecturerService lecturerService)
        {
            _moduleService = moduleService;
            _lecturerService = lecturerService;
        }

        [BindProperty(SupportsGet = true)]
        public int ModuleID { get; set; }

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

        public string ModuleCode { get; set; } = string.Empty;
        public List<Lecturer> Lecturers { get; set; } = new();
        public bool ShowSuccessPopup { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleID = id;
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null) return NotFound();

            StartDate = DateTime.SpecifyKind(StartDate, DateTimeKind.Utc);
            EndDate = DateTime.SpecifyKind(EndDate, DateTimeKind.Utc);

            ModuleCode = module.ModuleCode;
            Name = module.Name;
            Description = module.Description;
            LecturerID = module.LecturerID;
            StartDate = module.StartDate;
            EndDate = module.EndDate;
            Lecturers = await _lecturerService.GetAllLecturersAsync();

            if (TempData.ContainsKey("ShowSuccessPopup") && TempData["ShowSuccessPopup"] is bool isTrue && isTrue)
                ShowSuccessPopup = true;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || LecturerID == 0)
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

            var module = await _moduleService.GetModuleByIdAsync(ModuleID);
            if (module == null)
            {
                TempData["ErrorMessage"] = "Module not found.";
                return RedirectToPage("/FrontEnd/ModuleList");
            }

            module.Name = Name.Trim();
            module.Description = Description?.Trim();
            module.LecturerID = LecturerID;
            module.StartDate = StartDate;
            module.EndDate = EndDate;

            var success = await _moduleService.UpdateModuleAsync(module);
            if (success)
            {
                TempData["ShowSuccessPopup"] = true;
                return RedirectToPage(new { id = ModuleID });
            }
            TempData["ErrorMessage"] = "Failed to update module.";
            Lecturers = await _lecturerService.GetAllLecturersAsync();
            return Page();
        }
    }
}