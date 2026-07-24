namespace ShareIT.Models.ViewModel
{
    public class ChatViewModel
    {
        public int TicketId { get; set; }
        public string RefNo { get; set; }
        public string Message { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new();
        public bool IsReadOnly { get; set; }
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string Sender { get; set; }
        public string SenderType { get; set; } // "user" or "compliance"
        public DateTime CreatedOn { get; set; }
        public string FormattedTime => CreatedOn.ToString("HH:mm");
        public string FormattedDate => CreatedOn.ToString("dd MMM yyyy");
        public bool IsUserMessage => SenderType == "user";
    }
}
