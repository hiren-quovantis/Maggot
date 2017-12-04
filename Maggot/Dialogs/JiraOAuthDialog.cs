using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.ConnectorEx;
using Maggot.Common.OAuthHelper;
using static Maggot.Common.OAuthHelper.JiraAuthHelper;
using System.Net.Http;
using Maggot.Model;
using Atlassian.Jira;
using Maggot.Adapter;
using Maggot.Common.UIHelper;
using Maggot.Common.Enum;

namespace Maggot.Dialogs
{
    [Serializable]
    public class JiraOAuthDialog : IDialog<string>
    {
        public static readonly string RequestTokenKey = "RequestToken";
        public static readonly string AccessTokenKey = "AccessToken";
        public static readonly string AuthTokenKey = "AuthToken";
        public static readonly string FormContentKey = "FormContent";

        public static readonly string DialogStateKey = "DialogContextState";
        public static readonly string IssueContextKey = "IssueContext";
        public static readonly string ProjectIdKey = "ProjectId";

        private const string JiraBaseUrl = "https://mapmynumber.atlassian.net";

        protected ContextType DialogContextState;
        protected IssueContext SelectedIssueContext;
        private int ProjectId = 0;
        private string IssueKey = string.Empty;
        protected List<int> AvailableProjects = new List<int>();

        protected JiraAdapter jiraAdapter;

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);
            var msgText = msg.Text.ToLower();
            RequestToken requestToken = null;
            AccessToken accessToken = null;

            if(jiraAdapter == null)
            {
                jiraAdapter = new JiraAdapter(JiraBaseUrl);
            }

