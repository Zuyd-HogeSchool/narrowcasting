using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Admin.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public int DepartmentCount { get; set; }
        public int ScreenCount { get; set; }
        public int PlaylistCount { get; set; }
        public int AnnouncementCount { get; set; }
        public List<Screen> RecentScreens { get; set; } = new();
        public List<Models.AuditLog> RecentLogs { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Admins see everything; Employees see their own department
            if (User.IsInRole("Admin"))
            {
                DepartmentCount = await _db.Departments.CountAsync();
                ScreenCount = await _db.Screens.CountAsync();
                PlaylistCount = await _db.Playlists.CountAsync();
                AnnouncementCount = await _db.Announcements.CountAsync();
                RecentScreens = await _db.Screens.Include(s => s.Department).Take(5).ToListAsync();
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                var deptId = user?.DepartmentId;

                ScreenCount = await _db.Screens.CountAsync(s => s.DepartmentId == deptId);
                PlaylistCount = await _db.Playlists.CountAsync(p => p.Screen != null && p.Screen.DepartmentId == deptId);
                AnnouncementCount = await _db.Announcements.CountAsync(a => a.DepartmentId == deptId);
                RecentScreens = await _db.Screens.Include(s => s.Department)
                                                      .Where(s => s.DepartmentId == deptId)
                                                      .Take(5).ToListAsync();
            }

            RecentLogs = await _db.AuditLogs.Include(a => a.User)
                                            .OrderByDescending(a => a.Timestamp)
                                            .Take(8).ToListAsync();
        }
    }
}