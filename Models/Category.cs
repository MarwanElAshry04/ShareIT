namespace ShareIT.Models
{
    public class Category: BaseModel
    {
        public int Id { get; set; }
        public string ComplainType { get; set; }
        public string? definition { get; set; }
        public string? ComplainType_ar { get; set; }
        public double? priority { get; set; }
        public int status { get; set; }
    
    }
}
