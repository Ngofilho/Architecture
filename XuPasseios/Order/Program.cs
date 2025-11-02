using Order;
using Order.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

var host = builder.Build();

host.Run();
