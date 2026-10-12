namespace TimerNamespace
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                Timer timer = new Timer(Palmeiras, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(59)); 
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        void Palmeiras(object? state)
        {
            Console.WriteLine("Palmeiras é o maior do mundo");
        }
    }
}
