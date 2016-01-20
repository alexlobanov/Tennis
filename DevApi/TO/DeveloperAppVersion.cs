using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api_ng_sample_code.TO
{
    public class DeveloperAppVersion
    {
        [JsonProperty(PropertyName = "owner")]
        public string owner { get; set; }

        [JsonProperty(PropertyName = "versionId")]
        public long versionId { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string version { get; set; }

        [JsonProperty(PropertyName = "applicationKey")]
        public string applicationKey { get; set; }

        [JsonProperty(PropertyName = "delayData")]
        public bool delayData { get; set; }

        [JsonProperty(PropertyName = "subscriptionRequired")]
        public bool subscriptionRequired { get; set; }

        [JsonProperty(PropertyName = "ownerManaged")]
        public bool ownerManaged { get; set; }

        [JsonProperty(PropertyName = "active")]
        public bool active { get; set; }
    }
}
