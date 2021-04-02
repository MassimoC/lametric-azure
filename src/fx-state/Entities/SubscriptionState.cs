using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace fx_state.Entities
{

    public interface ISubscriptionState
    {
        void SetCounter(int value);
        Task Reset();
        Task<int> Get();
        void Delete();


    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SubscriptionState : ISubscriptionState
    {
        [JsonProperty("Name")]
        public string Name { get; private set; }
        [JsonProperty("PreviousRead")]
        public int PreviousRead { get; private set; }
        [JsonProperty("LastRead")]
        public int LastRead { get; private set; }
        [JsonProperty("UpdatedOn")]
        public DateTime UpdateOn { get; private set; }

        public void SetCounter(int value)
        {
            if (value != this.LastRead)
            {
                this.PreviousRead = this.LastRead;
                this.LastRead = value;
                this.UpdateOn = DateTime.UtcNow;
            }
        }

        public Task Reset()
        {
            this.PreviousRead = 0;
            this.LastRead = 0;
            return Task.CompletedTask;
        }

        public Task<int> Get()
        {
            return Task.FromResult(LastRead);
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(SubscriptionState))]
        public Task ProcessUserState([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<SubscriptionState>();
        }
    }
}
