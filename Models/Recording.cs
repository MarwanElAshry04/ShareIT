using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class Recording
    {
        public int Id { get; set; }
        [ForeignKey("Ticket")]
        public int compId { get; set; }
        public Ticket Ticket { get; set; }
        public string blobDESC { get; set; }
    }
}
