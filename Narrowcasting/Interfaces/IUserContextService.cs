using Narrowcasting.Models;

namespace Narrowcasting.Interfaces
{
    /// <summary>
    /// User context service interface to decouple UserManager from application logic.
    /// Reduces coupling and improves testability.
    /// </summary>
    public interface IUserContextService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
        Task<string> GetCurrentUserIdAsync();
        Task<int?> GetCurrentUserDepartmentAsync();
        Task<bool> IsUserAdminAsync();
        Task<bool> CanUserAccessDepartmentAsync(int departmentId);
        //Task<bool> CanUserAccessScreenAsync(int screenId);
    }
}
