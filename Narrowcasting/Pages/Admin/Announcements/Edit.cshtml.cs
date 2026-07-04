using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Announcements
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IDepartmentService _departmentService;
        private readonly IScreenService _screenService;
        private readonly IMediaFileService _mediaFileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(
            IAnnouncementService announcementService,
            IDepartmentService departmentService,
            IScreenService screenService,
            IMediaFileService mediaFileService,
            UserManager<ApplicationUser> userManager)
        {
            _announcementService = announcementService;
            _departmentService = departmentService;
            _screenService = screenService;
            _mediaFileService = mediaFileService;
            _userManager = userManager;
        }

        [BindProperty]
        public Announcement Item { get; set; } = new();
        public SelectList DepartmentSelectList { get; set; } = null!;
        public SelectList ScreenSelectList { get; set; } = null!;
        public SelectList MediaFileSelectList { get; set; } = null!;
        public bool IsNew => Item.Id == 0;


        private async Task LoadLookupsAsync()
        {
            var depts = await _departmentService.GetAllAsync();
            DepartmentSelectList = new SelectList(depts, "Id", "Name");

            var screens = await _screenService.GetAllAsync();
            ScreenSelectList = new SelectList(
                screens.Select(s => new
                {
                    s.Id,
                    Name = $"{s.Name} ({s.Department?.Name ?? "Geen afdeling"})"
                }),
                "Id",
                "Name");

            var mediaFiles = await _mediaFileService.GetAllOrderedAsync();
            MediaFileSelectList = new SelectList(
                mediaFiles.Select(m => new
                {
                    m.Id,
                    Name = string.IsNullOrWhiteSpace(m.Caption)
                        ? $"{m.FileName} ({m.MediaType})"
                        : $"{m.Caption} ({m.MediaType})"
                }),
                "Id",
                "Name");
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadLookupsAsync();
            if (id.HasValue)
            {
                var item = await _announcementService.GetByIdAsync(id.Value);
                if (item is null) return NotFound();
                Item = item;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadLookupsAsync();
            if (!ModelState.IsValid) return Page();

            if (Item.ScreenId.HasValue)
            {
                var screen = await _screenService.GetByIdAsync(Item.ScreenId.Value);
                if (screen is null)
                {
                    ModelState.AddModelError("Item.ScreenId", "Selecteer een geldig scherm.");
                    return Page();
                }

                Item.DepartmentId = screen.DepartmentId;
            }

            if (Item.MediaFileId.HasValue && await _mediaFileService.GetByIdAsync(Item.MediaFileId.Value) is null)
            {
                ModelState.AddModelError("Item.MediaFileId", "Selecteer een geldig mediabestand.");
                return Page();
            }

            var userId = _userManager.GetUserId(User)!;
            Item.CreatedById = userId;

            if (Item.Id == 0)
                await _announcementService.CreateAsync(Item, userId);
            else
                await _announcementService.UpdateAsync(Item, userId);

            TempData["Success"] = "Aankondiging opgeslagen.";
            return RedirectToPage("/Admin/Announcements/Index");
        }
    }
}
