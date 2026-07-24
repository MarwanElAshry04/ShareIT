namespace ShareIT.Models
{
    public class SubTicketType
    {
        public int Id { get; set; }
        public int TicketTypeId { get; set; }
        public string SubComplainType { get; set; }
        public string definition { get; set; }
        public double priority { get; set; }
        public int status { get; set; }
    }
}
