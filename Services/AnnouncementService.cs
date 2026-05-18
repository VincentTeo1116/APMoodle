using APMoodle.Models;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;

namespace APMoodle.Services.Interfaces
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.AnnouncementID == id);
        }

        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
        {
            announcement.CreatedAt = DateTime.UtcNow;
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
        {
            _context.Entry(announcement).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Announcements.CountAsync();
        }
    }
}