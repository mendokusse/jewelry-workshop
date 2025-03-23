namespace MyAspNetApp.Models
{
    public class Unit
    {
        public int UnitId { get; set; }
        public string UnitShortName { get; set; }
        public string UnitFullName { get; set; }

        public List<Material> Materials { get; set; } = new List<Material>();
    }
}