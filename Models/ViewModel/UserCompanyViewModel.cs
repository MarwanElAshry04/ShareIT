using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models.ViewModel
{
    public class UserCompanyViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int SelectedDepartment { get; set; }
        public List<Department> AvailableCompanies { get; set; } = new();
    }
}
