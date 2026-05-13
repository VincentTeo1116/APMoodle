using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;
using Microsoft.AspNetCore.Antiforgery;

namespace APMoodle.Pages.BackEnd
{
    [IgnoreAntiforgeryToken]
    public class CreateMaterialModel : PageModel
    {
        private readonly IMaterialService _materialService;
        private readonly IWebHostEnvironment _environment;

        public CreateMaterialModel(IMaterialService materialService, IWebHostEnvironment environment)
        {
            _materialService = materialService;
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)]
        public int ModuleId { get; set; }

        public void OnGet(int moduleId)
        {
            ModuleId = moduleId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"=== UPLOAD START ===");
            
            try
            {
                // Read data from form
                var moduleId = int.Parse(Request.Form["moduleId"]);
                var title = Request.Form["title"].ToString();
                var description = Request.Form["description"].ToString();
                var content = Request.Form["content"].ToString();
                var file = Request.Form.Files.GetFile("file");

                Console.WriteLine($"ModuleId: {moduleId}");
                Console.WriteLine($"Title: {title}");
                Console.WriteLine($"File: {file?.FileName}");

                // Validation
                if (string.IsNullOrWhiteSpace(title))
                {
                    return BadRequest("Title is required");
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file selected");
                }

                // 📁 SAVE FILE LOCALLY
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "materials");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename to prevent conflicts
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create URL that can be accessed from browser
                var fileUrl = $"/uploads/materials/{uniqueFileName}";

                Console.WriteLine($"File saved to: {filePath}");
                Console.WriteLine($"File URL: {fileUrl}");

                // Create material record
                var material = new Material
                {
                    Title = title,
                    Description = description ?? string.Empty,
                    Content = content ?? string.Empty,
                    FileURL = fileUrl,
                    ModuleID = moduleId,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _materialService.CreateMaterialAsync(material);

                if (!success)
                {
                    return StatusCode(500, "Failed to save to database");
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}