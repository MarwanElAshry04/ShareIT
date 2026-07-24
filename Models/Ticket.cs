using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ShareIT.Models
{
    public class Ticket:
        BaseModel
    {
        public int Id { get; set; }
        // Complaint / Suggestion / Feedback. Existing rows default to Complaint.
        public TicketKind Kind { get; set; } = TicketKind.Complaint;
        [ForeignKey("TicketType")]
        public int? TicketTypeId { get; set; }
        public TicketType? TicketType { get; set; }
        [ForeignKey("Reporter")]
        public int? ReporterId { get; set; }
        public Reporter? Reporter { get; set; }
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
