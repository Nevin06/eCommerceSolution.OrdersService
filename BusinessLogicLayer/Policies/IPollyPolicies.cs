using Polly;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;

public interface IPollyPolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount); //130
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak); //130
    IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout); //130
}
