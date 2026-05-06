using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Student
    {
        [Key]
        public int StudentID { get; set; }

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
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }  // Date of Birth

        [Required]
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;  // "Male", "Female", "Other"

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ProfilePic { get; set; }  // Google Drive URL
    }
}