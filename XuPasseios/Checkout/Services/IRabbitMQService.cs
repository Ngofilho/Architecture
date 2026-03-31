
using Common;
using static Checkout.Services.RabbitMQService;

namespace Checkout.Services
{
    public interface IRabbitMQService
    {
        Task ReceiveAsync(OrderMessage mensagem);
        Task SendAsync(OrderMessage mensagem);
    }
}