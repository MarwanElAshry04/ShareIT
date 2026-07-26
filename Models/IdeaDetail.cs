using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class IdeaDetail
    {
        // --- plumbing: 1-to-1 link to the ticket ---
        [Key]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // --- fields, in slide-6 order ---
        // (Idea/Project Title + Description live on the Ticket)
        public IdeaScope Scope { get; set; }                    // "Scope" (Functional/Departmental/BU)
        public ImpactType ImpactType { get; set; }              // "Impact" (Improve/Save/Reduce)

        [ForeignKey("ResponsibleDepartment")]
        public int? ResponsibleDepartmentId { get; set; }       // "Responsible Dep"
        public Department? ResponsibleDepartment { get; set; }

        public string? ImpactDescription { get; set; }          // "Describe The Impact"
        public string? Sustainability { get; set; }             // "Sustainability"
    }
}
