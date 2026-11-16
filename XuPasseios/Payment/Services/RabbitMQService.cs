using Payment.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Common;

namespace Payment.Services
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

        public async Task<PaymentDTO> ReceiveAsync(CancellationToken cancellation)
        {
            PaymentDTO body = null;// new();

            try
            {
                var consumer = new AsyncEventingBasicConsumer(this._channel);
                consumer.ReceivedAsync += async (ch, ea) =>
                {
                    this._logger.LogInformation($"Receiving message");
                    Message? message = JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(ea.Body.ToArray()),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    body = (PaymentDTO)message!;

                    this._logger.LogInformation($"Acknowledging the received message");
                    await this._channel.BasicAckAsync(ea.DeliveryTag, false);
                };
                /*   await this._channel.BasicConsumeAsync(this._queueName,
                       false, typeof(RabbitMQService).FullName! + Guid.NewGuid().ToString(), consumer);
                */
                return body;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(exception: ex, message: $"Error during the processing of the message", args: body);
                throw;
            }
        }

        public async Task Send(PaymentDTO payment, CancellationToken cancellation)
        {
            //await this._channel.BasicPublishAsync("backoffice", this._queueNameDestination, ea.Body);

            try
            {
                var bp = new BasicProperties();
                bp.DeliveryMode = DeliveryModes.Persistent;
                bp.Type = "VanillaMessage";
                var ea = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<PaymentDTO>(payment,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }));
                await this._channel.BasicPublishAsync("backoffice", this._queueNameDestination, true, bp, ea);
                string messageLog = $"Sending message to {_queueNameDestination}";
                this._logger.LogInformation(messageLog);
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex, "Error during publishing the message to Order");
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
    }
}
