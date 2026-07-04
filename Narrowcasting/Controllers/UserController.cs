using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Models;

namespace Narrowcasting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            var user = await _userManager.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) return NotFound(new { message = "Gebruiker niet gevonden." });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                roles,
                isAdmin = roles.Contains("Admin"),
                isEmployee = roles.Contains("Employee"),
                isActive = user.IsActive,
                isPendingApproval = !user.IsActive && roles.Contains("Employee") && user.DepartmentId is null,
                department = user.Department is null
                    ? null
                    : new
                    {
                        id = user.Department.Id,
                        name = user.Department.Name
                    }
            });
        }
    }
}
