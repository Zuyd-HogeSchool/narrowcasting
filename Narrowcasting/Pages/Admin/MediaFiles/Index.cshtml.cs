using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.MediaFiles
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMediaFileService _mediaFiles;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public string? EditCaption { get; set; }

        [BindProperty]
        public string? EditTextContent { get; set; }

        [BindProperty]
        public IFormFile? EditFile { get; set; }

        public IndexModel(IMediaFileService mediaFiles, UserManager<ApplicationUser> userManager)
        {
            _mediaFiles = mediaFiles;
            _userManager = userManager;
        }

        public IEnumerable<MediaFile> MediaFiles { get; set; } = Enumerable.Empty<MediaFile>();

        public async Task OnGetAsync()
        {
            MediaFiles = await _mediaFiles.GetAllOrderedAsync();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                TempData["Error"] = "Gebruiker niet gevonden.";
                return RedirectToPage();
            }

            var result = await _mediaFiles.UpdateAsync(EditId, EditCaption, EditTextContent, EditFile, userId);
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Bestand bijgewerkt.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string userId)
        {
            await _mediaFiles.DeleteAsync(id, userId);

            return RedirectToPage();
        }
    }
}
