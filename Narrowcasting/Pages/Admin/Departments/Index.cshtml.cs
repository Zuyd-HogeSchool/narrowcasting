using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using static Narrowcasting.Services.DepartmentService;

namespace Narrowcasting.Pages.Admin.Departments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDepartmentService _departments;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IDepartmentService departments, UserManager<ApplicationUser> userManager)
        {
            _departments = departments;
            _userManager = userManager;
        }

        public IEnumerable<Department> Departments { get; set; } = [];

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Admin"))
            {
                Departments = await _departments.GetAllAsync();
                return;
            }

            // Non?admin: only their own department
            var user = await _userManager.GetUserAsync(User);
            if (user?.DepartmentId is null)
            {
                Departments = [];
                return;
            }
            var department = await _departments.GetByIdAsync(user.DepartmentId.Value);
            Departments = department is null ? [] : new[] { department };
        }


        public async Task<IActionResult> OnPostSaveAsync(Department dept)
        {
            if (!ModelState.IsValid) return Page();
            var userId = _userManager.GetUserId(User)!;

            if (dept.Id == 0)
                await _departments.CreateAsync(dept, userId);
            else
                await _departments.UpdateAsync(dept, userId);

            TempData["Success"] = "Afdeling opgeslagen.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                TempData["Error"] = "Gebruiker niet gevonden.";
                return RedirectToPage("./Index");
            }

            var result = await _departments.DeleteAsync(id, userId);

            switch (result)
            {
                case DepartmentDeleteResult.NotFound:
                    TempData["Error"] = "Afdeling niet gevonden.";
                    break;
                case DepartmentDeleteResult.HasRelatedScreens:
                    TempData["Error"] = "Deze afdeling kan niet worden verwijderd omdat er nog schermen aan gekoppeld zijn.";
                    break;
                case DepartmentDeleteResult.Success:
                    TempData["Success"] = "Afdeling succesvol verwijderd.";
                    break;
            }

            return RedirectToPage("./Index");
        }
    }
}
