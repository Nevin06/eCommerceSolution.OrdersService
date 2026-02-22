using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid ProductID)
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
}
