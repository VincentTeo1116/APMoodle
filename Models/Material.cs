using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class Material
    {
        [Key]
        public int MaterialID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ContentType { get; set; } = "file";

        [MaxLength(500)]
        public string? FileURL { get; set; }

        [MaxLength(500)]
        public string? ContentUrl { get; set; }

        public string? ContentText { get; set; }

        public string? EmbedHtml { get; set; }

        [Required]
        public int ModuleID { get; set; }

        [ForeignKey("ModuleID")]
        public Module? Module { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        public List<Quiz>? Quizzes { get; set; }
    }
}