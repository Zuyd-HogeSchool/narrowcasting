using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Playlists
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IPlaylistService _playlists;
        private readonly IScreenService _screens;
        private readonly IMediaFileService _mediaFiles;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(IPlaylistService playlists, IScreenService screens,
                         IMediaFileService mediaFiles, UserManager<ApplicationUser> userManager)
        {
            _playlists = playlists;
            _screens = screens;
            _mediaFiles = mediaFiles;
            _userManager = userManager;
        }

        [BindProperty]
        public Playlist Playlist { get; set; } = new();

        public SelectList ScreenSelectList { get; set; } = null!;
        public SelectList MediaFileSelectList { get; set; } = null!;
        public bool IsNew => Playlist.Id == 0;

        private async Task LoadDropdownsAsync()
        {
            IEnumerable<Screen> screens = User.IsInRole("Admin")
                ? await _screens.GetAllAsync()
                : await GetEmployeeScreensAsync();

            ScreenSelectList = new SelectList(screens, "Id", "Name");

            var mediaFiles = await _mediaFiles.GetAllOrderedAsync();
            var mediaItems = mediaFiles.Select(m => new { m.Id, Text = m.FileName + (string.IsNullOrEmpty(m.Caption) ? "" : " - " + m.Caption) });
            MediaFileSelectList = new SelectList(mediaItems, "Id", "Text");
        }

        private async Task<IEnumerable<Screen>> GetEmployeeScreensAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.DepartmentId.HasValue == true
                ? await _screens.GetByDepartmentAsync(user.DepartmentId.Value)
                : Enumerable.Empty<Screen>();
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadDropdownsAsync();

            if (id.HasValue)
            {
                var playlist = await _playlists.GetByIdAsync(id.Value);
                if (playlist is null) return NotFound();
                if (playlist.Screen is null) return NotFound();

                // Employee can only manage playlists on their own department's screens
                if (!User.IsInRole("Admin"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user?.DepartmentId != playlist.Screen.DepartmentId)
                        return Forbid();
                }

                Playlist = playlist;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveDetailsAsync()
        {
            await LoadDropdownsAsync();
            if (!ModelState.IsValid) return Page();

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            var selectedScreen = await _screens.GetByIdAsync(Playlist.ScreenId);
            if (selectedScreen is null)
            {
                ModelState.AddModelError("Playlist.ScreenId", "Selecteer een geldig scherm.");
                return Page();
            }

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != selectedScreen.DepartmentId)
                    return Forbid();
            }

            if (Playlist.Id == 0)
            {
                Playlist.CreatedById = userId;
                await _playlists.CreateAsync(Playlist, userId);
                TempData["Success"] = "Playlist aangemaakt.";
                return RedirectToPage(new { id = Playlist.Id });
            }

            // For updates, load existing entity and copy only editable scalar fields to avoid
            // unintentionally overwriting navigation collections (Items) which are not part of the details form.
            var existing = await _playlists.GetByIdAsync(Playlist.Id);
            if (existing is null) return NotFound();
            if (existing.Screen is null) return NotFound();

            // permission check for employee
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != existing.Screen.DepartmentId)
                    return Forbid();
            }

            // permission check for employee
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != existing.Screen.DepartmentId)
                    return Forbid();
            }

            existing.Name = Playlist.Name;
            existing.ScreenId = Playlist.ScreenId;

            await _playlists.UpdateAsync(existing, userId);
            TempData["Success"] = "Playlist bijgewerkt.";
            return RedirectToPage(new { id = Playlist.Id });
        }

        public async Task<IActionResult> OnPostAddItemAsync(int playlistId, int mediaFileId, int durationSeconds)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            if (mediaFileId <= 0)
            {
                TempData["Error"] = "Selecteer een mediabestand.";
                return RedirectToPage(new { id = playlistId });
            }

            if (durationSeconds < 1 || durationSeconds > 3600)
            {
                TempData["Error"] = "De duur moet tussen 1 en 3600 seconden liggen.";
                return RedirectToPage(new { id = playlistId });
            }

            var current = await _playlists.GetByIdAsync(playlistId);
            if (current is null) return NotFound();
            if (current.Screen is null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != current.Screen.DepartmentId)
                    return Forbid();
            }

            var nextOrder = current.Items.Count;

            var item = new PlaylistItem
            {
                MediaFileId = mediaFileId,
                Order = nextOrder,
                DurationSeconds = durationSeconds
            };

            try
            {
                await _playlists.AddItemAsync(playlistId, item, userId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // e.g. "max 20 items" rule from PlaylistService
            }

            return RedirectToPage(new { id = playlistId });
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int itemId, int playlistId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            var playlist = await _playlists.GetByIdAsync(playlistId);
            if (playlist is null) return NotFound();
            if (playlist.Screen is null) return NotFound();

            if (!playlist.Items.Any(i => i.Id == itemId))
                return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != playlist.Screen.DepartmentId)
                    return Forbid();
            }

            await _playlists.RemoveItemAsync(itemId, userId);
            return RedirectToPage(new { id = playlistId });
        }
    }
}
