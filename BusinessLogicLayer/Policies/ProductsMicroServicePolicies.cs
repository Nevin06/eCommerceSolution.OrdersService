using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using System.Text;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;

public class ProductsMicroServicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroServicePolicies> _logger;
    public ProductsMicroServicePolicies(ILogger<ProductsMicroServicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 2 //Allows up to 2 concurrent requests
            , maxQueuingActions: 40 //Allows up to 40 additional requests to be queued when the maximum parallelization limit is reached. If the queue is full, additional requests will be rejected immediately.
            , onBulkheadRejectedAsync: async context =>
            {
                //TO DO: add logs
                _logger.LogWarning("Bulkhead Rejection: Too many concurrent requests to Products Microservice. Request rejected.");
                //await Task.CompletedTask;
                throw new BulkheadRejectedException("Bulkhead queue is full.");
            }
            );
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).FallbackAsync(
        async(context) =>
        {
            _logger.LogWarning("Fallback triggred: Products Microservice is unavailable. Returning dummy data.");
            ProductDTO productDTO = new ProductDTO(ProductID: Guid.Empty, ProductName: "Temporarily Unavailable(fallback)", UnitPrice: 0.0
                , Category: "Temporarily Unavailable(fallback)", Quantity: 0);
            HttpResponseMessage fallbackResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                //Content = new StringContent("[{\"ProductID\": \"00000000-0000-0000-0000-000000000000\", \"ProductName\": \"Temporarily Unavailable\", \"Price\": 0.0}]")
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(productDTO), Encoding.UTF8, "application/json")
            };

            //return await Task.FromResult(fallbackResponse);
            return fallbackResponse;
        });

        return policy;
    }
}
