using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models
{
    public class ProjectProposalDetail
    {
        // --- plumbing: 1-to-1 link to the ticket ---
        [Key]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // --- fields, in slide-7 order ---
        // (Project Title lives on the Ticket)
        public string? BusinessNeed { get; set; }        // "Business Need"
        public decimal? EstimatedCost { get; set; }      // "Estimated Cost" (money)
        public string? Scope { get; set; }               // "Scope"
        public string? ExpectedBenefits { get; set; }    // "Expected Benefits"
        public RoiReach RoiReach { get; set; }           // "Expected ROI" (BU/Multi-BU/Corporate)
        public string? KeyStakeholders { get; set; }     // "Key Stakeholders" (Financial/Operational/Safety/Compliance)
        public DateTime? TimelineStart { get; set; }     // "Timeline" start date
        public DateTime? TimelineEnd { get; set; }       // "Timeline" end date
    }
}
