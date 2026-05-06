using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Session
    {
        [Key]
        public int SessionID { get; set; }

        // Foreign key to Student (who took the quiz)
        [Required]
        public int StudentID { get; set; }

        [ForeignKey("StudentID")]
        public Student? Student { get; set; }

        // Foreign key to Quiz
        [Required]
        public int QuizID { get; set; }

        [ForeignKey("QuizID")]
        public Quiz? Quiz { get; set; }

        // When the attempt started
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Status of the session
        public bool IsCompleted { get; set; } = false;

        // Total score for the session (calculated from Results)
        public int? TotalScore { get; set; }

        // Navigation property: One session has many results (one per question)
        public List<Result>? Results { get; set; }
    }
}