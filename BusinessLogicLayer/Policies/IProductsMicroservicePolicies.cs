using Polly;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;

public interface IProductsMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(); //125
    IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(); //128
}
