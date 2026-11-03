using Microsoft.Extensions.Caching.Memory;
using ProductCatalog.DTO;
using ProductCatalog.Model;

namespace ProductCatalog.Service
{
    public class ProductService : IProductService
    {
        private readonly IMemoryCache _cache;
        private readonly List<ProductModel> _products = new()
        {
            new ProductModel { Id = 1, Name = "iPhone 15", Category = "Electronics", Price = 65000, Stock = 10, IsActive = true },
            new ProductModel { Id = 2, Name = "Samsung S24", Category = "Electronics", Price = 75000, Stock = 15, IsActive = true },
            new ProductModel { Id = 3, Name = "Dell Laptop", Category = "Computers", Price = 55000, Stock = 8, IsActive = true },
            new ProductModel { Id = 4, Name = "HP Laptop", Category = "Computers", Price = 45000, Stock = 12, IsActive = true },
            new ProductModel { Id = 5, Name = "Nike Shoes", Category = "Fashion", Price = 3000, Stock = 20, IsActive = true },
            new ProductModel { Id = 6, Name = "Adidas Shoes", Category = "Fashion", Price = 4000, Stock = 18, IsActive = true },
            new ProductModel { Id = 7, Name = "Sony Headphones", Category = "Electronics", Price = 5000, Stock = 25, IsActive = true },
            new ProductModel { Id = 8, Name = "MacBook Air", Category = "Computers", Price = 90000, Stock = 5, IsActive = true },
            new ProductModel { Id = 9, Name = "Watch", Category = "Fashion", Price = 2500, Stock = 30, IsActive = true },
            new ProductModel { Id = 10, Name = "Keyboard", Category = "Electronics", Price = 1500, Stock = 40, IsActive = true }
        };
        
        public ProductService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public PagedResult<ProductModel> GetProducts(ProductQueryDTO query)
        {
            // 1. Generate cache key
            string cacheKey = $"Products_{query.Search}_{query.MinPrice}_{query.MaxPrice}_{query.Category}_{query.SortBy}_{query.SortOrder}_{query.Page}_{query.PageSize}";

            // 2. Check cache
            if (_cache.TryGetValue(cacheKey, out PagedResult<ProductModel>? cachedResult))
            {
                return cachedResult!;
            }

            // 3. Start with all products
            IEnumerable<ProductModel> products = _products;

            // 4. Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                products = products.Where(p => p.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            }

            if(!string.IsNullOrWhiteSpace(query.MinPrice.ToString()))
            {
                products = products.Where(p => p.Price > query.MinPrice );
            }

            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                products = products.Where(p => p.Price < query.MaxPrice);
            }

            if(!string.IsNullOrWhiteSpace(query.Category))
            {
                products = products.Where(p => p.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            // 7. Sorting
            bool descending = query.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            products = (query.SortBy?.ToLowerInvariant(), descending) switch
            {
                ("price", false) => products.OrderBy(p => p.Price),
                ("price", true) => products.OrderByDescending(p => p.Price),

                ("category", false) => products.OrderBy(p => p.Category),
                ("category", true) => products.OrderByDescending(p => p.Category),

                ("name", false) => products.OrderBy(p => p.Name),
                ("name", true) => products.OrderByDescending(p => p.Name),

                _ => products.OrderBy(p => p.Id)
            };


            // 8. Get total records before pagination
            int totalRecords = products.Count();


            // 9. Pagination
            List<ProductModel> pagedProducts = products
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();


            // 10. Create response
            var result = new PagedResult<ProductModel>
            {
                Items = pagedProducts,
                TotalRecords = totalRecords,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / query.PageSize)
            };


            // 11. Store result in cache 
            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5));


            return result;

        }


    }
}
