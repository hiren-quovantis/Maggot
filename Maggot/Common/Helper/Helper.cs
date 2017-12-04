using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Atlassian.Jira.Remote;

namespace Maggot.Common.Helper
{

    public class PostBody
    {
        public Fields fields { get; set; }
    }

    public class Fields
    {
        public Assignee assignee { get; set; }
    }

    public class Assignee
    {
        public string name { get; set; }
    }

    public class IssueList
    {
        public RemoteIssue[] issues { get; set; }
    }

    public class Helper
    {
        public static List<KeyValuePair<string,string>> GetSearchJQLParameters(int projectId, string strOrderBy = "")
        {
            string strJql = string.Empty;
            System.Collections.Generic.List<string> parameters = new System.Collections.Generic.List<string>();

            if(string.IsNullOrEmpty(strOrderBy))
            {
                strOrderBy = "createdDate";
            }

            strJql = "project=" + projectId.ToString() + " order by " + strOrderBy;

            List<KeyValuePair<string, string>> content = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<string, string>("jql", strJql)
            };

            return content;
        }

        public static List<KeyValuePair<string, string>> GetParameters(Dictionary<string, string> dictParameters)
        {
            List<KeyValuePair<string, string>> content = new List<KeyValuePair<String, String>>();

            if (dictParameters != null && dictParameters.Count > 0)
            {
                foreach(var parameter in dictParameters)
                {
                    content.Add(new KeyValuePair<string, string>(parameter.Key, parameter.Value));
                }
            }

            return content;
        }
    }
}