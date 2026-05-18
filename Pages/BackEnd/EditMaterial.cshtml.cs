using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;
using Microsoft.AspNetCore.Antiforgery;

namespace APMoodle.Pages.BackEnd
{
    [IgnoreAntiforgeryToken]
    public class EditMaterialModel : PageModel
    {
        private readonly IMaterialService _materialService;
        private readonly IModuleService _moduleService;
        private readonly IWebHostEnvironment _environment;

        public EditMaterialModel(IMaterialService materialService, IModuleService moduleService, IWebHostEnvironment environment)
        {
            _materialService = materialService;
            _moduleService = moduleService;
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)]
        public int MaterialId { get; set; }
        
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContentType { get; set; } = "file";
        public string? FileURL { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentText { get; set; }
        public bool ShowSuccessPopup { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            MaterialId = material.MaterialID;
            ModuleId = material.ModuleID;
            Title = material.Title;
            Description = material.Description ?? string.Empty;
            ContentType = material.ContentType;
            FileURL = material.FileURL;
            ContentUrl = material.ContentUrl;
            ContentText = material.ContentText;

            if (TempData["ShowSuccessPopup"] != null && (bool)TempData["ShowSuccessPopup"])
            {
                ShowSuccessPopup = true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!int.TryParse(Request.Form["materialId"], out int materialId))
                {
                    return BadRequest("Invalid material ID");
                }

                var material = await _materialService.GetMaterialByIdAsync(materialId);
                if (material == null)
                {
                    return NotFound();
                }

                var title = Request.Form["title"].ToString();
                var description = Request.Form["description"].ToString();
                var contentType = Request.Form["contentType"].ToString();

                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest("Title is required");

                material.Title = title;
                material.Description = description ?? string.Empty;
                material.ContentType = contentType;

                switch (contentType)
                {
                    case "file":
                        var file = Request.Form.Files.GetFile("file");
                        if (file != null && file.Length > 0)
                        {
                            // Delete old file if exists
                            if (!string.IsNullOrEmpty(material.FileURL))
                            {
                                var oldFilePath = Path.Combine(_environment.WebRootPath, material.FileURL.TrimStart('/'));
                                if (System.IO.File.Exists(oldFilePath))
                                    System.IO.File.Delete(oldFilePath);
                            }

                            // Get module and lecturer info for file naming
                            var module = await _moduleService.GetModuleByIdAsync(material.ModuleID);
                            var lecturerName = module?.Lecturer?.Name ?? "Unknown";
                            var moduleName = module?.Name ?? "Module";
                            
                            var safeLecturerName = SanitizeFileName(lecturerName);
                            var safeModuleName = SanitizeFileName(moduleName);
                            var safeTitle = SanitizeFileName(title);
                            
                            var fileExtension = Path.GetExtension(file.FileName);
                            var uniqueFileName = $"{safeLecturerName}_{safeModuleName}_{safeTitle}{fileExtension}";
                            
                            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "materials");
                            Directory.CreateDirectory(uploadsFolder);
                            
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            
                            // Handle duplicate filenames
                            if (System.IO.File.Exists(filePath))
                            {
                                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                                uniqueFileName = $"{safeLecturerName}_{safeModuleName}_{safeTitle}_{timestamp}{fileExtension}";
                                filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            }

                            using (var stream = new FileStream(filePath, FileMode.Create))
                                await file.CopyToAsync(stream);

                            material.FileURL = $"/uploads/materials/{uniqueFileName}";
                            material.ContentUrl = null;
                            material.ContentText = null;
                        }
                        break;

                    case "link":
                        var contentUrl = Request.Form["contentUrl"].ToString();
                        if (string.IsNullOrWhiteSpace(contentUrl))
                            return BadRequest("URL is required");
                        material.ContentUrl = contentUrl;
                        material.FileURL = null;
                        material.ContentText = null;
                        break;

                    case "text":
                        var contentText = Request.Form["contentText"].ToString();
                        if (string.IsNullOrWhiteSpace(contentText))
                            return BadRequest("Text content is required");
                        material.ContentText = contentText;
                        material.FileURL = null;
                        material.ContentUrl = null;
                        break;
                }

                var success = await _materialService.UpdateMaterialAsync(material);
                if (success)
                {
                    TempData["ShowSuccessPopup"] = true;
                    TempData["SuccessMessage"] = "Material updated successfully!";

                    return RedirectToPage("/FrontEnd/EditMaterial", new { id = materialId });
                }
                return StatusCode(500, "Failed to save changes");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray());
            
            sanitized = sanitized.Replace(' ', '_');
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^\w\-_]", "");
            
            if (sanitized.Length > 50)
                sanitized = sanitized.Substring(0, 50);
            
            return sanitized;
        }
    }
}