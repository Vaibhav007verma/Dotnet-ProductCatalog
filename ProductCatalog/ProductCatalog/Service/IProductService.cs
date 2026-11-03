using ProductCatalog.Model;
using ProductCatalog.DTO;

namespace ProductCatalog.Service
{
    public interface IProductService
    {
        PagedResult<ProductModel> GetProducts(ProductQueryDTO query);
    }
}
