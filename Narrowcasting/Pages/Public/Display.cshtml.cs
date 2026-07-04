using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Enums;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Public
{
    public class DisplayModel : PageModel
    {
        private readonly IScreenService _screens;
        private readonly IAnnouncementService _announcements;
        private readonly IPlaylistService _playlists;

        public DisplayModel(IScreenService screens, IAnnouncementService announcements, IPlaylistService playlists)
        {
            _screens = screens;
            _announcements = announcements;
            _playlists = playlists;
        }

        public Screen? Screen { get; set; }
        public IEnumerable<Announcement> Announcements { get; set; } = Enumerable.Empty<Announcement>();
        public List<DisplaySlide> Slides { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int screenId)
        {
            Screen = await _screens.GetByIdAsync(screenId);
            if (Screen is null) return Page();

            var playlist = await _playlists.GetByScreenAsync(screenId);
            if (playlist is not null)
            {
                Slides.AddRange(playlist.Items
                    .OrderBy(i => i.Order)
                    .Select(i => DisplaySlide.FromMedia(i.MediaFile, i.DurationSeconds)));
            }

            Announcements = await _announcements.GetActiveForScreenAsync(screenId, Screen.DepartmentId, Screen.IsStaffScreen);
            Slides.AddRange(Announcements.Select(DisplaySlide.FromAnnouncement));

            return Page();
        }

        public sealed record DisplaySlide(
            string Kind,
            string Title,
            string? Body,
            string? Department,
            string? FilePath,
            string MediaType,
            int DurationMs)
        {
            public static DisplaySlide FromMedia(MediaFile media, int durationSeconds)
            {
                var title = string.IsNullOrWhiteSpace(media.Caption) ? "" : media.Caption!;
                return new DisplaySlide(
                    "media",
                    title,
                    media.TextContent,
                    null,
                    media.FilePath,
                    media.MediaType.ToString(),
                    Math.Clamp(durationSeconds, 3, 30) * 1000);
            }

            public static DisplaySlide FromAnnouncement(Announcement announcement)
            {
                var media = announcement.MediaFile;
                return new DisplaySlide(
                    "announcement",
                    announcement.Title,
                    announcement.Content,
                    announcement.Department?.Name,
                    media?.FilePath,
                    media?.MediaType.ToString() ?? Narrowcasting.Enums.MediaType.Text.ToString(),
                    4000);
            }
        }
    }
}
