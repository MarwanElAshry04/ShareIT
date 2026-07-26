using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class SafetyDetail
    {
        // --- plumbing: 1-to-1 link to the ticket ---
        [Key]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // --- fields, in slide-8 order ---
        // (Safety Concern Title + Description of Concern live on the Ticket;
        //  the "Upload Photo/File" is handled by the Attachments step)
        public string? PotentialImpact { get; set; }        // "Potential Impact" (Injury/Downtime/Damage/Environmental)
        public RiskLevel RiskLevel { get; set; }            // "Risk Level" (Low/Medium/High/Critical)
        public SafetyHazard HazardCategory { get; set; }    // "Category" (Unsafe Act/Unsafe Condition/Environmental/Health)
        public string? SuggestedAction { get; set; }        // "Suggested Preventive or Corrective Action"

        [ForeignKey("ResponsibleDepartment")]
        public int? ResponsibleDepartmentId { get; set; }   // "Responsible Department"
        public Department? ResponsibleDepartment { get; set; }
    }
}
