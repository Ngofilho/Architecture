using System.ComponentModel.DataAnnotations;

namespace Catalog.Models
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductWeight { get; set; }
        public decimal ProductHeight { get; set; }
        public decimal ProductWidth { get; set; }
        public decimal ProductDepth { get; set; }
        public decimal Sku { get; set; }
        public decimal Price { get; set; }
        public DateTime? DateInclusion { get; set; }
        public bool ProductActive { get; set; } = false;
    }
}
