using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager) =>
            _userManager = userManager;

        public List<UserRow> Users { get; set; } = [];
        public string? CurrentUserId { get; set; }

        public class UserRow
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string Role { get; set; } = "Employee";
            public string? DepartmentName { get; set; }
            public bool IsActive { get; set; }
            public bool IsPendingApproval { get; set; }
        }

        public async Task OnGetAsync()
        {
            CurrentUserId = _userManager.GetUserId(User);

            var allUsers = await _userManager.Users
                .Include(u => u.Department)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                Users.Add(new UserRow
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = roles.Contains("Admin") ? "Admin" : "Employee",
                    DepartmentName = u.Department?.Name,
                    IsActive = u.IsActive,
                    IsPendingApproval = !u.IsActive && !roles.Contains("Admin") && u.DepartmentId is null
                });
            }
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return RedirectToPage();

            // Never let an admin deactivate themselves by accident
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "Je kunt jezelf niet deactiveren.";
                return RedirectToPage();
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            // Lock the account so the employees truly can't log in when deactivated
            if (!user.IsActive)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            else
                await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["Success"] = user.IsActive ? "Gebruiker geactiveerd." : "Gebruiker gedeactiveerd.";
            return RedirectToPage();
        }
    }
}
