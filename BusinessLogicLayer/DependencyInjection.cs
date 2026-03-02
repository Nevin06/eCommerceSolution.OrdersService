using eCommerce.OrdersMicroService.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroService.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroService.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroService.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>(); //90
        services.AddAutoMapper(cfg => { }, typeof(OrderAddRequesttoOrderMappingProfile).Assembly); //91
        services.AddScoped<IOrdersService, OrdersService>(); //93
        services.AddStackExchangeRedisCache(options =>
        {
            //options.Configuration = "localhost:6379"; // Adjust as needed
            options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
            options.InstanceName = "OrdersMicroServiceCache";
        }); //135
        return services;
    }
}
