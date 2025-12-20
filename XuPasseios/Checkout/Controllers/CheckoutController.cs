using Checkout.Services;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : Controller
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRabbitMQService rabbitMQ;

        public CheckoutController(ILogger<WeatherForecastController> logger, IRabbitMQService rabbitMQ)
        {
            _logger = logger;
            this.rabbitMQ = rabbitMQ;
        }

        [HttpPost]
        public IActionResult Index([FromBody] OrderMessage order)
        {
            var rabbitMQPublish = this.rabbitMQ.SendAsync(order);
            //var rabbitMQPublish = this.rabbitMQ.SendAsync(new RabbitMQService.Mensagem { Date = DateTime.UtcNow, OrderId = new Random().Next() });
            return Created();
            //return View();
        }
    }
}
