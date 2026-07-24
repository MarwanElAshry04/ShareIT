namespace ShareIT.Models.ViewModel
{
    public class UserCompaniesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string PrimaryDeprtment { get; set; }
        public List<string> AvailableCompanies { get; set; }
    }
}
