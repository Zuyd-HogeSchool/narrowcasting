using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Playlists
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPlaylistService _playlists;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IPlaylistService playlists, UserManager<ApplicationUser> userManager)
        {
            _playlists = playlists;
            _userManager = userManager;
        }

        public IEnumerable<Playlist> Playlists { get; set; } = [];

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Admin"))
            {
                Playlists = await _playlists.GetAllAsync();
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                Playlists = user?.DepartmentId.HasValue == true
                    ? await _playlists.GetByDepartmentAsync(user.DepartmentId.Value)
                    : [];
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _playlists.DeleteAsync(id, userId);
            TempData["Success"] = "Playlist verwijderd.";
            return RedirectToPage();
        }
    }
}
