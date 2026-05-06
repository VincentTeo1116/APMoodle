using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }

        [Required]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } = "A";  // A, B, C, or D

        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Option1 { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Option2 { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Option3 { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Option4 { get; set; } = string.Empty;

        [ForeignKey("QuizID")]
        public Quiz? Quiz { get; set; }

        // Navigation properties
        public List<Result>? Results { get; set; }
    }
}