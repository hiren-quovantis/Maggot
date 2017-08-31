using Maggot.Common.OAuthHelper;
using Maggot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maggot.Adapter
{
    public static class JiraAdapter
    {
        public static string GetProjectList(string baseurl, AccessToken token)
        {
            return JiraAuthHelper.MakeGetRequest(token, baseurl + "/rest/api/2/project");
        }
    }
}