using Payment.Services;

namespace Payment
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;
        private readonly IRabbitMQService rabbitMQService;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IRabbitMQService rabbitMQService)
        {
            _logger = logger;
            this.configuration = configuration;
            this.rabbitMQService = rabbitMQService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Initializing the RabbitMQ receive method");
            await this.rabbitMQService.ReceiveAsync();
            this._logger.LogInformation("Initialized RabbitMQ receive method");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {}

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Stopping the RabbitMQ service for the Payment");
            await this.rabbitMQService.StopAsync(cancellationToken);
            this._logger.LogInformation("Disposing the RabbitMQ service for the Payment");
            this.rabbitMQService.Dispose();
            this._logger.LogInformation("Stopped and disposed the RabbitMQ service for the Payment");
        }
    }
}
