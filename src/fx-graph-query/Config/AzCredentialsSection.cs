namespace fx_graph_query.Config
{

    public class AzCredentialsSection
    {

        /// <summary>
        /// Service Principal Client Id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Service Principal Passowrd
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Azure Tenant Identifier
        /// </summary>
        public string TenantId { get; set; }


    }
}
