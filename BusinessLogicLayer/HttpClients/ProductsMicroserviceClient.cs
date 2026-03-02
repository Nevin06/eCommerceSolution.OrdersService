using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid ProductID)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/productid/{ProductID}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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

            return productDto;
        }
        catch(BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full. Returning dummy data.");
            return new ProductDTO(ProductID: Guid.Empty, ProductName: "Temporarily Unavailable(bulkhead)", UnitPrice: 0.0, Category: "Temporarily Unavailable(bulkhead)", Quantity: 0);
        }
    }
}
