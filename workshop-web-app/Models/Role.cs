namespace workshop_web_app.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}