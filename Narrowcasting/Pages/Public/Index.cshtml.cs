using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Public
{
    public class IndexModel : PageModel
    {
        private readonly IAnnouncementService _announcements;
        public IndexModel(IAnnouncementService announcements) => _announcements = announcements;

        public IEnumerable<Announcement> Announcements { get; set; } = Enumerable.Empty<Announcement>();

        public async Task OnGetAsync() =>
            Announcements = (await _announcements.GetActiveAsync()).Where(a => !a.IsInternal).Take(6);
    }
}
