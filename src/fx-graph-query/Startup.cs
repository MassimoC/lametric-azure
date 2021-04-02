using fx_graph_query;
using fx_graph_query.Config;
using fx_graph_query.Interfaces;
using fx_graph_query.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace fx_graph_query
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>();
            
            //Get all configuration information deserialized to an instance of our configuration root class.
            var typedConfig = config.Get<FxSettings>();

            // Add configuration data as services.
            builder.Services.AddSingleton(typedConfig);
            builder.Services.AddSingleton(config);
            builder.Services.AddTransient<IResourceGraph, AzResourceGraph>();
            builder.Services.AddHttpClient("AzResourceCount", client =>
            {
                client.BaseAddress = new Uri(typedConfig.ApiEndpoint);
            });

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName("FX-Graph-Query")
                .Enrich.WithVersion()
                .WriteTo.Console()
                .WriteTo.AzureApplicationInsights(typedConfig.Observability.InstrumentationKey)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProvidersExceptFunctionProviders();
                loggingBuilder.AddSerilog(logger);
            });

        }
    }
}
