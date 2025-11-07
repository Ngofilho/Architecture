using System.ComponentModel.DataAnnotations;

namespace Catalog.Domain.Entities;

public class ProductEntity
{
    [Key]
    public Guid ProductId { get; set; }
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; }
    [Required]
    [MaxLength (1000)]
    public string ProductDescription { get; set; }
    [Required]
    public string ProductWeight { get; set; }
    [Required]
    public decimal ProductHeight { get; set; }
    [Required]
    public decimal ProductWidth { get; set; }
    [Required]
    public decimal ProductDepth { get; set; }
    [Required]
    public decimal Sku { get; set; }
    [Required]
    public decimal Price { get; set; }
    public DateTime? DateInclusion { get; set; }
    public bool ProductActive { get; set; } = false;

    public ProductEntity(Guid productId,
                         string productName,
                         string productDescription,
                         string ProductWeight,
                         decimal productHeight,
                         decimal productWidth,
                         decimal productDepth,
                         decimal sku,
                         decimal price,
                         DateTime? dateInclusion,
                         bool productActive = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(productName);
        ArgumentException.ThrowIfNullOrEmpty(productDescription);
        ArgumentException.ThrowIfNullOrEmpty(ProductWeight);
        
        ProductId = productId;
        ProductName = productName;
        ProductDescription = productDescription;
        this.ProductWeight = ProductWeight;
        ProductHeight = productHeight;
        ProductWidth = productWidth;
        ProductDepth = productDepth;
        Sku = sku;
        Price = price;
        DateInclusion = dateInclusion;
        ProductActive = productActive;
    }
}
