using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Screens
{
    public class EditModel : PageModel
    {
        private readonly IScreenService _screens;
        private readonly IDepartmentService _departments;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(IScreenService screens, IDepartmentService departments,
                         UserManager<ApplicationUser> userManager)
        {
            _screens = screens;
            _departments = departments;
            _userManager = userManager;
        }

        [BindProperty]
        public Screen Screen { get; set; } = new();

        public SelectList DepartmentSelectList { get; set; } = null!;
        public bool IsNew => Screen.Id == 0;

        private async Task LoadDepartmentsAsync()
        {
            var depts = await _departments.GetAllAsync();
            DepartmentSelectList = new SelectList(depts, "Id", "Name");
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadDepartmentsAsync();

            if (id.HasValue)
            {
                var screen = await _screens.GetByIdAsync(id.Value);
                if (screen is null) return NotFound();

                // Employee can only edit their own department's screens
                if (!User.IsInRole("Admin"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user?.DepartmentId != screen.DepartmentId)
                        return Forbid();
                }

                Screen = screen;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? departmentId)
        {
            await LoadDepartmentsAsync();

            // Map simple posted departmentId into bound Screen if present
            if (departmentId.HasValue && departmentId.Value > 0)
                Screen.DepartmentId = departmentId.Value;

            if (!ModelState.IsValid)
            {
                var entries = ModelState.Select(kvp => new
                {
                    Key = kvp.Key,
                    Errors = kvp.Value?.Errors.Select(e => e.ErrorMessage).Where(s => !string.IsNullOrEmpty(s)).ToArray()
                        ?? Array.Empty<string>()
                }).Where(e => e.Errors.Any()).ToArray();

                var details = string.Join("; ", entries.Select(e => $"{e.Key}: {string.Join(",", e.Errors)}"));
                // Include attempted department id for debugging
                details = $"Attempted DepartmentId={Screen?.DepartmentId ?? -1}; {details}";
                TempData["Error"] = details;
                return Page();
            }

            var userId = _userManager.GetUserId(User)!;

            if (Screen.Id == 0)
            {
                await _screens.CreateAsync(Screen, userId);
                TempData["Success"] = "Scherm aangemaakt.";
                return RedirectToPage("/Admin/Screens/Index");
            }

            var existing = await _screens.GetByIdAsync(Screen.Id);
            if (existing is null) return NotFound();

            // permission check for employee
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != existing.DepartmentId)
                    return Forbid();
            }

            // copy editable properties
            existing.Name = Screen.Name;
            existing.Location = Screen.Location;
            existing.DepartmentId = Screen.DepartmentId;
            existing.IsActive = Screen.IsActive;
            existing.IsStaffScreen = Screen.IsStaffScreen;

            await _screens.UpdateAsync(existing, userId);

            TempData["Success"] = "Scherm bijgewerkt.";
            return RedirectToPage("/Admin/Screens/Index");
        }
    }
}
