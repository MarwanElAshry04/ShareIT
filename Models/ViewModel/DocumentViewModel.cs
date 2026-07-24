using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models.ViewModel
{
    public class DocumentViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Document name is required")]
        [Display(Name = "Document Name")]
        [StringLength(200, ErrorMessage = "Document name cannot exceed 200 characters")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        // Optional: nullable so an empty field binds to null (not a binding error).
        // Defaults to 0 in the controller when left blank.
        [Display(Name = "Display Order")]
        [Range(0, 100, ErrorMessage = "Display order must be between 0 and 100")]
        public int? DisplayOrder { get; set; }

        // File upload
        [Display(Name = "PDF Document")]
        [Required(ErrorMessage = "Please select a PDF file")]
        [DataType(DataType.Upload)]
        public IFormFile PdfFile { get; set; }

        // Logo upload (optional)
        [Display(Name = "Logo/Icon")]
        [DataType(DataType.Upload)]
        public IFormFile LogoFile { get; set; }

        // Existing files (for edit)
        public string? ExistingFilePath { get; set; }
        public string? ExistingLogoPath { get; set; }
    }

    public class HomeViewModel
    {
        public List<Document> Documents { get; set; } = new List<Document>();
    }

    public class DocumentListViewModel
    {
        public List<Document> Documents { get; set; } = new List<Document>();
        public int TotalCount { get; set; }
    }
}
