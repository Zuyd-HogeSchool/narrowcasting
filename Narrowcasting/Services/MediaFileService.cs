using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using Narrowcasting.Enums;

namespace NarrowCasting.Services
{
    public class MediaFileService : IMediaFileService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _audit;
        private readonly IWebHostEnvironment _env;

        public MediaFileService(ApplicationDbContext db, IAuditService audit, IWebHostEnvironment env)
        {
            _db = db;
            _audit = audit;
            _env = env;
        }

        public async Task<IEnumerable<MediaFile>> GetAllOrderedAsync()
        {
            return await _db.MediaFiles.OrderByDescending(m => m.UploadedAt).ToListAsync();
        }

        public async Task<MediaFile?> GetByIdAsync(int id)
        {
            return await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateAsync(MediaFile file, string? userId = null)
        {
            _db.MediaFiles.Add(file);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await _audit.LogAsync("MediaFile", file.Id, "Create", userId);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, string? caption, string? textContent, IFormFile? newFile, string userId)
        {
            var media = await _db.MediaFiles.FindAsync(id);
            if (media is null)
                return (false, "Media not found");

            // Update text fields
            media.Caption = caption;
            if (media.MediaType == MediaType.Text && textContent != null)
            {
                media.TextContent = textContent;
            }

            // Handle file replacement
            if (newFile is not null && newFile.Length > 0)
            {
                // Only allow file replacement for non‑text types
                if (media.MediaType == MediaType.Text)
                    return (false, "Cannot upload file for text‑only media.");

                // Delete old physical file
                if (!string.IsNullOrEmpty(media.FilePath))
                {
                    var oldFilePath = Path.Combine(GetUploadsRoot(), Path.GetFileName(media.FilePath));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                // Save new file
                var uploadsRoot = GetUploadsRoot();
                var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(newFile.FileName);
                var filePath = Path.Combine(uploadsRoot, fileName);
                var webPath = "/uploads/" + fileName;

                Directory.CreateDirectory(uploadsRoot);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await newFile.CopyToAsync(stream);

                media.FileName = Path.GetFileName(newFile.FileName);
                media.FilePath = webPath;
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync("MediaFile", id, "Update", userId);
            return (true, null);
        }

        private string GetUploadsRoot() =>
            Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "UploadedFiles"));

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, string userId)
        {
            var media = await _db.MediaFiles
                .Include(m => m.PlaylistItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (media is null)
                return (false, "Media not found");

            if (media.PlaylistItems.Any())
            {
                return (false, "Cannot delete media that is used in playlists.");
            }

            _db.MediaFiles.Remove(media);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("MediaFile", id, "Delete", userId);
            return (true, null);
        }
    }
}
