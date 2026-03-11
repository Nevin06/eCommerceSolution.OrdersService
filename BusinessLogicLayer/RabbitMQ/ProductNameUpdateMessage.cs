namespace eCommerce.OrdersMicroService.BusinessLogicLayer.RabbitMQ;

public record ProductNameUpdateMessage(Guid ProductID, string? NewName);
