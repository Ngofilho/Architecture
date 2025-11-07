using Catalog.Domain.Entities;

namespace Catalog.Service
{
    public interface IProductService
    {
        ProductEntity GetProduct(Guid productId);
        IEnumerable<ProductEntity> GetAllProducts();
    }
}