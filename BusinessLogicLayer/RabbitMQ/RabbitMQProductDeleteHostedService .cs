using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.RabbitMQ
{
    //163
    public class RabbitMQProductDeleteHostedService : IHostedService
    {
        private readonly IRabbitMQProductDeleteConsumer _consumer;
        private readonly ILogger<RabbitMQProductDeleteHostedService> _logger;

        public RabbitMQProductDeleteHostedService(IRabbitMQProductDeleteConsumer consumer, ILogger<RabbitMQProductDeleteHostedService> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        //Called at application startup, before the application starts accepting requests.
        //This is where you can initialize resources, start background tasks, or perform any setup required for your service.
        //Handling events
        //In this method, you can set up event handlers for RabbitMQ message consumption.
        //For example, you might start consuming messages from a queue and define how to handle incoming messages.
        //The cancellationToken parameter can be used to gracefully shut down the service if needed.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQProductDeleteHostedService.StartAsync START");
            await _consumer.Consume(); //163
            //return Task.CompletedTask;
        }

        //Called when the application is shutting down, either gracefully or forcefully.
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQProductDeleteHostedService.StopAsync STOP");
            _consumer.Dispose(); //163
            return Task.CompletedTask;
        }
    }
}
