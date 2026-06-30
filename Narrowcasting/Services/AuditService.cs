using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        public AuditService(ApplicationDbContext db) => _db = db;

        public async Task LogAsync(string entity, int entityId, string action, string userId, string? details = null)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entity,
                EntityId = entityId.ToString(),
                Details = details,
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? userId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(a => a.UserId == userId);
            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.Timestamp <= to.Value);

            return await query.OrderByDescending(a => a.Timestamp).Take(500).ToListAsync();
        }
    }
}
