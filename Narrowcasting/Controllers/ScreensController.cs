using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScreensController : ControllerBase
    {
        private readonly IScreenService _screens;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScreensController(IScreenService screens, UserManager<ApplicationUser> userManager)
        {
            _screens = screens;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (User.IsInRole("Admin"))
                return Ok(await _screens.GetAllAsync());

            var user = await _userManager.GetUserAsync(User);
            if (user?.DepartmentId is null)
                return Ok(Array.Empty<Screen>());

            return Ok(await _screens.GetByDepartmentAsync(user.DepartmentId.Value));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var screen = await _screens.GetByIdAsync(id);
            if (screen is null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != screen.DepartmentId)
                    return Forbid();
            }

            return Ok(screen);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Screen screen)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _screens.CreateAsync(screen, userId);

            return CreatedAtAction(nameof(GetById), new { id = screen.Id }, screen);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Screen screen)
        {
            if (id != screen.Id) return BadRequest("Route id en body id komen niet overeen.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _screens.GetByIdAsync(id);
            if (existing is null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _screens.UpdateAsync(screen, userId);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _screens.GetByIdAsync(id);
            if (existing is null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _screens.DeleteAsync(id, userId);

            return NoContent();
        }
    }
}
