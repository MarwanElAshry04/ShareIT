namespace ShareIT.Models
{
    /// <summary>
    /// The nature of a submitted ticket. ShareIT covers all three;
    /// existing records default to <see cref="Complaint"/>.
    /// </summary>
    public enum TicketKind
    {
        Complaint = 0,
        Suggestion = 1,
        Feedback = 2
    }
}
