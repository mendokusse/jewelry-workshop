namespace workshop_web_app.Models
{
    public class ProductType
    {
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}