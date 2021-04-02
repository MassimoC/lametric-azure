using Newtonsoft.Json;
using System.Collections.Generic;

namespace fx_state.DTOs
{

    public class LaMetricResult
    {

        [JsonProperty("frames")]
        public List<LaMetricText> Frames { get; set; }
    }

    public class LaMetricText
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("icon")]
        public LaMetricIcon IconCode { get; set; }
    }

    public enum LaMetricIcon
    {
        Up = 120,
        Down = 124,
        NoChanges = 401,
        Azure = 37287
    }

}
