namespace workshop_web_app.Models
{
    public class Catalog
    {
        public int CatalogPhotoId { get; set; }
        public int? ProductTypeId { get; set; }
        public string ProductName { get; set; }
        public string ProductImgPath { get; set; }
        public decimal ProductPrice { get; set; }
        
        public ProductType ProductType { get; set; }
    }
}