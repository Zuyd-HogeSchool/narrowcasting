using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _audit;

        public AnnouncementService(ApplicationDbContext db, IAuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _db.Announcements.Include(a => a.Department)
                                          .Include(a => a.Screen)
                                          .Include(a => a.MediaFile)
                                          .Include(a => a.CreatedBy)
                                          .OrderByDescending(a => a.PublishedAt)
                                          .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetActiveAsync()
        {
            return await _db.Announcements.Include(a => a.Department)
                                          .Include(a => a.Screen)
                                          .Include(a => a.MediaFile)
                                          .Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.Now)
                                          .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetActiveForDepartmentAsync(int departmentId)
        {
            return await _db.Announcements.Include(a => a.Department)
                                          .Include(a => a.Screen)
                                          .Include(a => a.MediaFile)
                                          .Where(a => a.DepartmentId == departmentId
                                                && (a.ExpiresAt == null || a.ExpiresAt > DateTime.Now))
                                          .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetActiveForScreenAsync(int screenId, int departmentId, bool includeInternal)
        {
            return await _db.Announcements
                .Include(a => a.Department)
                .Include(a => a.Screen)
                .Include(a => a.MediaFile)
                .Where(a => a.DepartmentId == departmentId)
                .Where(a => a.ScreenId == null || a.ScreenId == screenId)
                .Where(a => includeInternal || !a.IsInternal)
                .Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.Now)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            return await _db.Announcements.Include(a => a.Department)
                                          .Include(a => a.Screen)
                                          .Include(a => a.MediaFile)
                                          .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task CreateAsync(Announcement announcement, string userId)
        {
            _db.Announcements.Add(announcement);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Announcement", announcement.Id, "Create", userId);
        }

        public async Task UpdateAsync(Announcement announcement, string userId)
        {
            _db.Announcements.Update(announcement);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Announcement", announcement.Id, "Update", userId);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var announcement = await _db.Announcements.FindAsync(id);
            if (announcement is null) return;
            _db.Announcements.Remove(announcement);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Announcement", id, "Delete", userId);
        }
    }
}