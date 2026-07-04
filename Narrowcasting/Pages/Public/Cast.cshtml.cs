using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using System.Linq;

namespace Narrowcasting.Pages.Public
{
    public class CastModel : PageModel
    {
        private readonly IDepartmentService _departments;
        private readonly IScreenService _screens;

        public CastModel(IDepartmentService departments, IScreenService screens)
        {
            _departments = departments;
            _screens = screens;
        }

        public IEnumerable<Department> Departments { get; set; } = Enumerable.Empty<Department>();
        public IEnumerable<Screen> Screens { get; set; } = Enumerable.Empty<Screen>();
        public string? SelectedDepartment { get; set; }

        public async Task OnGetAsync(string? dept)
        {
            Departments = await _departments.GetAllAsync();
            SelectedDepartment = dept;

            if (!string.IsNullOrEmpty(dept))
            {
                var department = Departments.FirstOrDefault(d => d.Name == dept);
                if (department is not null)
                    Screens = await _screens.GetByDepartmentAsync(department.Id);
            }
        }
    }
}
