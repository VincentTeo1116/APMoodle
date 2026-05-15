using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; }

        [Required]
        [MaxLength(10)]
        public string AdminCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;  // Store hashed password

        [Required]
        [Column(TypeName = "date")]
        public DateOnly DOB { get; set; }  // Date of Birth

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;  // "Male", "Female", "Other"

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ProfilePic { get; set; }  // Google Drive URL

        // Navigation properties
        public List<Announcement>? Announcements { get; set; }
    }
}