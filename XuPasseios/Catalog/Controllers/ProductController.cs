using AutoMapper;
using Catalog.Domain.Entities;
using Catalog.Infra.DbContexts;
using Catalog.Models;
using Catalog.Service;
using Microsoft.AspNetCore.Mvc;
using Catalog.Helpers;
using Catalog.ResourceParameters;

namespace Catalog.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, ILogger<ProductController> logger,
            IConfiguration configuration, IMapper mapper)
        {
            this._productService = productService;
            this._logger = logger;
            this._configuration = configuration;
            this._mapper = mapper;
        }

        [HttpGet("{productId}", Name = "Product")]
        [ProducesResponseType(typeof(ProductEntity), 200)]
        public IActionResult GetProduct(Guid productId)
        {
            var productEntity = this._productService.GetProduct(productId);
            return Ok(this._mapper.Map<ProductEntity, ProductDto>(productEntity));
        }

        [HttpGet(Name = "GetAll")]
        [ProducesResponseType(typeof(IEnumerable<ProductEntity>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] ProductResourceParameters resourceParameters)
        {
            this._logger.LogInformation("Getting all products");
            var productsEntity = await this._productService.
                                GetAllProducts(resourceParameters);
            var paginationMetadata = new
            {
                totalCount = productsEntity.TotalCount,
                pageSize = productsEntity.PageSize,
                currentPage = productsEntity.CurrentPage,
                totalPages = productsEntity.TotalPages,
                hasNext = productsEntity.HasNext,
                hasPrevious = productsEntity.HasPrevious
            };

            Response.Headers.Add("X-Pagination",
                System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

            var productsWithLinks = productsEntity.Select(product =>
            {
                var productDto = this._mapper.Map<ProductEntity, ProductDto>(product);
                var links = CreateLinksForProduct(product.ProductId, resourceParameters.Fields);
                var productWithLinks = new
                {
                    product = productDto,
                    links
                };
                return productWithLinks;
            });

            var links = CreateLinksForProducts(resourceParameters,
                productsEntity.HasNext, productsEntity.HasPrevious);

            var productsWithLinksResult = new
            {
                value = productsWithLinks,
                links
            };

            return Ok(productsWithLinksResult);
        }


        private IEnumerable<LinkDto> CreateLinksForProducts(
        ProductResourceParameters productResourceParameters,
        bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();
            // self 
            links.Add(
                new LinkDto(CreateProductsResourceUri(productResourceParameters,
                ResourceUriType.Current),
                "self",
                "GET"));
            if (hasNext)
            {
                links.Add(
                    new LinkDto(CreateProductsResourceUri(productResourceParameters,
                    ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }
            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateProductsResourceUri(productResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }
            return links;
        }

        private string? CreateProductsResourceUri(
        ProductResourceParameters productResourceParameters,
        ResourceUriType type)
        {

            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetProducts",
                    new
                    {
                        fields = productResourceParameters.Fields,
                        orderby = productResourceParameters.OrderBy,
                        mainCategory = productResourceParameters.MainCategory,
                        searchQuery = productResourceParameters.SearchQuery,
                        pageNumber = productResourceParameters.PageNumber - 1,
                        pageSize = productResourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return Url.Link("GetProducts",
                    new
                    {
                        fields = productResourceParameters.Fields,
                        orderby = productResourceParameters.OrderBy,
                        mainCategory = productResourceParameters.MainCategory,
                        searchQuery = productResourceParameters.SearchQuery,
                        pageNumber = productResourceParameters.PageNumber + 1,
                        pageSize = productResourceParameters.PageSize
                    });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetProducts",
                    new
                    {
                        fields = productResourceParameters.Fields,
                        orderby = productResourceParameters.OrderBy,
                        mainCategory = productResourceParameters.MainCategory,
                        searchQuery = productResourceParameters.SearchQuery,
                        pageNumber = productResourceParameters.PageNumber,
                        pageSize = productResourceParameters.PageSize
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForProduct(Guid productId, string? fields)
        {
            var links = new List<LinkDto>();
            // self 
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(Url.Link("GetProduct",
                    new { productId = productId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new LinkDto(Url.Link("GetProduct",
                    new { productId = productId, fields = fields }),
                    "self",
                    "GET"));
            }

            links.Add(
                    new(Url.Link("", new { productId }),
                    "create_product",
                    "POST"));

            return links;
        }
    }
}
