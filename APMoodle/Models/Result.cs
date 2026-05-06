using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        // Foreign key to Session
        [Required]
        public int SessionID { get; set; }

        [ForeignKey("SessionID")]
        public Session? Session { get; set; }

        // Foreign key to Question
        [Required]
        public int QuestionID { get; set; }

        [ForeignKey("QuestionID")]
        public Question? Question { get; set; }

        // Student's answer (A, B, C, D)
        [Required]
        [MaxLength(1)]
        public string Answer { get; set; } = string.Empty;

        // Time spent on this question (in seconds)
        [Required]
        public int TimeUsed { get; set; }

        // Track if answer was correct
        public bool IsCorrect { get; set; } = false;
    }
}