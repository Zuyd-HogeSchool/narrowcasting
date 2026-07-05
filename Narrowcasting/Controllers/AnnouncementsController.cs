
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcements;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnnouncementsController(IAnnouncementService announcements, UserManager<ApplicationUser> userManager)
        {
            _announcements = announcements;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive([FromQuery] int? departmentId)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return Ok(await _announcements.GetAllAsync());

                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId.HasValue == true)
                    return Ok(await _announcements.GetActiveForDepartmentAsync(user.DepartmentId.Value));
            }

            var result = departmentId.HasValue
                ? await _announcements.GetActiveForDepartmentAsync(departmentId.Value)
                : await _announcements.GetActiveAsync();

            return Ok(result.Where(a => !a.IsInternal));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var announcement = await _announcements.GetByIdAsync(id);
            if (announcement is null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != announcement.DepartmentId)
                    return Forbid();
            }

            return Ok(announcement);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([FromBody] Announcement announcement)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != announcement.DepartmentId)
                    return Forbid("Je kunt alleen aankondigingen aanmaken voor je eigen afdeling.");
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            announcement.CreatedById = userId;
            await _announcements.CreateAsync(announcement, userId);

            return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, announcement);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, [FromBody] Announcement announcement)
        {
            if (id != announcement.Id) return BadRequest("Route id en body id komen niet overeen.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _announcements.GetByIdAsync(id);
            if (existing is null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != existing.DepartmentId)
                    return Forbid();
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _announcements.UpdateAsync(announcement, userId);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _announcements.GetByIdAsync(id);
            if (existing is null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != existing.DepartmentId)
                    return Forbid();
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _announcements.DeleteAsync(id, userId);

            return NoContent();
        }
    }
}
