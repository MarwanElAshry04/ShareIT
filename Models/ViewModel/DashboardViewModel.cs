namespace ShareIT.Models.ViewModel
{
    public class DashboardViewModel
    {
        public List<ChartLabelValue> SubmissionsByCompany { get; set; } = new();
        public List<string> TypeLabels { get; set; } = new();
        public List<StackedSeries> SubmissionsByTypeAndStatus { get; set; } = new();
        public List<ChartLabelValue> TopTicketTypes { get; set; } = new();
        public List<ChartLabelValue> StatusBreakdown { get; set; } = new();
        public List<ChartLabelValue> KindBreakdown { get; set; } = new();
        public int TotalTickets { get; set; }
    }

    public class ChartLabelValue
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class StackedSeries
    {
        public string StatusLabel { get; set; }
        public List<int> Values { get; set; } = new();
    }
}