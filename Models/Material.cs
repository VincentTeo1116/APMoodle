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

        [MaxLength(500)]
        public string? FileURL { get; set; }  // Link to Google Drive or local file

        public string? Content { get; set; }  // For text-based materials

        // Foreign key to Module
        [Required]
        public int ModuleID { get; set; }

        [ForeignKey("ModuleID")]
        public Module? Module { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<Quiz>? Quizzes { get; set; }
    }
}