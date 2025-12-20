using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Order.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IConfiguration _configuration;

        private IConnection _connection;
        private IChannel _channel;
        private string _queueNameDestination;
        private readonly string _queueName;
        private readonly ConnectionFactory _factory;

        public RabbitMQService(ILogger<RabbitMQService> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;

            this._factory = new ConnectionFactory();
            
            this._factory.UserName = configuration.GetSection("rabbit:user").Value!;
            this._factory.Password = configuration.GetSection("rabbit:password").Value!;
            this._factory.VirtualHost = configuration.GetSection("rabbit:vhost").Value!;
            this._factory.HostName = configuration.GetSection("rabbit:hostName").Value!; //Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? configuration.GetSection("rabbit:hostName").Value!;
            //this._factory.HostName = "rabbitmqbackoffice";//Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? configuration.GetSection("rabbit:hostName").Value!;
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

        public async Task ReceiveAsync()
        {
            Mensagem body = null;// new();
            try
            {
                var consumer = new AsyncEventingBasicConsumer(this._channel);
                consumer.ReceivedAsync += async (ch, ea) =>
                {
                    this._logger.LogInformation($"Receiving message");
                    Mensagem? message = JsonSerializer.Deserialize<Mensagem>(Encoding.UTF8.GetString(ea.Body.ToArray()),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    body = message!;

                    this._logger.LogInformation($"Acknowledging the received message");
                    await this._channel.BasicAckAsync(ea.DeliveryTag, false);

                    string messageLog = $"Sending message to {_queueNameDestination}";
                    this._logger.LogInformation(messageLog);

                    await this._channel.BasicPublishAsync("backoffice", this._queueNameDestination, ea.Body);
                };
                await this._channel.BasicConsumeAsync(this._queueName,
                    false, typeof(RabbitMQService).FullName! + Guid.NewGuid().ToString(), consumer);

            }
            catch (Exception ex)
            {
                _logger.LogCritical(exception: ex, message: $"Error during the processing of the message", args: body);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service stopping.");

            this._channel?.CloseAsync();
            this._connection?.CloseAsync();

            _logger.LogInformation("RabbitMQ Consumer Service stopped.");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("Service disposing RabbitMQ channel");
            _channel?.Dispose();
            _logger.LogInformation("Service disposing RabbitMQ connection");
            _connection?.Dispose();
        }

        private class Mensagem
        {
            public int OrderId { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
