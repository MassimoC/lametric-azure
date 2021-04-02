using fx_graph_query.Config;
using fx_graph_query.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace fx_graph_query
{
    public class AzResourceCount
    {

        /// <summary>
        /// ref to the injected configuration
        /// </summary>
        private readonly FxSettings _appSettings;
        private readonly ILogger<AzResourceCount> _logger;
        private readonly IResourceGraph _azResourceGraph;
        private readonly HttpClient _httpClient;


        public AzResourceCount(IResourceGraph azResourceGraph, FxSettings appSettings, HttpClient httpClient, ILogger<AzResourceCount> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
            _azResourceGraph = azResourceGraph;
            _httpClient = httpClient;
        }



        [FunctionName(nameof(AzResourceCount))]
        // 0 */5 * * * *
        // every 5 mins
        public async Task RunOnSchedule(
                [TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, 
                ILogger log)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            List<string> subscriptionIds = _appSettings.SubscriptionIdentifiers.ToList();
            var azureGraphQuery = "Resources | summarize count()";
          
            try
            {
                _azResourceGraph.InitWithSP(
                    _appSettings.AzCredentials.ClientId,
                    _appSettings.AzCredentials.ClientSecret,
                    _appSettings.AzCredentials.TenantId
                    );

                var jsonResult = await _azResourceGraph.QueryAsync("count-az-resources", azureGraphQuery, subscriptionIds);

                if (jsonResult.Count >= 0)
                {
                    var rows = jsonResult["rows"];
                    string totalAzResources = rows[0]?.ToString(Formatting.None).Replace("[","").Replace("]", "");
                    _logger.LogInformation($"Total resources found: {totalAzResources}");

                    var httpContent = new StringContent(totalAzResources, Encoding.UTF8, "text/plain");

                    var response = await Policy
                        .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                        .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(5)
                        }, (result, timeSpan, retryCount, ctx) =>
                        {
                            log.LogWarning($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Attempt #{retryCount}");
                        })
                        .ExecuteAsync(() => _httpClient.PutAsync(_appSettings.ApiEndpoint, httpContent));

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong");
            }
        }
    }
}
