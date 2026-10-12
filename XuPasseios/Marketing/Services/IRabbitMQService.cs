
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Marketing.Services
{
    public interface IRabbitMQService
    {
        IChannel ReceiveAsync();
        Task StopAsync(CancellationToken cancellationToken);
        void Dispose();
    }
}