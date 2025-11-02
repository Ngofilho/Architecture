using Logistics;
using Logistics.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

var host = builder.Build();

host.Run();
