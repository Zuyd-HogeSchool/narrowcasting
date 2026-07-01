using Narrowcasting.Models;

namespace Narrowcasting.Interfaces
{
    public interface IScreenService
    {
        Task<IEnumerable<Screen>> GetAllAsync();
        Task<IEnumerable<Screen>> GetActiveAsync();
        Task<IEnumerable<Screen>> GetByDepartmentAsync(int departmentId);
        Task<Screen?> GetByIdAsync(int id);
        Task CreateAsync(Screen screen, string userId);
        Task UpdateAsync(Screen screen, string userId);
        Task DeleteAsync(int id, string userId);
    }
}
