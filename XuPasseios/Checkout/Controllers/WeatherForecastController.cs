using Checkout.Services;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRabbitMQService rabbitMQ;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IRabbitMQService rabbitMQ)
        {
            _logger = logger;
            this.rabbitMQ = rabbitMQ;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            var rabbit = this.rabbitMQ.ReceiveAsync(new RabbitMQService.Mensagem { Date = DateTime.UtcNow, OrderId = new Random().Next(1, 1000) });
            return Created();
        }
    }
}
