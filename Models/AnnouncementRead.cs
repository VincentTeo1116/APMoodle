using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMoodle.Models
{
    public class AnnouncementRead
    {
        [Key]
        public int AnnouncementReadID { get; set; }

        public int UserID { get; set; }
        public int AnnouncementID { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AnnouncementID")]
        public virtual Announcement? Announcement { get; set; }
    }
}