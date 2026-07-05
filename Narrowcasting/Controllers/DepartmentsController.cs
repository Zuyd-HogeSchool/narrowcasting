using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departments;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentsController(IDepartmentService departments, UserManager<ApplicationUser> userManager)
        {
            _departments = departments;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId is null)
                    return Ok(Array.Empty<Department>());

                var department = await _departments.GetByIdAsync(user.DepartmentId.Value);
                return Ok(department is null ? Array.Empty<Department>() : new[] { department });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _departments.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _departments.GetByIdAsync(id);
            if (department is null) return NotFound();
            return Ok(department);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Department department)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _departments.CreateAsync(department, userId);

            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Department department)
        {
            if (id != department.Id) return BadRequest("Route id en body id komen niet overeen.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _departments.GetByIdAsync(id);
            if (existing is null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _departments.UpdateAsync(department, userId);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _departments.GetByIdAsync(id);
            if (existing is null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            await _departments.DeleteAsync(id, userId);

            return NoContent();
        }
    }
}
