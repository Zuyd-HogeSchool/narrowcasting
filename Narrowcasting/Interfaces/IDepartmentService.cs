using Narrowcasting.Models;
using static Narrowcasting.Services.DepartmentService;

namespace Narrowcasting.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task CreateAsync(Department department, string userId);
        Task UpdateAsync(Department department, string userId);
        Task<DepartmentDeleteResult> DeleteAsync(int id, string userId);
    }
}
