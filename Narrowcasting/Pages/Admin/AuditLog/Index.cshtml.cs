using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;

namespace Narrowcasting.Pages.Admin.AuditLog
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAuditService _audit;
        public IndexModel(IAuditService audit) => _audit = audit;

        public IEnumerable<Models.AuditLog> Logs { get; set; } = [];

        public string? FilterUserId { get; set; }
        public DateTime? FilterFrom { get; set; }
        public DateTime? FilterTo { get; set; }

        public async Task OnGetAsync(string? userId, DateTime? from, DateTime? to)
        {
            FilterUserId = userId;
            FilterFrom = from;
            FilterTo = to;
            Logs = await _audit.GetLogsAsync(userId, from, to);
        }
    }
}
