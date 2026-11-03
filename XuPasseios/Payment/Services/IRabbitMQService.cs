
using Payment.DTOs;

namespace Payment.Services
{
    public interface IRabbitMQService
    {
        Task Send(PaymentDTO mensagem, CancellationToken cancellation);
        Task<PaymentDTO> ReceiveAsync(CancellationToken cancellation);
        Task StopAsync(CancellationToken cancellationToken);
        void Dispose();
    }
}