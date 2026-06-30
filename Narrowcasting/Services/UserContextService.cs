using Microsoft.AspNetCore.Identity;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Services
{
    /// <summary>
    /// User context service to reduce direct UserManager coupling across the app.
    /// Centralizes user authorization and permission logic.
    /// </summary>
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IScreenService _screenService;

        public UserContextService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var httpUser = _httpContextAccessor.HttpContext?.User;
            if (httpUser is null) return null;
            return await _userManager.GetUserAsync(httpUser);
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Id ?? throw new InvalidOperationException("User not authenticated");
        }


        public async Task<int?> GetCurrentUserDepartmentAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.DepartmentId;
        }

        public async Task<bool> IsUserAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            return user is not null && await _userManager.IsInRoleAsync(user, "Admin");
        }

        public async Task<bool> CanUserAccessDepartmentAsync(int departmentId)
        {
            if (await IsUserAdminAsync()) return true;
            var userDeptId = await GetCurrentUserDepartmentAsync();
            return userDeptId == departmentId;
        }
    }
}
