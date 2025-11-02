using Marketing;
using Marketing.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

var host = builder.Build();

host.Run();
