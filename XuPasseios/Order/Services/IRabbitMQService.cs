
using Order.DTOs;

namespace Order.Services
{
    public interface IRabbitMQService
    {
        Task<OrderDTO> ReceiveAsync();
        Task SendAsync(CancellationToken cancellationToken, OrderDTO mensagem);
        Task StopAsync(CancellationToken cancellationToken);
        void Dispose();
    }
}