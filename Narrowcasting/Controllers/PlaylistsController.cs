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
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistService _playlists;
        private readonly IPlaylistItemService _playlistItems;
        private readonly IScreenService _screens;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlaylistsController(IPlaylistService playlists, IPlaylistItemService playlistItems, IScreenService screens,
                                   UserManager<ApplicationUser> userManager)
        {
            _playlists = playlists;
            _playlistItems = playlistItems;
            _screens = screens;
            _userManager = userManager;
        }

        [HttpGet("{screenId:int}")]
        public async Task<IActionResult> GetByScreen(int screenId)
        {
            var screen = await _screens.GetByIdAsync(screenId);
            if (screen is null) return NotFound("Scherm niet gevonden.");

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != screen.DepartmentId)
                    return Forbid();
            }

            var playlist = await _playlists.GetByScreenAsync(screenId);
            if (playlist is null) return NotFound("Geen playlist gevonden voor dit scherm.");

            return Ok(playlist);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([FromBody] Playlist playlist)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var screen = await _screens.GetByIdAsync(playlist.ScreenId);
            if (screen is null) return BadRequest("Opgegeven ScreenId bestaat niet.");

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != screen.DepartmentId)
                    return Forbid();
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            playlist.CreatedById = userId;
            await _playlists.CreateAsync(playlist, userId);

            return CreatedAtAction(nameof(GetByScreen), new { screenId = playlist.ScreenId }, playlist);
        }

        [HttpPost("{id:int}/items")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AddItem(int id, [FromBody] PlaylistItem item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            var playlist = await _playlists.GetByIdAsync(id);
            if (playlist is null) return NotFound("Playlist niet gevonden.");
            if (playlist.Screen is null) return BadRequest("Playlist heeft geen gekoppeld scherm.");

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != playlist.Screen.DepartmentId)
                    return Forbid();
            }

            try
            {
                await _playlists.AddItemAsync(id, item, userId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("items/{itemId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Challenge();

            var item = await _playlistItems.GetByIdAsync(itemId);
            if (item is null) return NotFound("Playlist item niet gevonden.");

            var playlist = await _playlists.GetByIdAsync(item.PlaylistId);
            if (playlist is null) return NotFound("Playlist niet gevonden.");
            if (playlist.Screen is null) return BadRequest("Playlist heeft geen gekoppeld scherm.");

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DepartmentId != playlist.Screen.DepartmentId)
                    return Forbid();
            }

            await _playlists.RemoveItemAsync(itemId, userId);
            return NoContent();
        }
    }
}
