using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.OrdersMicroService.BusinessLogicLayer.RabbitMQ
{
    //160
    public class RabbitMQProductDeleteConsumer : IDisposable, IRabbitMQProductDeleteConsumer
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<RabbitMQProductDeleteConsumer> _logger;
        private readonly IDistributedCache _cache;

        public RabbitMQProductDeleteConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeleteConsumer> logger, IDistributedCache cache)
        {
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        public async Task Consume()
        {
            try
            {
                //string routingKey = "product.#"; //166
                //167
                var headers = new Dictionary<string, object>
                {
                    {  "x-match", "all"  }, // for header exchange, specify that all headers must match // any for any header match 
                    { "eventType", "product.delete" },
                    {"RowCount", 1 }
                    //{ "timestamp", DateTime.UtcNow }
                };

                //string queueName = _configuration["RABBITMQ_PRODUCTS_NAME_UPDATE_QUEUE"]!;
                string queueName = "orders.product.delete.queue";
                //_logger.LogInformation("RabbitMQPublisher.Publish START, routingKey={routingKey}", routingKey);
                string hostName = _configuration["RABBITMQ_HOST"]!;
                string userName = _configuration["RABBITMQ_USERNAME"]!;
                string password = _configuration["RABBITMQ_PASSWORD"]!;
                string port = _configuration["RABBITMQ_PORT"]!;

                ConnectionFactory connectionFactory = new ConnectionFactory
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password,
                    Port = Convert.ToInt32(port)
                }; //158
                _logger.LogInformation("RabbitMQ config host={hostName} port={port} user={userName}",
    hostName, port, userName);
                _connection = await connectionFactory.CreateConnectionAsync();

                _channel = await _connection.CreateChannelAsync(); //158
                _logger.LogInformation("Connection + channel created");

                //string exchangeName = "products.exchange"; // specify exchange name if needed
                string exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;

                _logger.LogInformation("Declaring exchange {exchange}", exchangeName);
                // If the exchange already exists, this will do nothing. If it doesn't exist, it will be created.
                await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true); //159 //165 //167
                _logger.LogInformation("Consumed message to {exchange}", exchangeName);

                //161
                await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //arguments => x-message-ttl time to live | x-max-length max queue length | x-expires queue expiration time

                // Bind the queue to the exchange with the routing key
                await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers); //165 //166 //167

                //162
                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    try
                    {
                        byte[] body = eventArgs.Body.ToArray();
                        string message = System.Text.Encoding.UTF8.GetString(body);
                        _logger.LogInformation("Received message: {message}", message);
                        // Process the message here (e.g., deleted product from the database)
                        if (message != null)
                        {
                            ProductDeleteMessage? deleteMessage = System.Text.Json.JsonSerializer.Deserialize<ProductDeleteMessage>(message);

                            //168
                            await _cache.RemoveAsync($"product:{deleteMessage?.ProductID}");

                            _logger.LogInformation("Deserialized message: {deleteMessage}", deleteMessage);
                            _logger.LogInformation("Deleted product with ProductID: {ProductID} and ProductName: {ProductName}", deleteMessage?.ProductID, deleteMessage?.ProductName);
                        }
                        // Acknowledge the message after processing
                        await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        // Optionally, you can reject the message and requeue it
                        await _channel.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: true);
                    }
                };
                await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing to RabbitMQ");
                throw; // or decide how to handle
            }
        }
    }
}
