using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fx_graph_query.Interfaces
{
    public interface IResourceGraph
    {
        IResourceGraph InitWithSP(string clientId, string clientSecret, string tenantId);
        Task<JObject> QueryAsync(string name, string query, List<string> subscriptionId);
    }
}
