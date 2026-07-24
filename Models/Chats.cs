using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class Chats:BaseModel
    {
        public int Id { get; set; }
        [ForeignKey("Ticket")]
        public int compId { get; set; }
        public Ticket Ticket { get; set; }
        public string Comment { get; set; }
        public bool hasNewMessage { get; set; }

        public string flag { get; set; }
    }
}
