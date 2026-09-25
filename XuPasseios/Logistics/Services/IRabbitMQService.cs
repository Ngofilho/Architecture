
using RabbitMQ.Client;

namespace Logistics.Services
{
    public interface IRabbitMQService
    {
        IChannel Receive();
        void PublishMessage(Common.MarketingMessage message);
        Task StopAsync(CancellationToken cancellationToken);
        void Dispose();
    }
}