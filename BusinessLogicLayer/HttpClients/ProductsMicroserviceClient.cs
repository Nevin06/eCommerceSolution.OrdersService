using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _cache;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid ProductID)
    {
        try
        {
            //Key: product:123
            //Value: {ProductID: 123, ProductName: "Product A", UnitPrice: 10.0, Category: "Category A", Quantity: 100}
            string cacheKey = $"product:{ProductID}"; //136
            string ? cachedProduct = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProduct))
            {
                _logger.LogInformation("Cache hit for product with ID {ProductID}", ProductID);
                ProductDTO? productFromCache = System.Text.Json.JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/productid/{ProductID}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) //138
                {
                    ProductDTO? productFromFallback = await response.Content.ReadFromJsonAsync<ProductDTO?>();

                    if (productFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented.");
                    }

                    return productFromFallback;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad Request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                }
            }

            ProductDTO? productDto = await response.Content.ReadFromJsonAsync<ProductDTO?>();

            if (productDto == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }

            //Key: product:123
            //Value: {ProductID: 123, ProductName: "Product A", UnitPrice: 10.0, Category: "Category A", Quantity: 100}
            string productJson = JsonSerializer.Serialize(productDto);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));
            await _cache.SetStringAsync($"product:{ProductID}",productJson, options);

            return productDto;
        }
        catch(BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full. Returning dummy data.");
            return new ProductDTO(ProductID: Guid.Empty, ProductName: "Temporarily Unavailable(bulkhead)", UnitPrice: 0.0, Category: "Temporarily Unavailable(bulkhead)", Quantity: 0);
        }
    }
}
