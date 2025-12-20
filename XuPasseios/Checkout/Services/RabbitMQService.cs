using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Common;

namespace Checkout.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private ILogger<RabbitMQService> _logger;
        private IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueNameDestination;
        private readonly string _queueName;
        private readonly ConnectionFactory _factory;

        public class Mensagem
        {
            public int OrderId { get; set; }
            public DateTime Date { get; set; }
        }

        public RabbitMQService(ILogger<RabbitMQService> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;

            this._factory = new ConnectionFactory();

            this._factory.UserName = configuration.GetSection("rabbit:user").Value!;
            this._factory.Password = configuration.GetSection("rabbit:password").Value!;
            this._factory.VirtualHost = configuration.GetSection("rabbit:vhost").Value!;
            this._factory.HostName = configuration.GetSection("rabbit:hostName").Value!;
            this._queueName = configuration.GetSection("rabbit:queueName").Value!;
            this._queueNameDestination = configuration.GetSection("rabbit:queueNameDestination").Value!;
            this._factory.ClientProvidedName = nameof(RabbitMQService);

            var connection = Task.Run(async () => await this._factory.CreateConnectionAsync());

            Task.WhenAll(connection);

            this._connection = connection.Result;

            var channel = Task.Run(async () => await this._connection.CreateChannelAsync());

            Task.WhenAll(channel);

            this._channel = channel.Result;

        }

        public async Task ReceiveAsync(OrderMessage mensagem)
        {
            try
            {
                this._logger.LogInformation("");
                var bp = new BasicProperties();
                bp.DeliveryMode = DeliveryModes.Persistent;
                bp.Type = "VanillaMessage";
                var ea = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<OrderMessage>(mensagem,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }));
                await this._channel.BasicPublishAsync("backoffice", this._queueNameDestination, true, bp, ea);
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex, "Error during publishing the message to Order");
                throw;
            }
        }

        public async Task SendAsync(OrderMessage mensagem)
        {
            try
            {
                await this._channel.BasicPublishAsync("backoffice", this._queueNameDestination, false,  UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensagem)));
            }
            catch (Exception ex)
            {

                this._logger.LogCritical(ex, "Error during sending the message to Order");
                throw;
            }
        }
    }
}
