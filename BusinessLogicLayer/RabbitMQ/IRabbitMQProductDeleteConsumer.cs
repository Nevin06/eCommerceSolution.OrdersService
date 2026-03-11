namespace eCommerce.OrdersMicroService.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeleteConsumer
{
    Task Consume();
    void Dispose(); //164
}