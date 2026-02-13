using MassTransit;

namespace BlazorApp01.Messaging.Facade;

public interface IPublishEndpointFacade
{
    /// <summary>
    /// Publish to RabbitMQ via MassTransit.
    /// Publishes an object as a message, using the message type specified. If the object cannot be cast
    /// to the specified message type, an exception will be thrown.
    /// </summary>
    /// <param name="message">The message object</param>
    /// <param name="messageType">The type of the message (use message.GetType() if desired)</param>
    /// <param name="cancellationToken"></param>
    Task Publish(object message, Type messageType, CancellationToken cancellationToken = default);
}

internal sealed class PublishEndpointFacade(IPublishEndpoint publishEndpoint) : IPublishEndpointFacade
{
    public async Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(message, messageType, cancellationToken);
    }
}
