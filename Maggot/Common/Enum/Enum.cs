using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maggot.Common.Enum
{
    public enum ContextType
    {
        Project = 1,
        Issue = 2,
        User = 3
    };

    public enum IssueContext
    {
        Create=1,
        View=2,
        Edit=3,
        Delete=4,
        Assign=5
    };
}