using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _audit;
        private readonly int _maxItems;


        public PlaylistService(ApplicationDbContext db, IAuditService audit, int maxItems = 20)
        {
            _db = db;
            _audit = audit;
            _maxItems = maxItems;
        }

        public async Task<IEnumerable<Playlist>> GetAllAsync()
        {
            return await _db.Playlists.Include(p => p.Screen!).ThenInclude(s => s.Department)
                                      .Include(p => p.Items)
                                      .OrderByDescending(p => p.CreatedAt)
                                      .ToListAsync();
        }

        public async Task<IEnumerable<Playlist>> GetByDepartmentAsync(int departmentId)
        {
            return await _db.Playlists.Include(p => p.Screen!).ThenInclude(s => s.Department)
                                      .Include(p => p.Items)
                                      .Where(p => p.Screen != null && p.Screen.DepartmentId == departmentId)
                                      .OrderByDescending(p => p.CreatedAt)
                                      .ToListAsync();
        }

        public async Task<Playlist?> GetByIdAsync(int id)
        {
            return await _db.Playlists.Include(p => p.Screen)
                                      .Include(p => p.Items).ThenInclude(i => i.MediaFile)
                                      .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Playlist?> GetByScreenAsync(int screenId)
        {
            return await _db.Playlists.Include(p => p.Items).ThenInclude(i => i.MediaFile)
                                      .Where(p => p.ScreenId == screenId)
                                      .OrderByDescending(p => p.CreatedAt)
                                      .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Playlist playlist, string userId)
        {
            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Playlist", playlist.Id, "Create", userId);
        }

        public async Task UpdateAsync(Playlist playlist, string userId)
        {
            _db.Playlists.Update(playlist);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Playlist", playlist.Id, "Update", userId);
        }

        public async Task AddItemAsync(int playlistId, PlaylistItem item, string userId)
        {
            var count = await _db.PlaylistItems.CountAsync(i => i.PlaylistId == playlistId);
            if (count >= _maxItems)
                throw new InvalidOperationException($"Een playlist mag maximaal {_maxItems} items bevatten.");

            item.PlaylistId = playlistId;
            _db.PlaylistItems.Add(item);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("PlaylistItem", item.Id, "Add", userId);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var playlist = await _db.Playlists.FindAsync(id);
            if (playlist == null)
                throw new InvalidOperationException("Playlist not found.");

            _db.Playlists.Remove(playlist);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Playlist", id, "Delete", userId);
        }

        public async Task RemoveItemAsync(int itemId, string userId)
        {
            var item = await _db.PlaylistItems.FindAsync(itemId);
            if (item == null)
                throw new InvalidOperationException("Playlist item not found.");

            _db.PlaylistItems.Remove(item);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("PlaylistItem", itemId, "Remove", userId);
        }
    }
}
