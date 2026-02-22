using eCommerce.OrdersMicroService.DataAccessLayer.Repositories;
using eCommerce.OrdersMicroService.DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroService.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;
        string connectionString = connectionStringTemplate.Replace("$MONGODB_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
            .Replace("$MONGODB_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));

        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        services.AddScoped<IMongoDatabase>(provider =>
        {
            IMongoClient client = provider.GetRequiredService<IMongoClient>();
            //return client.GetDatabase("OrdersDatabase"); // returns MongoDB database object // DB created automatically at runtime
            return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE")); // 104
        });
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        return services;
    }
}
