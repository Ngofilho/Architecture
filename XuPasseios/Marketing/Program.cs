using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Marketing;
using Marketing.Services;
using Serilog;
using Serilog.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

/*inicio do config do log*/
var seriLogConfiguration = new LoggerConfiguration()
    .WriteTo.Elasticsearch(new[] { new Uri("http://localhost:9200") }, opts =>
    {
        /*opts.DataStream = new DataStreamName("logs", "globoticket-logs", "demo");
        
        opts.BootstrapMethod = BootstrapMethod.Failure;*/
    });

builder.Services.AddHostedService<Worker>()
    .AddLogging((logginbuilder) => 
    {
        logginbuilder.ClearProviders();
        logginbuilder.AddProvider(new SerilogLoggerProvider(seriLogConfiguration.CreateLogger())).AddConsole();
    });

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRepository, Repository>();
/*builder.Services.AddDbContext<CatalogContext>(dbContextOptions
    => dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CatalogDBConnectionString"]));

*/



var host = builder.Build();

host.Run();
