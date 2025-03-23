namespace MyAspNetApp.Models
{
    public class Status
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}