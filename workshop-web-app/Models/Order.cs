using System;

namespace workshop_web_app.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int ProductTypeId { get; set; }
        public int? CustomerUserId { get; set; }
        public int? ManagerUserId { get; set; }
        public int? JewelerUserId { get; set; }
        public int StatusId { get; set; }
        public string OrderComment { get; set; }
        public decimal? OrderPrice { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? OrderUpdateDate { get; set; }


        public ProductType ProductType { get; set; }
        public Status Status { get; set; }
        public User CustomerUser { get; set; }
        public User ManagerUser { get; set; }
        public User JewelerUser { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}