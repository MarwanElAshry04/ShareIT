namespace ShareIT.Models.ViewModel
{
    public class TicketListViewModel
    {
        public List<Ticket> Tickets { get; set; } = new();
        public TicketStatisticsViewModel Statistics { get; set; } = new();

        // Search filters
        public string SearchTerm { get; set; }
        public string Status { get; set; }  // Changed from StatusId to Status (string)
        public string Kind { get; set; }    // Complaint / Suggestion / Feedback (empty = all)
        public int? TypeId { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public string? Validation { get; set; }
        public string ReporterType { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        // Lookup data
        public List<string> Statuses { get; set; } = new();  // Changed from List<TicketStatus>
        public List<TicketType> TicketTypes { get; set; } = new();
        public List<string> Priorities { get; set; } = new();
        public List<string> ReporterTypes { get; set; } = new();
        public List<string> AssignedDepartments { get; set; } = new();
    }

    public class TicketStatisticsViewModel
    {
        public int TotalTickets { get; set; }
        public int Initiate { get; set; }
        public int InProcess { get; set; }
        public int Closed { get; set; }
        public int ValidTickets { get; set; }
        public int InvalidTickets { get; set; }
        public int ComplaintCount { get; set; }
        public int SuggestionCount { get; set; }
        public int FeedbackCount { get; set; }
        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }
        public int AnonymousReports { get; set; }
        public int DisclosedReports { get; set; }
        public double AvgResolutionDays { get; set; }
        public List<ChartData> MonthlyTrends { get; set; } = new();
        public List<ChartData> TypeDistribution { get; set; } = new();
    }
    public class ChartData
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
}
