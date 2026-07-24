namespace ShareIT.Models.ViewModel
{
    public class EmailDto
    {
        public string OutlookItemId { get; set; }
        public string Subject { get; set; }
        public SenderDto Sender { get; set; }
        public List<string> ToRecipients { get; set; }
        public List<string> CcRecipients { get; set; }
        public string BodyPreview { get; set; }
        public string FullBody { get; set; }
        public string ConversationId { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
    }

    public class SenderDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }


    public class AttachmentDto
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string AttachmentType { get; set; }
        public string Id { get; set; }
    }

    public class TempEmailData
    {
        public int Id { get; set; }
        public EmailDto EmailData { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProcessed { get; set; }
    }
}
