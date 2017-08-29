using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maggot.Model
{
    [Serializable]
    public class AccessToken : RequestToken
    {
        [JsonProperty(PropertyName = "oauth_expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "oauth_session_handle")]
        public string OAuthSessionHandle { get; set; }

        [JsonProperty(PropertyName = "oauth_authorization_expires_in")]
        public long AuthorizationExpiresIn { get; set; }

        //oauth_token=Kbx3Xt7bh8zFpjz5pQbPC4StJXGP7ohA&
        //oauth_token_secret=sQkm7ulnqR5UEux055h2H8BAVqcqHZmd&
        //oauth_expires_in=157680000&
        //oauth_session_handle=arg8vdXR8fgf6hzbcOpfB7fRI0B4l5to&
        //oauth_authorization_expires_in=160272000



    }
}