using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class Attachment:
         BaseModel
    {
        //NOT USED JUST PUT FILES IN DIR
        public int Id { get; set; }
        [ForeignKey("Ticket")]       
        public int compId { get; set; }
        public Ticket? Ticket { get; set; }
        public string filePath { get; set; }
        public string attachmentType { get; set; }
    
    }
}
