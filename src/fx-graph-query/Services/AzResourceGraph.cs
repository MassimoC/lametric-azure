using fx_graph_query.Interfaces;
using GuardNet;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace fx_graph_query.Services
{
    public class AzResourceGraph : IResourceGraph
    {
        private readonly ILogger<AzResourceGraph> _logger;
        private ResourceGraphClient _azGraphClient;
        private AzureCredentials _credentials;
        


        public AzResourceGraph(ILogger<AzResourceGraph> logger)
        {
            Guard.NotNull(logger, nameof(logger));
            _logger = logger;
        }

        private ResourceGraphClient GetAzureGraphClient()
        {
            if (_azGraphClient == null)
            {
                _azGraphClient = CreateAzureGraphClient();
            }
            return _azGraphClient;
        }
        private ResourceGraphClient CreateAzureGraphClient()
        {
            Guard.NotNull(_credentials, nameof(_credentials));
            _azGraphClient = new ResourceGraphClient(_credentials);
            return _azGraphClient;
        }

        public IResourceGraph InitWithSP(string clientId, string clientSecret, string tenantId)
        {
            Guard.NotNullOrEmpty(clientId, nameof(clientId));
            Guard.NotNullOrEmpty(clientSecret, nameof(clientSecret));
            Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
            _credentials = new AzureCredentialsFactory().FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            return this;
        }

        public async Task<JObject> QueryAsync(string name, string query, List<string> subscriptionId)
        {
            Guard.NotNullOrWhitespace(name, nameof(name));
            Guard.NotNullOrWhitespace(query, nameof(query));
            Guard.NotNull(subscriptionId, nameof(subscriptionId));

            _logger.LogInformation($"Executing graph query: name [{name}] value [{query}]");

            var client = CreateAzureGraphClient();
            var azQuery = new QueryRequest
            {
                Subscriptions = subscriptionId,
                Query = query,
                Options = new QueryRequestOptions
                {
                    ResultFormat = ResultFormat.Table
                }
            };

            
            // dependency
            var durationMeasurement = new Stopwatch();
            var startTime = DateTimeOffset.UtcNow;
            object dependencyData = _azGraphClient.BaseUri;
            durationMeasurement.Start();

            var queryResult = await client.ResourcesAsync(azQuery);
                  
            _logger.LogDependency("AzureResourceGraph", dependencyData, "Microsoft.ResourceGraph", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);

            return queryResult.Data as JObject;
        }

    }
}
