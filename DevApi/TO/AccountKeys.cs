using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api_ng_sample_code.TO
{
    public class AccountKeys
    {
        [JsonProperty(PropertyName = "appName")]
        public string appName { get; set; }

        [JsonProperty(PropertyName = "appId")]
        public string appId { get; set; }

        [JsonProperty(PropertyName = "appVersions")]
        public List<DeveloperAppVersion> appVersions { get; set; }

    }
}
