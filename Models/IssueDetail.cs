using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class IssueDetail
    {
        // --- plumbing: 1-to-1 link to the ticket ---
        [Key]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // --- fields, in slide-5 order ---
        // (Issue Title + Description of the Problem live on the Ticket)
        public string? IssueType { get; set; }                  // "Issue Type"

        public IssueImpactArea ImpactArea { get; set; }         // "Impact Area"

        [ForeignKey("ResponsibleDepartment")]
        public int? ResponsibleDepartmentId { get; set; }       // "Responsible Department"
        public Department? ResponsibleDepartment { get; set; }

        public string? RootCause { get; set; }                  // "Root Cause (if known)"
        public string? ProposedSolution { get; set; }           // "Proposed Solution"
        public SolutionCategory? SolutionCategory { get; set; } // its (Operational/People/…) tag
    }
}
