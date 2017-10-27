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
        public static IList<Microsoft.Bot.Connector.Attachment> GetInitialMenu()
        {
            IList<Microsoft.Bot.Connector.Attachment> attachment = new List<Microsoft.Bot.Connector.Attachment>();

            var heroCard = new HeroCard
            {
                Title = "Select an Item to proceed"
            };
 
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Projects", value: ($"Project Selected")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Issues", value: ($"Issue Selected")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Profile", value: ($"Profile Selected")));

            attachment.Add(heroCard.ToAttachment());

            return attachment;
        }

        public static IList<Microsoft.Bot.Connector.Attachment> GetActionItemForProject()
        {
            IList<Microsoft.Bot.Connector.Attachment> attachment = new List<Microsoft.Bot.Connector.Attachment>();

            var heroCard = new HeroCard
            {
                Title = "Select an action to proceed"
            };

            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Project details", value: ($"Project details")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "List all Versions", value: ($"List all issues")));

            attachment.Add(heroCard.ToAttachment());

            return attachment;
        }

        public static IList<Microsoft.Bot.Connector.Attachment> GetProjectListCard(RemoteProject[] projects)
        {
            IList<Microsoft.Bot.Connector.Attachment> attachment = new List<Microsoft.Bot.Connector.Attachment>();

            var heroCard = new HeroCard
            {
                Title = "Select a Project to proceed"
            };

            for (int i = 0; i < projects.Length; i++)
            {
                heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, projects[i].name, value: ($"{projects[i].id}")));
            }

            attachment.Add(heroCard.ToAttachment());

            return attachment;
        }

        public static IList<Microsoft.Bot.Connector.Attachment> DisplayProjectDetails(RemoteProject project)
        {
            IList<Microsoft.Bot.Connector.Attachment> attachment = new List<Microsoft.Bot.Connector.Attachment>();

            var heroCard = new HeroCard
            {
                Title = project.name,
                Subtitle = project.description,
                Text = "Category : " + project.projectCategory + "\nLead : " + project.lead
            };

            attachment.Add(heroCard.ToAttachment());

            return attachment;
        }
    }
}