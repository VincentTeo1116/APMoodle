using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Module
    {
        [Key]
        public int ModuleID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ModuleCode { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(10)]
        public string InvitationCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ModulePic { get; set; }  // Google Drive URL

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } // Start date of the module

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } // End date of the module

        [Required]
        public int LecturerID { get; set; }

        // Navigation property
        [ForeignKey("LecturerID")]
        public Lecturer? Lecturer { get; set; }
    }
}