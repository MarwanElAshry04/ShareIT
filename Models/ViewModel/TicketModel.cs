using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models.ViewModel
{
    public class NewTicketViewModel
    {
        // Step 1: Reporter Data
        [Display(Name = "Are you an Elsewedy Electric Employee?")]
        public string? IsEmployee { get; set; }

        // Employee fields
        [Display(Name = "Universal Code")]
        public string? EmpNum { get; set; }

        [Display(Name = "Company")]
        public int? EmpCompanyId { get; set; }

        [Display(Name = "Department")]
        public int? EmpDepartmentId { get; set; }

        [Display(Name = "Name")]
        public string? EmpName { get; set; }

        [Display(Name = "Mobile")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Enter valid phone number (11 digits)")]
        public string? EmpMobile { get; set; }

        [Display(Name = "First Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string? EmpEmail1 { get; set; }

        [Display(Name = "Second Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string? EmpEmail2 { get; set; }

        // Non-employee fields
        [Display(Name = "Relationship with ELSEWEDY")]
        public int? NonEmpRelationId { get; set; }

        [Display(Name = "Organization")]
        public string? Organization { get; set; }

        [Display(Name = "Name")]
        public string? NonEmpName { get; set; }

        [Display(Name = "Mobile")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Enter valid phone number (11 digits)")]
        public string?  NonEmpMobile { get; set; }

        [Display(Name = "First Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string? NonEmpEmail1 { get; set; }

        [Display(Name = "Second Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string? NonEmpEmail2 { get; set; }

        public string? ReporterType { get; set; } = "Disclose"; // "Disclose" or "Anonymous"

        // Step 2: Ticket Data
        [Display(Name = "What would you like to submit?")]
        public TicketType TicketType { get; set; } = TicketType.Issue;

        [Display(Name = "Title")]
        public string? Title { get; set; }

        // Step 3: type-specific detail. We reuse the detail model classes as
        // "input holders" — only the one matching TicketType gets saved.
        public IssueDetail Issue { get; set; } = new();
        public IdeaDetail Idea { get; set; } = new();
        public ProjectProposalDetail ProjectProposal { get; set; } = new();
        public SafetyDetail Safety { get; set; } = new();
        public FeedbackDetail Feedback { get; set; } = new();

        // Not [Required] here — the controller validates it at step 3 only.
        // (A [Required]/non-nullable field would make the whole model invalid on
        //  every earlier step and block the wizard from advancing.)
        [Display(Name = "Description")]
        [StringLength(5000, ErrorMessage = "Description is too long")]
        public string? Description { get; set; }

        [Display(Name = "Company")]
        public int? ConcerningCompanyId { get; set; }

        [Display(Name = "Department")]
        public int? ConcerningDepartmentId { get; set; }

        [Display(Name = "Person")]
        public string? ConcerningPerson { get; set; }

        [Display(Name = "Other Suppliers / Related Parties")]
        public string? ConcerningOtherSuppliers { get; set; }

        // Step 3: Attachments
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        // Step 4: Review
        // Validated by the controller at step 5 (not [Required] here — see Description note).
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
        public string? UploadedFilePaths { get; set; }
        // In NewTicketViewModel.cs

        // Keep the existing UploadedFiles list for display
        public List<string> UploadedFiles
        {
            get => string.IsNullOrEmpty(UploadedFilePaths)
                ? new List<string>()
                : UploadedFilePaths.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        // Lookup data for dropdowns
        public List<TempFileData> TempFiles { get; set; } = new List<TempFileData>();

        public List<Company>? Companies { get; set; }
        public List<Department>? Departments { get; set; }
        public List<LookupItem>? Relations { get; set; }

        // Current step tracking
        public int CurrentStep { get; set; } = 1; // 1=Reporter, 2=Ticket, 3=Attachments, 4=Review
        public string RefNo { get; set; }
    }

    public class LookupItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class UploadedFileInfo
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }
    public class TempFileData
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }
}