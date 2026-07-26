using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class FeedbackDetail
    {
        // --- plumbing: 1-to-1 link to the ticket ---
        [Key]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // --- fields, in slide-9 order ---
        // (Feedback Title + Description live on the Ticket; Upload Photo/File = Attachments step)
        public FeedbackCategory Category { get; set; }               // "Category" (Supervisor Behavior/Workload/Communication/Facilities/Policy/HR Process)

        [ForeignKey("ResponsibleDepartment")]
        public int? ResponsibleDepartmentId { get; set; }            // "Responsible Department"
        public Department? ResponsibleDepartment { get; set; }

        public string? ImpactArea { get; set; }                      // "Impact Area"
        public PreferredResolution PreferredResolution { get; set; } // "Preferred Resolution Type" (Personal/Departmental/Organizational)
        public Confidentiality Confidentiality { get; set; }         // "Confidentiality Level" (Public/Private/Anonymous)
        public FeedbackActionType ActionType { get; set; }           // (Action Required / Feedback Only / Anonymous Share)
    }
}