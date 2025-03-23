namespace MyAspNetApp.Models
{
    public class OrderDetail
    {
        public int DetailsListId { get; set; }
        public int OrderId { get; set; }
        public int MaterialId { get; set; }
        public float OrderMaterialWeight { get; set; }

        public Order Order { get; set; }
        public Material Material { get; set; }
    }
}