using Catalog.Domain.Entities;
using Catalog.Infra.DbContexts;
using Catalog.Models;

namespace Catalog.Service
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly CatalogContext _catalogContext;

        public ProductService(ILogger<ProductService> logger, CatalogContext catalogContext)
        {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            this._catalogContext = catalogContext
                ?? throw new ArgumentNullException(nameof(catalogContext));
        }

        public IEnumerable<ProductEntity> GetAllProducts()
        {
            return this._catalogContext.Products.ToList();
        }

        public ProductEntity GetProduct(Guid productId)
        {
            try
            {
                var product = this._catalogContext.Products.FirstOrDefault(prd => prd.ProductId == productId);
                if (product == null)
                {
                    return new ProductEntity(new Guid(), string.Empty, string.Empty,
                        string.Empty, decimal.MinValue, decimal.MinValue,
                        decimal.MinValue, decimal.MinValue, decimal.MinValue, DateTime.MinValue);
                }
                return product;

            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error during the retrieval of the product in the database");
                throw;
            }
        }
    }
}
