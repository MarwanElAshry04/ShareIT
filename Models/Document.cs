using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models
{
    public class Document:BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Document name is required")]
        [StringLength(200, ErrorMessage = "Document name cannot exceed 200 characters")]
        [Display(Name = "Document Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "File path is required")]
        [Display(Name = "File Path")]
        public string FilePath { get; set; }

        // Optional: no logo is uploaded for most documents, so this must allow NULL.
        [Display(Name = "Logo/Icon")]
        public string? Logo { get; set; }

        // Optional basic fields
        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

    
    }
}
