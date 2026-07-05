using Narrowcasting.Models;

namespace Narrowcasting.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task<IEnumerable<Announcement>> GetActiveAsync();
        Task<IEnumerable<Announcement>> GetActiveForDepartmentAsync(int departmentId);
        Task<IEnumerable<Announcement>> GetActiveForScreenAsync(int screenId, int departmentId, bool includeInternal);
        Task<Announcement?> GetByIdAsync(int id);
        Task CreateAsync(Announcement announcement, string userId);
        Task UpdateAsync(Announcement announcement, string userId);
        Task DeleteAsync(int id, string userId);
    }
}
