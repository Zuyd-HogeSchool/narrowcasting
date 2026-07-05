using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;

namespace Narrowcasting.Pages.Public
{
    public class AboutModel : PageModel
    {
        private readonly IDepartmentService _departments;
        public AboutModel(IDepartmentService departments) => _departments = departments;

        public IEnumerable<Department> Departments { get; set; } = [];

        public async Task OnGetAsync() =>
            Departments = await _departments.GetAllAsync();
    }
}