            if (msgText.StartsWith("botauthorized:") && context.PrivateConversationData.TryGetValue(RequestTokenKey, out requestToken))
            {
                try
                {
                    // Dialog is resumed by the OAuth callback and access token
                    // is encoded in the message.Text
                    var oauthToken = JiraAuthHelper.DecryptString(msg.Text.Remove(0, "BotAuthorized:".Length).Trim());

                    var tempAccessToken = jiraAdapter.GetAccessToken(requestToken.OAuthToken);

                    context.PrivateConversationData.SetValue(AccessTokenKey, tempAccessToken);

                    await context.PostAsync("Authentication Successfull !! What can I do for you today ?");

                    var reply = context.MakeMessage();
                    reply.Attachments = ProjectUIHelper.GetInitialMenu();
                    await context.PostAsync(reply);

                    context.Wait(this.MessageReceivedAsync);

                    // Jira.CreateRestClient()

                }
                catch (Exception e)
                {

                }
            }
            else if (context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken) && msgText.Equals("project selected"))
            {
                context.PrivateConversationData.TryGetValue(DialogStateKey, out DialogContextState);
                var projects = jiraAdapter.GetProjectList(accessToken);

                if (projects.Length == 0)
                {
                    await context.PostAsync("Oop's you dont' have any projects assigned yet. Please contact your admin.");
                }
                if (projects.Length == 1)
                {
                    await context.PostAsync($"You have been assigned {projects.First().name} project. What would you wish to do further: ");
                    DialogContextState = ContextType.Project;
                    context.PrivateConversationData.SetValue(DialogStateKey, DialogContextState);
                    AvailableProjects.Add(Convert.ToInt32(projects.First().id));
                }
                if (projects.Length > 1)
                {
                    var reply = context.MakeMessage();
                    reply.Attachments = ProjectUIHelper.GetProjectListCard(projects);
                    await context.PostAsync(reply);

                    DialogContextState = ContextType.Project;
                    context.PrivateConversationData.SetValue(DialogStateKey, DialogContextState);
                    foreach (var item in projects)
                    {
                        AvailableProjects.Add(Convert.ToInt32(item.id));
                    }
                }

                context.Wait(MessageReceivedAsync);
            }
            else if (context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken) && msgText.StartsWith("issue selected"))
            {
                var reply = context.MakeMessage();
                reply.Attachments = IssueUIHelper.GetActionItemForIssue();
                DialogContextState = ContextType.Issue;
                context.PrivateConversationData.SetValue(DialogStateKey, DialogContextState);
                await context.PostAsync(reply);
            }
            else if (context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken) && (msgText.Equals("Clear") || (msgText.StartsWith("hi") && msgText.Substring(msgText.IndexOf("hi")+2,1) == string.Empty)))
            {
                await context.PostAsync("Hello! What can I do for you today?");
                var reply = context.MakeMessage();
                reply.Attachments = ProjectUIHelper.GetInitialMenu();
                await context.PostAsync(reply);
            }
            else if (DialogContextState == ContextType.Project && AvailableProjects.Count > 0 && !string.IsNullOrEmpty(msg.Text))
            {
                await JiraProject(context, msgText);
            }
            else if (DialogContextState == ContextType.Issue && !string.IsNullOrEmpty(msg.Text))
            {
                await JiraIssue(context, msgText);
            }
            else if (context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken) && !string.IsNullOrEmpty(msgText))
            {
                await context.PostAsync("Uh! There's nothing like that, please try again.");
            }
            else
            {
                await LogIn(context);
            }
        }

        private async Task JiraIssue(IDialogContext context, string msg)
        {
            AccessToken accessToken = null;

            if (SelectedIssueContext == IssueContext.Assign)
            {
                if (!string.IsNullOrEmpty(IssueKey) && context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken))
                {
                    Dictionary<string, string> dictParameters = new Dictionary<string, string>();
                    dictParameters.Add("name", msg);

                    if (jiraAdapter.AssignIssue(accessToken, IssueKey, dictParameters))
                    {
                        await context.PostAsync("Issue assigned succesfully to: " + msg);
                    }
                    else
                    {
                        await context.PostAsync("An error occurred while assigning issue to: " + msg);
                    }
                }
                if (context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken)) {

                    if (!string.IsNullOrEmpty(msg) && jiraAdapter.GetIssue(accessToken, msg))
                    {
                        IssueKey = msg;
                        await context.PostAsync("Please provide assignee name");
                    }
                    else
                    {
                        await context.PostAsync("Invalid issue id/key, please enter again");
                    }
                }   
            }

            if (msg.Equals("assign issue") && context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken))
            {
                SelectedIssueContext = IssueContext.Assign;
                await context.PostAsync("Please provide issue id");
            }
        }

        private async Task JiraProject(IDialogContext context, string msg)
        {
            AccessToken accessToken = null;
            int projectId;      

            if(!context.PrivateConversationData.TryGetValue(ProjectIdKey, out ProjectId))
            {
                if (int.TryParse(msg, out projectId) && AvailableProjects.Contains(projectId))
                {
                    var reply = context.MakeMessage();
                    reply.Attachments = ProjectUIHelper.GetActionItemForProject();
                    await context.PostAsync(reply);
                    ProjectId = projectId;
                    context.PrivateConversationData.SetValue(ProjectIdKey, ProjectId);
                }
            }

            if (ProjectId != 0 && msg.Equals("project details") && context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken))
            {
                var projectDetails = jiraAdapter.GetProjectDetails(accessToken, ProjectId);
                if(projectDetails != null)
                {
                    var reply = context.MakeMessage();
                    reply.Attachments = ProjectUIHelper.DisplayProjectDetails(projectDetails);
                    await context.PostAsync(reply);
                }
            }
            else if (ProjectId != 0 && msg.Equals("list all issues") && context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken))
            {
                var issueDetails = jiraAdapter.GetProjectIssues(accessToken, ProjectId);
                if (issueDetails != null)
                {
                    var reply = context.MakeMessage();
                    reply.Attachments = IssueUIHelper.DisplayIssueDetails(issueDetails);
                    await context.PostAsync(reply);
                }
            }
            //else
            //{
            //    await context.PostAsync("Uh ho, didn't saw that coming! Please try again");
            //}
        }

        private async Task LogIn(IDialogContext context)
        {
            string token;

            if (!context.PrivateConversationData.TryGetValue(AuthTokenKey, out token))
            {
                //var conversationReference = context.Activity.ToConversationReference();
                //conversationReference.ActivityId = context.Activity.Id;

                // sending the sigin card with JIRA login url
                var reply = context.MakeMessage();

                var requestToken = jiraAdapter.GetRequestToken();

                var jiraLoginUrl = JiraAuthHelper.GetJiraOAuthUrl(JiraBaseUrl, requestToken);

                //context.PrivateConversationData.SetValue("persistedCookie", conversationReference);
                context.PrivateConversationData.SetValue(RequestTokenKey, requestToken);

                reply.Text = "Please login in to JIRA using this card";
                reply.Attachments.Add(SigninCard.Create("You need to authorize me",
                                                        "Login to Jira!",
                                                        jiraLoginUrl
                                                        ).ToAttachment());
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done(token);
            }
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }
    }
}