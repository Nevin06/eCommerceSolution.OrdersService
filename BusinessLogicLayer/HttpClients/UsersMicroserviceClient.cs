using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _cache;
    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            //Key: user:123
            //Value: {UserID: 123, PersonName: "John Doe", Email: "john.doe@example.com", Gender: "Male"}
            string cacheKey = $"user:{userID}"; //139
            string? cachedUser = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedUser))
            {
                _logger.LogInformation("Cache hit for user with ID {userID}", userID);
                UserDTO? userFromCache = System.Text.Json.JsonSerializer.Deserialize<UserDTO>(cachedUser);
                return userFromCache;
            }
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/Users/{userID}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) //138
                {
                    UserDTO? userFromFallback = await response.Content.ReadFromJsonAsync<UserDTO?>();

                    if (userFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented.");
                    }

                    return userFromFallback;
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
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    return new UserDTO(UserID: Guid.Empty, PersonName: "Temporarily Unavailable", Email: "Temporarily Unavailable", Gender: "Temporarily Unavailable");
                }
            }

            UserDTO? userDto = await response.Content.ReadFromJsonAsync<UserDTO?>();

            if (userDto == null)
            {
                throw new ArgumentException("Invalid User ID");
            }


            string userJson = JsonSerializer.Serialize(userDto);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));
            await _cache.SetStringAsync($"user:{userID}", userJson, options);

            return userDto;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because Circuit breaker is in Open state. Returning dummy data.");
            return new UserDTO(UserID: Guid.Empty, PersonName: "Temporarily Unavailable", Email: "Temporarily Unavailable", Gender: "Temporarily Unavailable");
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout occured while fetching user data. Returning dummy data.");
            return new UserDTO(UserID: Guid.Empty, PersonName: "Temporarily Unavailable(timeout)", Email: "Temporarily Unavailable(timeout)", Gender: "Temporarily Unavailable(timeout)");
        }
    }
}
