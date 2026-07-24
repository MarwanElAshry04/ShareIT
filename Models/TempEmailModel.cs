namespace ShareIT.Models
{
    public class TempEmailModel
    {
        public int Id { get; set; }
        public string OutlookItemId { get; set; }
        public string Subject { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string ToRecipients { get; set; } // JSON
        public string CcRecipients { get; set; } // JSON
        public string BodyPreview { get; set; }
        public string FullBody { get; set; }
        public string ConversationId { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Attachments { get; set; } // JSON
        public bool IsProcessed { get; set; }
    }
}
