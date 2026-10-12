using Logistics.Services;
using Common;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace Logistics
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;
        private readonly IRabbitMQService rabbitMQService;
        private IChannel channel;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IRabbitMQService rabbitMQService)
        {
            _logger = logger;
            this.configuration = configuration;
            this.rabbitMQService = rabbitMQService;

            this._logger.LogInformation("Initializing the RabbitMQ receive method");
            this.channel = this.rabbitMQService.Receive();
            this._logger.LogInformation("Initialized RabbitMQ receive method");
        }

        /*public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Initializing the RabbitMQ receive method");
            await this.rabbitMQService.ReceiveAsync();
            this._logger.LogInformation("Initialized RabbitMQ receive method");
        }*/
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogInformation("Starting the RabbitMQ service for the Logistics");
            while (!stoppingToken.IsCancellationRequested)
            {
                var rabbitMessage = await this.channel.BasicGetAsync(this.configuration.GetSection("rabbit:queueName").Value!, true);
                if (rabbitMessage != null)
                {
                    var messageBody = JsonSerializer.Deserialize<LogisticsMessage>(Encoding.UTF8.GetString(rabbitMessage.Body.Span));
                    this._logger.LogInformation($"Received message: {messageBody}");


                    var marketingMessage = new MarketingMessage
                    {
                        Order = new OrderMessage
                        {
                            OrderDetails = messageBody?.Order?.OrderDetails,

                            MessageId = Guid.NewGuid(),
                            DateOfSending = DateTime.UtcNow,
                            MessageVersion = "1.0",
                            Origin = "Logistics",
                            Destiny = "Marketing",
                            Customers = messageBody?.Order?.Customers                            
                        },
                        campaign = new CampaignDTO
                        {
                            idCampaign = 1,
                            Campaign = "Summer Sale",
                            CampaignAuthor = 1,
                            CampaignStart = DateTime.UtcNow,
                            CampaignEnd = DateTime.UtcNow.AddDays(30)
                        },
                        MessageId = messageBody.MessageId,
                        DateOfSending = messageBody.DateOfSending,
                        MessageVersion = messageBody.MessageVersion,
                        Origin = messageBody.Origin,
                        Destiny = "Marketing"
                    };

                    this.rabbitMQService.PublishMessage(marketingMessage);
                    this._logger.LogWarning("Message published to Marketing queue " , args : marketingMessage);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping the RabbitMQ service for the Logistics");
            await this.rabbitMQService.StopAsync(cancellationToken);
            this._logger.LogInformation("Disposing the RabbitMQ service for the Logistics");
            this.rabbitMQService.Dispose();
            this._logger.LogInformation("Stopped and disposed the RabbitMQ service for the Logistics");
        }
    }
}
