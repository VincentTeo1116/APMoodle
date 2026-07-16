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
                .Where(a => a.Status == "Active")
                .Include(a => a.Creator)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements
                .Where(a => a.Status == "Active")
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.AnnouncementID == id);
        }

        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement, int createdByAdminId)
        {
            announcement.CreatedBy = createdByAdminId;
            announcement.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            announcement.Status = "Active";
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
             return await GetAnnouncementByIdAsync(announcement.AnnouncementID) ?? announcement;
        }

        public async Task<Announcement> UpdateAnnouncementAsync(int id, Announcement announcement, int modifiedByAdminId)
        {
            var existingAnnouncement = await _context.Announcements.FindAsync(id);
            if (existingAnnouncement == null)
                throw new KeyNotFoundException($"Announcement with ID {id} not found");

            existingAnnouncement.Title = announcement.Title;
            existingAnnouncement.Message = announcement.Message;
            existingAnnouncement.Status = announcement.Status;
            existingAnnouncement.LastModifiedBy = modifiedByAdminId;
            existingAnnouncement.LastModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            _context.Entry(existingAnnouncement).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            // Reload with navigation properties
            return await GetAnnouncementByIdAsync(id) ?? existingAnnouncement;
        }

        public async Task<bool> DeleteAnnouncementAsync(int id, int deletedByAdminId)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            announcement.Status = "Removed";
            announcement.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            announcement.DeletedBy = deletedByAdminId;

            _context.Entry(announcement).Property(x => x.Status).IsModified = true;
            _context.Entry(announcement).Property(x => x.DeletedAt).IsModified = true;
            _context.Entry(announcement).Property(x => x.DeletedBy).IsModified = true;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Announcements.CountAsync(a => a.Status == "Active");
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var readIds = await _context.AnnouncementReads
                .Where(ar => ar.UserID == userId)
                .Select(ar => ar.AnnouncementID)
                .ToListAsync();

            var allActive = await _context.Announcements
                .Where(a => a.Status == "Active" && a.DeletedAt == null)
                .Select(a => a.AnnouncementID)
                .ToListAsync();

            return allActive.Count(id => !readIds.Contains(id));
        }

        public async Task MarkAnnouncementAsReadAsync(int userId, int announcementId)
        {
            var exists = await _context.AnnouncementReads
                .AnyAsync(ar => ar.UserID == userId && ar.AnnouncementID == announcementId);
            if (exists) return;

            _context.AnnouncementReads.Add(new AnnouncementRead
            {
                UserID = userId,
                AnnouncementID = announcementId,
                ReadAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAnnouncementsAsReadAsync(int userId)
        {
            var allIds = await _context.Announcements
                .Where(a => a.Status == "Active" && a.DeletedAt == null)
                .Select(a => a.AnnouncementID)
                .ToListAsync();

            var readIds = await _context.AnnouncementReads
                .Where(ar => ar.UserID == userId)
                .Select(ar => ar.AnnouncementID)
                .ToListAsync();

            var toAdd = allIds.Except(readIds).ToList();
            if (toAdd.Any())
            {
                var reads = toAdd.Select(id => new AnnouncementRead
                {
                    UserID = userId,
                    AnnouncementID = id,
                    ReadAt = DateTime.UtcNow
                });
                await _context.AnnouncementReads.AddRangeAsync(reads);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsAnnouncementReadAsync(int userId, int announcementId)
        {
            return await _context.AnnouncementReads
                .AnyAsync(ar => ar.UserID == userId && ar.AnnouncementID == announcementId);
        }
    }
}