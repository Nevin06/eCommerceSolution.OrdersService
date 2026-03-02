using Polly;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;

public interface IUsersMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(); //119
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(); //123
    IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(); //126
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(); //129
}
