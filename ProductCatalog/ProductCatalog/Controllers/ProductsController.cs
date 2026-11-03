using Microsoft.AspNetCore.Mvc;
using ProductCatalog.DTO;
using ProductCatalog.Service;

namespace ProductCatalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] ProductQueryDTO query)
        {
            if (query.Page < 1 || query.PageSize < 1 || query.PageSize > 100)
            {
                return BadRequest("Invalid pagination values.");
            }

            if (query.MinPrice < 0 || query.MaxPrice < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice > query.MaxPrice)
            {
                return BadRequest("MinPrice cannot exceed MaxPrice.");
            }

            if (!string.IsNullOrWhiteSpace(query.SortOrder) && query.SortOrder.ToLowerInvariant() is not ("asc" or "desc"))
            {
                return BadRequest("SortOrder must be asc or desc.");
            }

            var result = _productService.GetProducts(query);

            return Ok(result);
        }


    }
}
