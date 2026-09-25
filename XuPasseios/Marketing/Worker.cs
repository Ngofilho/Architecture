using Common;
using Marketing.Dtos;
using Marketing.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Marketing
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;
        private readonly IRabbitMQService rabbitMQService;
        private readonly IRepository repository;
        private IChannel channel;


        public Worker(ILogger<Worker> logger, IConfiguration configuration, 
            IRabbitMQService rabbitMQService, IRepository repository)
        {
            _logger = logger;
            this.configuration = configuration;
            this.rabbitMQService = rabbitMQService;
            this.repository = repository;

            this._logger.LogInformation("Starting the RabbitMQ service for the Marketing");
            channel = this.rabbitMQService.ReceiveAsync();
            this._logger.LogInformation("Started the RabbitMQ service for the Marketing");
        }

        //public override async Task StartAsync(CancellationToken cancellationToken)
        //{
        //    this._logger.LogInformation("Starting the RabbitMQ service for the Marketing");
        //    channel =  this.rabbitMQService.ReceiveAsync(cancellationToken);
        //    this._logger.LogInformation("Started the RabbitMQ service for the Marketing");
        //    await base.StartAsync(cancellationToken);
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogInformation("Initializing the RabbitMQ receive method");

            while (!stoppingToken.IsCancellationRequested)
            {
                var r = await this.channel.BasicGetAsync(this.configuration.GetSection("rabbit:queueName").Value!, true);
                if (r != null)
                {   
                    var corpo = JsonSerializer.Deserialize<MarketingMessage>( Encoding.UTF8.GetString(r.Body.Span));
                    this._logger.LogInformation($"Corpo da Mensagem: {corpo}");

                    foreach (var item in corpo?.Order?.OrderDetails)
                    {
                        this.repository.Add(new Dtos.SaleDTO
                        {
                            
                            campaign = new Dtos.CampaignDTO
                            {
                                CampaignAuthor = 1
                            },
                            OrderDetails = new List<OrderDetails>() { 
                                new OrderDetails 
                                { 
                                    ItemId = Guid.NewGuid(),
                                    Quantity = item.Quantity,
                                    ClientId = item.ClientId,
                                    Price = item.Price
                                } },
                            Customers = new List<Person> 
                            { 
                                new Person 
                                { 
                                    ClientId = item.ClientId 
                                }  
                            }
                        });
                    }
                }
            }
            
            /*consumer.ReceivedAsync += (ch, ea) =>
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    this.Consumer_Received(ch, ea);
                }
                return Task.CompletedTask;
            };*/
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            this._logger.LogInformation($"##### Receiving message #####");
            this._logger.LogInformation(Encoding.UTF8.GetString(e.Body.ToArray()));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping the RabbitMQ service for the Marketing");
            await this.rabbitMQService.StopAsync(cancellationToken);
            this._logger.LogInformation("Disposing the RabbitMQ service for the Marketing");
            this.rabbitMQService.Dispose();
            this._logger.LogInformation("Stopped and disposed the RabbitMQ service for the Marketing");
        }
    }
}
