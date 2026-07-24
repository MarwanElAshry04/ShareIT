using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ShareIT.Models.ViewModel
{

    public class TicketSearchModel
    {
        [Required(ErrorMessage = "Ticket reference is required")]
        [Display(Name = "Ticket Reference")]
        public string TicketRef { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? Message { get; set; }
        public string? MessageType { get; set; } // "success", "danger", "warning", "info"
        public bool ShowForgetPasswordLink { get; set; }
        public bool ShowTicketDetails { get; set; }

        // Add this property
        public TicketDetailsViewModel? TicketDetails { get; set; }
    }

    // Add this new ViewModel
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; }
        public List<TimelineEvent> Timeline { get; set; }
        public List<Chats> RecentMessages { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    public class TimelineEvent
    {
        public int Id { get; set; }          // ← from DB row
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string TimeAgo => GetTimeAgo();

        private string GetTimeAgo()
        {
            var timeSpan = DateTime.Now - Date;
            if (timeSpan <= TimeSpan.FromSeconds(60)) return "just now";
            if (timeSpan <= TimeSpan.FromMinutes(60)) return $"{timeSpan.Minutes} minutes ago";
            if (timeSpan <= TimeSpan.FromHours(24)) return $"{timeSpan.Hours} hours ago";
            if (timeSpan <= TimeSpan.FromDays(30)) return $"{timeSpan.Days} days ago";
            if (timeSpan <= TimeSpan.FromDays(365)) return $"{timeSpan.Days / 30} months ago";
            return $"{timeSpan.Days / 365} years ago";
        }
    }

}