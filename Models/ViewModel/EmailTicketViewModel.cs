using System.ComponentModel.DataAnnotations;
using ShareIT.Models;

namespace ShareIT.Models.ViewModel
{
    public class EmailTicketViewModel
    {
        public int TempEmailId { get; set; }

        [Display(Name = "Ticket Type")]
        public TicketType TicketType { get; set; } = TicketType.Issue;

        [Display(Name = "Concerning Company")]
        public string ConcerningCompany { get; set; }

        [Display(Name = "Concerning Department")]
        public string ConcerningDepartment { get; set; }

        [Display(Name = "Concerning Person")]
        public string ConcerningPerson { get; set; }

        [Display(Name = "Other Suppliers")]
        public string ConcerningOther { get; set; }

        [Display(Name = "Description")]
        public string Desc { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Language")]
        public string LangFlag { get; set; } = "E";

        // Reporter Information (Automatically filled from email)
        public string ReporterName { get; set; }
        public string ReporterEmail { get; set; }
        public string? ReporterMobile { get; set; }
        public int? DepartmentId { get; set; }
        public string? EmpCode { get; set; }

        // Email Information (Read-only)
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailSender { get; set; }
        public DateTime EmailDate { get; set; }
        public List<EmailAttachmentViewModel> EmailAttachments { get; set; } = new();
    }

    public class EmailAttachmentViewModel
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string ContentBytes { get; set; } // Base64
        public string TempPath { get; set; }
    }
}
