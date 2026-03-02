using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<PollyPolicies> _logger;
    public PollyPolicies(ILogger<PollyPolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).CircuitBreakerAsync(
        //handledEventsAllowedBeforeBreaking: 3, //Number of retries
        //durationOfBreak: TimeSpan.FromMinutes(2), //Delay between retries //121
        handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, //130
        durationOfBreak: durationOfBreak, //130
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

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).WaitAndRetryAsync(
        //retryCount: 5, //Number of retries
        retryCount: retryCount, //130
        //sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2), //Delay between retries //120
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), //Delay between retries //121
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            //TO DO: add logs
            _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
        });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        //AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        return policy;
    }
}
