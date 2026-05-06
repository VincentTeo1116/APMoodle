using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public Admin? Admin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}