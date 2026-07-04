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
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDepartmentService _departments;

        public CreateModel(UserManager<ApplicationUser> userManager, IDepartmentService departments)
        {
            _userManager = userManager;
            _departments = departments;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList DepartmentSelectList { get; set; } = null!;

        public class InputModel
        {
            [Required(ErrorMessage = "Naam is verplicht")]
            [MaxLength(100)]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "E-mailadres is verplicht")]
            [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Wachtwoord is verplicht")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required]
            public string Role { get; set; } = "Employee";

            public int? DepartmentId { get; set; }
        }

        public async Task OnGetAsync()
        {
            var depts = await _departments.GetAllAsync();
            DepartmentSelectList = new SelectList(depts, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var depts = await _departments.GetAllAsync();
            DepartmentSelectList = new SelectList(depts, "Id", "Name");

            // Business rule: Employee must have a department; Admin must not.
            if (Input.Role == "Employee" && Input.DepartmentId is null)
                ModelState.AddModelError("Input.DepartmentId", "Employee-gebruikers moeten een afdeling hebben.");

            if (!ModelState.IsValid) return Page();

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FullName = Input.FullName,
                DepartmentId = Input.Role == "Admin" ? null : Input.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            await _userManager.AddToRoleAsync(user, Input.Role);

            TempData["Success"] = $"Gebruiker '{Input.FullName}' aangemaakt.";
            return RedirectToPage("/Admin/Users/Index");
        }
    }
}
