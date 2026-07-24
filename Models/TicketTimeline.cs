namespace ShareIT.Models
{
    public class TicketTimeline : BaseModel
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public DateTime EventDate { get; set; }
    }
}
