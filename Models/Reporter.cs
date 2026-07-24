using System.ComponentModel.DataAnnotations.Schema;

namespace ShareIT.Models
{
    public class Reporter: BaseModel
    {
        public int Id { get; set; }
        public string? reporterType { get; set; }
    
        public Ticket? Ticket { get; set; }
        public Department? Department { get; set; }

        [ForeignKey("Department")]
        public int? DepratmentId { get; set; }
        public string? empCode { get; set; }
        public Relation Relation { get; set; }
        public int? RelationId { get; set; }
        public string? name { get; set; }
        public string? email1 { get; set; }
        public string? email2 { get; set; }
        public string? mobile { get; set; }
    }
}
