using Atlassian.Jira;
using Atlassian.Jira.Remote;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maggot.Common.UIHelper
{
    public static class ProjectUIHelper
    {
        public static IList<Microsoft.Bot.Connector.Attachment> GetProjectListCard(RemoteProject[] projects)
        {
            IList<Microsoft.Bot.Connector.Attachment> attachment = new List<Microsoft.Bot.Connector.Attachment>();

            var heroCard = new HeroCard
            {
                Title = "Select a Project to proceed"
            };

            for (int i = 0; i < projects.Length; i++)
            {
                heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, projects[i].name, value: ($"Project {projects[i].key} Selected")));
            }

            attachment.Add(heroCard.ToAttachment());

            return attachment;
        }
    }
}