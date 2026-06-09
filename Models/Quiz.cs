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
        // NOTE: No direct Quiz->Result navigation. Results relate to a Quiz only
        // indirectly (via Session and via Question). Declaring a Quiz.Results
        // collection makes EF invent a shadow "QuizID" FK column on Results that
        // does not exist in the database, which breaks every quiz submission
        // (Npgsql 42703: column "QuizID" of relation "Results" does not exist).
        public List<Session>? Sessions { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }
}