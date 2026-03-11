namespace eCommerce.OrdersMicroService.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductNameUpdateConsumer
    {
        Task Consume();
        void Dispose(); //163
    }
}