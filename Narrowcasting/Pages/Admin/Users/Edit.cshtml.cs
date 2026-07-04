using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDepartmentService _departments;

        public EditModel(UserManager<ApplicationUser> userManager, IDepartmentService departments)
        {
            _userManager = userManager;
            _departments = departments;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList DepartmentSelectList { get; set; } = null!;

        public class InputModel
        {
            public string Id { get; set; } = string.Empty;

            [Required, MaxLength(100)]
            public string FullName { get; set; } = string.Empty;

            public string? Email { get; set; }

            [Required]
            public string Role { get; set; } = "Employee";

            public int? DepartmentId { get; set; }

            public bool IsActive { get; set; }
        }

        private async Task LoadDepartmentsAsync()
        {
            var depts = await _departments.GetAllAsync();
            DepartmentSelectList = new SelectList(depts, "Id", "Name");
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            await LoadDepartmentsAsync();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            Input = new InputModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.Contains("Admin") ? "Admin" : "Employee",
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDepartmentsAsync();

            if (Input.Role == "Employee" && Input.DepartmentId is null)
                ModelState.AddModelError("Input.DepartmentId", "Employee-gebruikers moeten een afdeling hebben.");

            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user is null) return NotFound();

            user.FullName = Input.FullName;
            user.DepartmentId = Input.Role == "Admin" ? null : Input.DepartmentId;
            user.IsActive = Input.Role == "Admin" || Input.DepartmentId.HasValue;

            await _userManager.UpdateAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, user.IsActive ? null : DateTimeOffset.MaxValue);

            // Sync role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(Input.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, Input.Role);
            }

            TempData["Success"] = "Gebruiker bijgewerkt.";
            return RedirectToPage("/Admin/Users/Index");
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Wachtwoord gereset."
                : string.Join("; ", result.Errors.Select(e => e.Description));

            return RedirectToPage(new { id });
        }
    }
}
