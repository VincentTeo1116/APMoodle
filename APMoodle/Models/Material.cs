using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Material
    {
        [Key]
        public int MaterialID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FilePath { get; set; }  // Link to file or video

        // Foreign key to Module
        public int ModuleID { get; set; }
        [ForeignKey("ModuleID")]
        public Module? Module { get; set; }

        // Foreign key to Lecturer (who uploaded)
        public int UploadedBy { get; set; }
        [ForeignKey("UploadedBy")]
        public Lecturer? Lecturer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}