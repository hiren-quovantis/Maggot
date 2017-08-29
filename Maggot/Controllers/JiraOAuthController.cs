using Maggot.Common.OAuthHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Maggot.Controllers
{
    public class JiraOAuthController : ApiController
    {
        [HttpGet]
        [Route("api/OAuthCallback/")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri(Name = "oauth_token")] string code, CancellationToken token)
        {
            try
            {
                // Get the resumption cookie
                //BotAuth botdata = JsonConvert.DeserializeObject<BotAuth>(GoogleAuthHelper.Base64Decode(state));
                return Request.CreateResponse(string.Format("Copy Paste the following code and sent it to the bot {0}:{1}", "BotAuthorized", JiraAuthHelper.EncryptString(code)));

            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
