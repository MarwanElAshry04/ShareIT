namespace ShareIT.Models.ViewModel
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int? Department { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
