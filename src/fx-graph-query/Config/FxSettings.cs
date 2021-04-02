using System.Collections.Generic;

namespace fx_graph_query.Config
{
    public class FxSettings
    {
        /// <summary>
        /// Connection to Azure Storage Account
        /// </summary>
        public string AzureWebJobsStorage { get; set; }

        /// <summary>
        /// Connection to the API
        /// </summary>
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// Observability settings.
        /// </summary>
        public ObservabilitySection Observability { get; set; }

        /// <summary>
        /// A collection Azure SubscriptionIDs.
        /// </summary>
        public ICollection<string> SubscriptionIdentifiers { get; set; }

        /// <summary>
        /// The container for all our application specific configuration settings.
        /// </summary>
        public AzCredentialsSection AzCredentials { get; set; }
    }
}
