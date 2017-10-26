using Atlassian.Jira.Remote;
using Maggot.Common.OAuthHelper;
using Maggot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Maggot.Adapter
{
    [Serializable]
    public class JiraAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl"></param>
        public JiraAdapter(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public RemoteProject[] GetProjectList(AccessToken token)
        {
            return JiraAuthHelper.MakeGetRequest<RemoteProject[]>(token, this.baseUrl + "/rest/api/2/project");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public RemoteProject GetProjectDetails(AccessToken token, int projectId)
        {
            return JiraAuthHelper.MakeGetRequest<RemoteProject>(token, this.baseUrl + "/rest/api/2/project/"+projectId.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RequestToken GetRequestToken()
        {
            //oauth_token=K5QFzzFJOIlSr0ooZQ4R8GFa1wj5OHyY&oauth_token_secret=4NY0cMJpOrUbLOmRZLxf45LJcVlbO6yI
            return JiraAuthHelper.MakeOAuthRequest<RequestToken>(this.baseUrl + RequestTokenUrlPart);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestToken"></param>
        /// <returns></returns>
        public AccessToken GetAccessToken(string requestToken)
        {
            //oauth_token=K5QFzzFJOIlSr0ooZQ4R8GFa1wj5OHyY&oauth_token_secret=4NY0cMJpOrUbLOmRZLxf45LJcVlbO6yI
            return JiraAuthHelper.MakeOAuthRequest<AccessToken>(this.baseUrl + AccessTokenUrlPart, requestToken);

        }

        #region " Private Variables "

        private string baseUrl = string.Empty;
        private const string RequestTokenUrlPart = "/plugins/servlet/oauth/request-token";
        private const string AuthorizeUrlPart = "/plugins/servlet/oauth/authorize";
        private const string AccessTokenUrlPart = "/plugins/servlet/oauth/access-token";

        #endregion
    }
}