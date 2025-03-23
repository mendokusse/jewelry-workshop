namespace workshop_web_app.Models
{
    public class Material
    {
        public int MaterialId { get; set; }
        public int UnitId { get; set; }
        public decimal MaterialPrice { get; set; }
        public string MaterialName { get; set; }
        public float MaterialQuantity { get; set; }

        public Unit Unit { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}