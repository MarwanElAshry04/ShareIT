using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ShareIT.Models
{
    public class Ticket:
        BaseModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        // Which of the 6 types this ticket is. This enum is the single source of
        // truth for a ticket's type (the old Category lookup table was retired).
        public TicketType TicketType { get; set; } = TicketType.Issue;
        [ForeignKey("Reporter")]
        public int? ReporterId { get; set; }
        public Reporter? Reporter { get; set; }

        // The type-specific detail — only the one matching TicketType is filled in.
        public IssueDetail? IssueDetail { get; set; }
        public IdeaDetail? IdeaDetail { get; set; }
        public ProjectProposalDetail? ProjectProposalDetail { get; set; }
        public SafetyDetail? SafetyDetail { get; set; }
        public FeedbackDetail? FeedbackDetail { get; set; }
        public string? Status { get; set; }
        public string RefNo { get; set; }
        public string password { get; set; }
        public string Desc { get; set; }
        public string? ConcerningCompany { get; set; }
        public string? ConcerningDepartment { get; set; }
        public string? ConcerningOther { get; set; }
        public string? ConcerningPerson { get; set; }
        public string? finalRes { get; set; }
        public string langFlag { get; set; }
        public string? forwarding { get; set; }
        public string? toMails { get; set; }
        public string? ccMails { get; set; }
        public string? messageMail { get; set; }
        public string? validation { get; set; }
        public IEnumerable<Chats>? TicketChats { get; set; }
        public IEnumerable<Attachment>? TicketAttachments { get; set; }

        public DateTime? closureDate { get; set; }

    }
}
