using Microsoft.AspNetCore.Http;
using ShareIT.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class Video : BaseModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Section is required")]
        [Display(Name = "Section/Category")]
        [StringLength(100, ErrorMessage = "Section name cannot exceed 100 characters")]
        public string Section { get; set; }

        [Display(Name = "Video Path")]
        public string? VideoPath { get; set; }  // Fixed naming: vedPath -> VideoPath

        [Required(ErrorMessage = "File type is required")]
        [Display(Name = "File Type")]
        public string? FileType { get; set; }  // Fixed naming: fType -> FileType

        [Display(Name = "File Name")]
        public string? FileName { get; set; }

        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        [Display(Name = "Duration")]
        public string? Duration { get; set; }

        // Not mapped to database - used only for file upload
        [NotMapped]
        [Display(Name = "Video File")]
        public IFormFile VideoFile { get; set; }
    }
}