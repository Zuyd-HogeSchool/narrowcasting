using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Narrowcasting.Enums;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Pages.Admin.MediaFiles
{
    [Authorize]
    [RequestFormLimits(MultipartBodyLengthLimit = 300 * 1024 * 1024)]
    [RequestSizeLimit(300 * 1024 * 1024)]
    public class CreateModel : PageModel
    {
        private readonly IMediaFileService _mediaFiles;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateModel> _logger;
        private const long MaxImageBytes = 10 * 1024 * 1024;
        private const long MaxVideoBytes = 250 * 1024 * 1024;
        private const long MaxAudioBytes = 50 * 1024 * 1024;

        public CreateModel(
            IMediaFileService mediaFiles,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger)
        {
            _mediaFiles = mediaFiles;
            _env = env;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        [MaxLength(10000)]
        public string? TextContent { get; set; }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Selecteer een mediatype.")]
        public MediaType SelectedMediaType { get; set; } = MediaType.Image;

        [BindProperty]
        [MaxLength(500)]
        public string? Caption { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ----- validation -----
            if (SelectedMediaType != MediaType.Text && Upload is null)
                ModelState.AddModelError("Upload", "Select a file to upload.");

            if (SelectedMediaType == MediaType.Text && string.IsNullOrWhiteSpace(TextContent))
                ModelState.AddModelError("TextContent", "Please enter some text.");

            if (!ModelState.IsValid)
                return Page();

            if (!IsSupportedMediaType(SelectedMediaType))
            {
                ModelState.AddModelError("SelectedMediaType", "Selecteer image, video, audio of text.");
                return Page();
            }

            // ----- file‑type / size checks (non‑text only) -----
            if (SelectedMediaType != MediaType.Text)
            {
                if (Upload!.Length == 0)
                {
                    ModelState.AddModelError("Upload", "The selected file is empty.");
                    return Page();
                }

                var ext = Path.GetExtension(Upload.FileName);
                var rules = GetUploadRules(SelectedMediaType);

                if (Upload.Length > rules.MaxBytes)
                {
                    ModelState.AddModelError("Upload",
                        $"Het bestand is te groot. Maximum is {rules.MaxMegabytes} MB " +
                        $"voor {SelectedMediaType.ToString().ToLowerInvariant()}.");
                    return Page();
                }

                if (!rules.ContentTypeSet.Contains(Upload.ContentType) ||
                    !rules.ExtensionSet.Contains(ext))
                {
                    ModelState.AddModelError("Upload", rules.ErrorMessage);
                    return Page();
                }
            }

            // ----- build entity -----
            var userId = _userManager.GetUserId(User);
            var mediaFile = new MediaFile
            {
                FileName = SelectedMediaType == MediaType.Text
                    ? "Text"
                    : Path.GetFileName(Upload!.FileName),
                FilePath = string.Empty,   // will be set below for files
                MediaType = SelectedMediaType,
                UploadedById = userId,
                Caption = Caption,
                TextContent = SelectedMediaType == MediaType.Text ? TextContent : null
            };

            // ----- save physical file (non‑text) -----
            if (SelectedMediaType != MediaType.Text)
            {
                var savedWebPath = await SaveUploadedFileAsync(Upload!);
                if (savedWebPath is null)  // error already added to ModelState
                    return Page();

                mediaFile.FilePath = savedWebPath;
            }

            // ----- persist record -----
            try
            {
                await _mediaFiles.CreateAsync(mediaFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create media record");
                ModelState.AddModelError(string.Empty, "Something went wrong saving the record.");
                return Page();
            }

            TempData["Success"] = "Bestand geupload.";
            return RedirectToPage("/Admin/MediaFiles/Index");
        }

        private async Task<string?> SaveUploadedFileAsync(IFormFile file)
        {
            var uploadsRoot = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "UploadedFiles"));
            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsRoot, fileName);
            var webPath = "/uploads/" + fileName;

            try
            {
                Directory.CreateDirectory(uploadsRoot);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return webPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save uploaded file {FileName}", file.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                ModelState.AddModelError("Upload", "Upload failed. Please try again or choose another file.");
                return null;
            }
        }

        private static bool IsSupportedMediaType(MediaType t) =>
        t is MediaType.Image or MediaType.Video or MediaType.Audio or MediaType.Text;

        private static UploadRules GetUploadRules(MediaType type) => type switch
        {
            MediaType.Image => new UploadRules(MaxImageBytes,
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" },
                new[] { "image/jpeg", "image/png", "image/gif", "image/webp" },
                "Only image files (jpg, png, gif, webp) are allowed."),
            MediaType.Video => new UploadRules(MaxVideoBytes,
                new[] { ".mp4", ".webm", ".mov", ".m4v" },
                new[] { "video/mp4", "video/webm", "video/quicktime", "video/x-m4v" },
                "Only video files (mp4, webm, mov, m4v) are allowed."),
            MediaType.Audio => new UploadRules(MaxAudioBytes,
                new[] { ".mp3", ".wav", ".ogg", ".m4a" },
                new[] { "audio/mpeg", "audio/wav", "audio/x-wav", "audio/ogg", "audio/mp4", "audio/x-m4a" },
                "Only audio files (mp3, wav, ogg, m4a) are allowed."),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        private sealed record UploadRules(
            long MaxBytes,
            IEnumerable<string> Extensions,
            IEnumerable<string> ContentTypes,
            string ErrorMessage)
        {
            public long MaxMegabytes => MaxBytes / 1024 / 1024;
            public HashSet<string> ExtensionSet => new(Extensions, StringComparer.OrdinalIgnoreCase);
            public HashSet<string> ContentTypeSet => new(ContentTypes, StringComparer.OrdinalIgnoreCase);
        }
    }
}

