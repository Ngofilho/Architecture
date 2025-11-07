
namespace Checkout.Services
{
    public interface IRabbitMQService
    {
        Task ReceiveAsync(RabbitMQService.Mensagem mensagem);
    }
}