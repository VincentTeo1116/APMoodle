using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Message
    {
        [Key]
        public int MessageID { get; set; }

        [Required]
        public int ChatID { get; set; }

        [Required]
        public int SenderID { get; set; }

        [Required]
        [MaxLength(10)]
        public string SenderRole { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // Navigation property
        [ForeignKey("ChatID")]
        public ChatSession? ChatSession { get; set; }
    }
}