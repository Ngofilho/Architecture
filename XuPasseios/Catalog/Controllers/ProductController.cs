using AutoMapper;
using Catalog.Domain.Entities;
using Catalog.Infra.DbContexts;
using Catalog.Models;
using Catalog.Service;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{productId}", Name="Product")]
        [ProducesResponseType(typeof(ProductEntity), 200)]
        public IActionResult GetProduct(Guid productId)
        {
            var productEntity = this._productService.GetProduct(productId);
            return Ok(this._mapper.Map<ProductEntity, ProductDto>(productEntity));
        }

        [HttpGet(Name = "GetAll")]
        [ProducesResponseType(typeof(IEnumerable<ProductEntity>), 200)]
        public IActionResult GetAll()
        {
            var productsEntity = this._productService.GetAllProducts();
            return Ok(
                this._mapper.Map<IEnumerable<ProductDto>>(productsEntity));
        }
    }
}
