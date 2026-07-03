using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin
{
    /// <summary>
    /// Base page model for admin pages to reduce code duplication and improve cohesion.
    /// Provides common functionality: user context, authorization, TempData messages.
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    public abstract class AdminPageModel : PageModel
    {
        protected readonly UserManager<ApplicationUser> UserManager;

        protected AdminPageModel(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        /// <summary>
        /// Get current user's ID from claims.
        /// </summary>
        protected string GetCurrentUserId()
        {
            return UserManager.GetUserId(User) ?? throw new InvalidOperationException("User not authenticated");
        }

        /// <summary>
        /// Get current user object.
        /// </summary>
        protected async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return await UserManager.GetUserAsync(User);
        }

        /// <summary>
        /// Check if current user is admin.
        /// </summary>
        protected bool IsAdmin => User.IsInRole("Admin");

        /// <summary>
        /// Check if current user is employee.
        /// </summary>
        protected bool IsEmployee => User.IsInRole("Employee");

        /// <summary>
        /// Get current user's department ID (for employees).
        /// </summary>
        protected async Task<int?> GetUserDepartmentIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.DepartmentId;
        }

        /// <summary>
        /// Verify employee can access resource in their department.
        /// </summary>
        protected async Task<bool> CanAccessDepartmentAsync(int resourceDepartmentId)
        {
            if (IsAdmin) return true;
            var userDeptId = await GetUserDepartmentIdAsync();
            return userDeptId == resourceDepartmentId;
        }

        /// <summary>
        /// Set success message in TempData.
        /// </summary>
        protected void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Set error message in TempData.
        /// </summary>
        protected void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }
    }
}
