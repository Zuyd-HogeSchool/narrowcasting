using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Services
{
    public class ScreenService : IScreenService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _audit;
        public ScreenService(ApplicationDbContext db, IAuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<IEnumerable<Screen>> GetAllAsync()
        {
            return await _db.Screens.Include(s => s.Department)
                                    .OrderBy(s => s.Name)
                                    .ToListAsync();
        }

        public async Task<IEnumerable<Screen>> GetActiveAsync()
        {
            return await _db.Screens.Include(s => s.Department)
                                    .Where(s => s.IsActive)
                                    .OrderBy(s => s.Name)
                                    .ToListAsync();
        }

        public async Task<IEnumerable<Screen>> GetByDepartmentAsync(int departmentId)
        {
            return await _db.Screens.Include(s => s.Department)
                                    .Where(s => s.DepartmentId == departmentId)
                                    .OrderBy(s => s.Name)
                                    .ToListAsync();
        }

        public async Task<Screen?> GetByIdAsync(int id)
        {
            return await _db.Screens.Include(s => s.Department)
                                    .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateAsync(Screen screen, string userId)
        {
            _db.Screens.Add(screen);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Screen", screen.Id, "Create", userId);
        }

        public async Task UpdateAsync(Screen screen, string userId)
        {
            _db.Screens.Update(screen);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Screen", screen.Id, "Update", userId);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var screen = await _db.Screens.FindAsync(id);

            if (screen is null) return;

            _db.Screens.Remove(screen);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Screen", id, "Delete", userId);
        }
    }
}
