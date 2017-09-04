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

namespace Maggot.Dialogs
{
    [Serializable]
    public class JiraOAuthDialog : IDialog<string>
    {
        public static readonly string RequestTokenKey = "RequestToken";
        public static readonly string AccessTokenKey = "AccessToken";
        public static readonly string AuthTokenKey = "AuthToken";
        public static readonly string FormContentKey = "FormContent";
        private const string JiraBaseUrl = "https://mapmynumber.atlassian.net";

        protected JiraAdapter jiraAdapter;

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);
            RequestToken requestToken = null;
            AccessToken accessToken = null;

            if(jiraAdapter == null)
            {
                jiraAdapter = new JiraAdapter(JiraBaseUrl);
            }

            if (msg.Text.StartsWith("BotAuthorized:") && context.PrivateConversationData.TryGetValue(RequestTokenKey, out requestToken))
            {
                try
                {
                    // Dialog is resumed by the OAuth callback and access token
                    // is encoded in the message.Text
                    var oauthToken = JiraAuthHelper.DecryptString(msg.Text.Remove(0, "BotAuthorized:".Length).Trim());

                    var tempAccessToken = jiraAdapter.GetAccessToken(requestToken.OAuthToken);

                    context.PrivateConversationData.SetValue(AccessTokenKey, tempAccessToken);

                    await context.PostAsync("Authentication Successfull !! What can I do for you today ?");

                    context.Wait(this.MessageReceivedAsync);

                    // Jira.CreateRestClient()

                }
                catch (Exception e)
                {

                }
            }
            else if (context.PrivateConversationData.TryGetValue(AccessTokenKey, out accessToken))
            {
                var projects = jiraAdapter.GetProjectList(accessToken);
                
                if(projects.Length == 0)
                {
                    await context.PostAsync("You dont' have any projects assigned yet. Please contact your admin.");
                }
                if (projects.Length == 1)
                {
                    await context.PostAsync($"You have been assigned {projects.First().name} project. What would you wish to do further: ");
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Attachments = ProjectUIHelper.GetProjectListCard(projects);
                    await context.PostAsync(reply);
                }

                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await LogIn(context);
            }
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