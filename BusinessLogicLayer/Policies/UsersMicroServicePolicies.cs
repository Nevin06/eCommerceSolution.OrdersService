using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;

public class UsersMicroServicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroServicePolicies> _logger;
    private readonly IPollyPolicies _pollyPolicies;
    public UsersMicroServicePolicies(ILogger<UsersMicroServicePolicies> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3, //Number of retries
        durationOfBreak: TimeSpan.FromMinutes(2), //Delay between retries //121
        onBreak: (outcome, timespan) =>
        {
            //TO DO: add logs
            _logger.LogInformation($"Circuit Breaker opened for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent " +
                $"requests will be blocked.");
        },
        onReset: () =>
        {
            //TO DO: add logs
            _logger.LogInformation($"Circuit Breaker closed. The subsequent requests will be allowed.");
        });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).WaitAndRetryAsync(
        retryCount: 1, //Number of retries
        //sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2), //Delay between retries //120
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), //Delay between retries //121
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            //TO DO: add logs
            _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
        });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        //var retryPolicy = GetRetryPolicy();
        //var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        //var timeoutPolicy = GetTimeoutPolicy();

        var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5)); //130

        AsyncPolicyWrap<HttpResponseMessage> policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return policy;
    }
}
