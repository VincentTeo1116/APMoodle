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
        [EmailAddress]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;
    }
}