using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<List<Announcement>> GetAllAnnouncementsAsync();
        Task<Announcement?> GetAnnouncementByIdAsync(int id);
        Task<Announcement> CreateAnnouncementAsync(Announcement announcement, int createdByAdminId);
        Task<Announcement> UpdateAnnouncementAsync(int id, Announcement announcement, int modifiedByAdminId);
        Task<bool> DeleteAnnouncementAsync(int id, int deletedByAdminId);
        Task<int> GetTotalCountAsync();
    }
}