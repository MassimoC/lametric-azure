using fx_state.DTOs;
using fx_state.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace fx_state
{
    public static class AzSubscriptionMgmtApi
    {
        [FunctionName("Get_SubscriptionState")]
        public static async Task<IActionResult> GetSubscriptionDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subs/{subName}")] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            string subName,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            var entityId = new EntityId(nameof(SubscriptionState), subName);
            var state = await client.ReadEntityStateAsync<SubscriptionState>(entityId);
            if (!state.EntityExists) return new NotFoundObjectResult($"subscription not found {subName}");

            string qsFormat = req.Query["format"];
            if (!String.IsNullOrEmpty(qsFormat) && (qsFormat == "raw"))
            {
                return new OkObjectResult(state);
            }

            var result = GetLaMetricResult(state);
            return new OkObjectResult(result);
        }

        [FunctionName("Update_SubscriptionState")]
        public static async Task<IActionResult> SetValues(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "subs/{subName}")] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            string subName,
            ILogger log)
        {

            log.LogInformation("C# HTTP trigger function processed a request.");
            string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (String.IsNullOrEmpty(reqBody)) return new BadRequestObjectResult("invalid message");

            if (Int32.TryParse(reqBody, out int counter))
            {
                var entityId = new EntityId(nameof(SubscriptionState), subName);
                await client.SignalEntityAsync<ISubscriptionState>(entityId, e => e.SetCounter(counter));
                return new OkObjectResult("ok");
            }

            return new BadRequestObjectResult("invalid message");

        }

        private static LaMetricResult GetLaMetricResult(EntityStateResponse<SubscriptionState> state)
        {

            var increment = state.EntityState.LastRead - state.EntityState.PreviousRead;
            LaMetricIcon icon = increment > 0 ? LaMetricIcon.Up :
                (increment < 0 ? LaMetricIcon.Down : LaMetricIcon.NoChanges);

            var lmText = (icon == LaMetricIcon.Up) ?
                String.Format("+{0}", increment.ToString()) :
                String.Format("{0}", increment.ToString());

            var lametricResult = new LaMetricResult();
            lametricResult.Frames = new List<LaMetricText>
            {
               new LaMetricText { IconCode=icon, Text=lmText },
               new LaMetricText { IconCode=LaMetricIcon.Azure, Text=state.EntityState.LastRead.ToString() }
            };

            return lametricResult;
        }
    }
}
