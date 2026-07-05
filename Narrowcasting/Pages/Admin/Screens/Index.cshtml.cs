using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Screens
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IScreenService _screens;
        private readonly IDepartmentService _departments;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IScreenService screens, IDepartmentService departments,
                          UserManager<ApplicationUser> userManager)
        {
            _screens = screens;
            _departments = departments;
            _userManager = userManager;
        }

        public IEnumerable<Screen> Screens { get; set; } = Enumerable.Empty<Screen>();
        public IEnumerable<Department> Departments { get; set; } = Enumerable.Empty<Department>();

        public async Task OnGetAsync()
        {
            Departments = await _departments.GetAllAsync();

            // AVG: Employee only sees their own department
            if (User.IsInRole("Admin"))
            {
                Screens = await _screens.GetAllAsync();
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                Screens = user?.DepartmentId.HasValue == true
                    ? await _screens.GetByDepartmentAsync(user.DepartmentId.Value)
                    : Enumerable.Empty<Screen>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            var screen = await _screens.GetByIdAsync(id);
            if (screen is null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != screen.DepartmentId)
                    return Forbid();
            }

            await _screens.DeleteAsync(id, userId);
            TempData["Success"] = "Scherm succesvol verwijderd.";
            return RedirectToPage();
        }
    }
}
