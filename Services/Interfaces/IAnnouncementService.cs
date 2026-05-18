using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<List<Announcement>> GetAllAnnouncementsAsync();
        Task<Announcement?> GetAnnouncementByIdAsync(int id);
        Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
        Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
        Task<bool> DeleteAnnouncementAsync(int id);
        Task<int> GetTotalCountAsync();
    }
}