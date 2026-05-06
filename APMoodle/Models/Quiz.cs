using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Quiz
    {
        [Key]
        public int QuizID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Theme { get; set; } = string.Empty;

        // Foreign key to teaching material (belongs to which material)
        public int MaterialID { get; set; }

        [ForeignKey("MaterialID")]
        public Material? Material { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<Question>? Questions { get; set; }
        public List<Result>? Results { get; set; }
    }
}