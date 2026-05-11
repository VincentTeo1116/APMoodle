using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        [Required]
        public int StudentID { get; set; }

        [Required]
        public int ModuleID { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Active";  // Active, Completed, Dropped

        public int? Progress { get; set; } = 0;  // Percentage of completion

        // Navigation properties
        [ForeignKey("StudentID")]
        public Student? Student { get; set; }

        [ForeignKey("ModuleID")]
        public Module? Module { get; set; }
    }
}