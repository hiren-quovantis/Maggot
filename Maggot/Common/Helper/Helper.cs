using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maggot.Common.Helper
{
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
    }
}