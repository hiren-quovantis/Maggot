using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maggot.Model
{
    [Serializable]
    public class RequestToken
    {

        [JsonProperty(PropertyName = "oauth_token")]
        public string OAuthToken { get; set; }

        [JsonProperty(PropertyName = "oauth_token_secret")]
        public string OAuthTokenSecret { get; set; }
    }
}