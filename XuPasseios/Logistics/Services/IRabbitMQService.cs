
namespace Logistics.Services
{
    public interface IRabbitMQService
    {
        Task ReceiveAsync();
        Task StopAsync(CancellationToken cancellationToken);
        void Dispose();
    }
}