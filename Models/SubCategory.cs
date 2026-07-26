namespace ShareIT.Models
{
    public class SubCategory
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string SubComplainType { get; set; }
        public string definition { get; set; }
        public double priority { get; set; }
        public int status { get; set; }
    }
}
