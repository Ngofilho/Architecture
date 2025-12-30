using Catalog.Domain.Entities;
using Catalog.Helpers;
using Catalog.ResourceParameters;

namespace Catalog.Service
{
    public interface IProductService
    {
        ProductEntity GetProduct(Guid productId);
        Task<PagedList<ProductEntity>> GetAllProducts(ProductResourceParameters resourceParameters);
    }
}