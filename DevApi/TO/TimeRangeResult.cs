using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api_ng_sample_code.TO
{
    public class TimeRangeResult
    {
        [JsonProperty(PropertyName = "timeRange")]
        public TimeRange timeRange { get; set; }

        [JsonProperty(PropertyName = "marketCount")]
        public int marketCount { get; set; }
       
    }
}
