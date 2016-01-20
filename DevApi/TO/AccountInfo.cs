using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api_ng_sample_code.TO
{
    public class AccountInfo
    {
        [JsonProperty(PropertyName = "availableToBetBalance")]
        public string availableToBetBalance { get; set; }

        [JsonProperty(PropertyName = "exposure")]
        public string exposure { get; set; }

        [JsonProperty(PropertyName = "retainedCommission")]
        public string retainedCommission { get; set; }

        [JsonProperty(PropertyName = "exposureLimit")]
        public string exposureLimit { get; set; }

        [JsonProperty(PropertyName = "discountRate")]
        public string discountRate { get; set; }

        [JsonProperty(PropertyName = "pointsBalance")]
        public string pointsBalance { get; set; }

        [JsonProperty(PropertyName = "wallet")]
        public string wallet { get; set; }
    }
}
