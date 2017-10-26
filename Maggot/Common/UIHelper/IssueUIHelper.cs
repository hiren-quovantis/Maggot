using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Atlassian.Jira;
using Atlassian.Jira.Remote;
using Microsoft.Bot.Connector;

namespace Maggot.Common.UIHelper
{
    public static class IssueUIHelper
    {
        public static IList<Microsoft.Bot.Connector.Attachment> GetActionItemForIssue()
        {
            IList<Microsoft.Bot.Connector.Attachment> attachment = new List<Microsoft.Bot.Connector.Attachment>();

            var heroCard = new HeroCard
            {
                Title = "Select an action to proceed"
            };

            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Create", value: ($"Create Issue")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "View", value: ($"View Issue")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Edit", value: ($"Edit Issue")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Delete", value: ($"Delete Issue")));
            heroCard.Buttons.Add(new CardAction(ActionTypes.ImBack, "Assign", value: ($"Assign Issue")));

            attachment.Add(heroCard.ToAttachment());

            return attachment;
        }
    }
}