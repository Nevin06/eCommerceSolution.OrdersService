using eCommerce.OrdersMicroService.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroService.BusinessLogicLayer.Policies;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/Users/{userID}");

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
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    return new UserDTO(UserID: Guid.Empty, PersonName: "Temporarily Unavailable", Email: "Temporarily Unavailable", Gender: "Temporarily Unavailable");
                }
            }

            UserDTO? userDto = await response.Content.ReadFromJsonAsync<UserDTO?>();

            if (userDto == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

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
