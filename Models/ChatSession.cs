using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class ChatSession
    {
        [Key]
        public int ChatID { get; set; }

        [Required]
        public int StudentID { get; set; }

        [Required]
        public int LecturerID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("StudentID")]
        public Student? Student { get; set; }

        [ForeignKey("LecturerID")]
        public Lecturer? Lecturer { get; set; }

        public List<Message>? Messages { get; set; }
    }
}