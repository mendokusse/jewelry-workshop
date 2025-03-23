namespace workshop_web_app.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string UserName { get; set; }
        public string UserPasswordHash { get; set; }
        public string UserPhone { get; set; }
        public string UserEmail { get; set; }

        public Role Role { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}