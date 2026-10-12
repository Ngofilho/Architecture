using Checkout.Services;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : Controller
    {
        private readonly ILogger<CheckoutController> _logger;
        private readonly IRabbitMQService rabbitMQ;

        public CheckoutController(ILogger<CheckoutController> logger, IRabbitMQService rabbitMQ)
        {
            _logger = logger;
            this.rabbitMQ = rabbitMQ;
        }

        [HttpPost]
        public IActionResult Index([FromBody] OrderMessage order)
        {
            this._logger.LogInformation($"Order received: {order}");
            var rabbitMQPublish = this.rabbitMQ.SendAsync(order);
            return Created();
        }
    }
}
