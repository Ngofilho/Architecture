using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnection>(sp => 
{
    var hostName = $"{Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME")}" ?? "localhost";
    var factory = new ConnectionFactory
    {
        Uri = new Uri($"amqp://guest:guest@{hostName}/")
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();    
})
    .AddHealthChecks()
    .AddRabbitMQ(tags: new string[] {"RabbitMQ", "BackOffice", "BusService"}, name: "BusService-RabbitMQ");

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.AddHealthCheckEndpoint("Health Check Service Bus", $"http://healthcheck:8080/health");    
}).AddInMemoryStorage();


var app = builder.Build();

app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/health", new HealthCheckOptions 
{ 
    Predicate = p => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecksUI(options => { options.UIPath = "/dashboard"; });


app.Run();