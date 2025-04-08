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

        public string UserFormattedPhone
            {
                get
                {
                    if (string.IsNullOrEmpty(UserPhone) || UserPhone.Length != 10)
                    {
                        return UserPhone;
                    }

                    return $"+7 ({UserPhone.Substring(0, 3)}) {UserPhone.Substring(3, 3)}-{UserPhone.Substring(6, 2)}-{UserPhone.Substring(8, 2)}";
                }
            }

        public Role Role { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}