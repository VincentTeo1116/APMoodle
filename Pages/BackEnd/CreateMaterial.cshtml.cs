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
        private readonly IModuleService _moduleService;
        private readonly IWebHostEnvironment _environment;

        public CreateMaterialModel(IMaterialService materialService, IModuleService moduleService, IWebHostEnvironment environment)
        {
            _materialService = materialService;
            _moduleService = moduleService;
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
            try
            {
                var moduleIdStr = Request.Form["moduleId"].ToString();
                var moduleId = !string.IsNullOrEmpty(moduleIdStr) ? int.Parse(moduleIdStr) : 0;
                var title = Request.Form["title"].ToString();
                var description = Request.Form["description"].ToString();
                var contentType = Request.Form["contentType"].ToString();

                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest("Title is required");

                // Get module and lecturer info for file naming
                var module = await _moduleService.GetModuleByIdAsync(moduleId);
                var lecturerName = module?.Lecturer?.Name ?? "Unknown";
                var moduleName = module?.Name ?? "Module";

                // Sanitize names (remove special characters for filename)
                var safeLecturerName = SanitizeFileName(lecturerName);
                var safeModuleName = SanitizeFileName(moduleName);
                var safeTitle = SanitizeFileName(title);

                var material = new Material
                {
                    Title = title,
                    Description = description ?? string.Empty,
                    ModuleID = moduleId,
                    ContentType = contentType,
                    CreatedAt = DateTime.UtcNow
                };

                switch (contentType)
                {
                    case "file":
                        var file = Request.Form.Files.GetFile("file");
                        if (file == null || file.Length == 0)
                            return BadRequest("No file selected");

                        // Get file extension
                        var fileExtension = Path.GetExtension(file.FileName);

                        // Create filename: [LecturerName]_[ModuleName]_[MaterialName][extension]
                        var uniqueFileName = $"{safeLecturerName}_{safeModuleName}_{safeTitle}{fileExtension}";

                        // Handle duplicate filenames
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "materials");
                        Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // If file exists, add timestamp to make it unique
                        if (System.IO.File.Exists(filePath))
                        {
                            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            uniqueFileName = $"{safeLecturerName}_{safeModuleName}_{safeTitle}_{timestamp}{fileExtension}";
                            filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await file.CopyToAsync(stream);

                        material.FileURL = $"/uploads/materials/{uniqueFileName}";
                        break;

                    case "link":
                        var contentUrl = Request.Form["contentUrl"].ToString();
                        if (string.IsNullOrWhiteSpace(contentUrl))
                            return BadRequest("URL is required");
                        material.ContentUrl = contentUrl;
                        break;

                    case "text":
                        var contentText = Request.Form["contentText"].ToString();
                        if (string.IsNullOrWhiteSpace(contentText))
                            return BadRequest("Text content is required");
                        material.ContentText = contentText;
                        break;
                }

                var success = await _materialService.CreateMaterialAsync(material);
                return success ? new OkResult() : StatusCode(500, "Failed to save");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove invalid filename characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray());

            // Replace spaces with underscores
            sanitized = sanitized.Replace(' ', '_');

            // Remove any other problematic characters
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^\w\-_]", "");

            // Limit length to prevent overly long filenames
            if (sanitized.Length > 50)
                sanitized = sanitized.Substring(0, 50);

            return sanitized;
        }
    }
}