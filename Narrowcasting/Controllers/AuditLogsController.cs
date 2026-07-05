using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Narrowcasting.Interfaces;

namespace Narrowcasting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditLogsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var logs = await _auditService.GetLogsAsync(userId, from, to);
            return Ok(logs);
        }
    }
}
