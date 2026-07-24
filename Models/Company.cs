namespace ShareIT.Models
{
    public class Company:BaseModel
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public int status { get; set; }
    }
}
