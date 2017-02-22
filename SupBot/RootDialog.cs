using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
#pragma warning disable CS1998

namespace SupportBot
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Welcome to the support bot!");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            string name;
            if (context.ConversationData.TryGetValue("Name", out name))
            {
                PromptForProblem(context, name);
            }
            else
            {
                context.Call(FormDialog.FromForm<UserInfo>(UserInfo.BuildForm, FormOptions.PromptInStart), this.AfterUserInfoForm);
            }
        }

        public async Task AfterUserInfoForm(IDialogContext context, IAwaitable<UserInfo> result)
        {
            UserInfo userInfo = await result;

            context.ConversationData.SetValue("Name", userInfo.Name);
            context.ConversationData.SetValue("PhoneNumber", userInfo.PhoneNumber);
            context.ConversationData.SetValue("EmailAddress", userInfo.EmailAddress);
            context.ConversationData.SetValue("Department", userInfo.Department);

            PromptForProblem(context, userInfo.Name);
        }

        private void PromptForProblem(IDialogContext context, string name)
        {
            PromptDialog.Choice(context, AfterGetProblem, new List<string>() { "Crash", "Error", "Other" }, $"Welcome, {name}! What problem are you having?");
        }
        private async Task AfterGetProblem(IDialogContext context, IAwaitable<string> result)
        {
            string problem = await result;

            switch (problem.ToLower())
            {
                case "crash":
                    await context.PostAsync("To solve this error, please install the latest version of the software.  You can find it at http://newestversionsite");
                    context.ConversationData.SetValue("Issue", "Crash");

                    PromptForSuccess(context);
                    break;
                case "error":
                    PromptForError(context);
                    break;
                default:
                    PromptForIssue(context);
                    break;
            }
        }

        private void PromptForSuccess(IDialogContext context)
        {
            PromptDialog.Choice(context, AfterGetSuccess, new List<string>() { "Yes", "No" }, "Did this solve the issue?");
        }
        private async Task AfterGetSuccess(IDialogContext context, IAwaitable<string> result)
        {
            string success = await result;

            if (success.ToLower() == "yes")
            {
                await context.PostAsync("Great! Thank you for using the support bot.");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                PromptForIssue(context);
            }
        }

        private void PromptForError(IDialogContext context)
        {
            PromptDialog.Text(context, AfterGetError, "What error are you receiving?");
        }
        private async Task AfterGetError(IDialogContext context, IAwaitable<string> result)
        {
            string error = await result;

            if (error.Contains("60342"))
            {
                await context.PostAsync("Please try running the repair tool found at http://repairtoolsite");
                context.ConversationData.SetValue("Issue", "Error #60342");
                PromptForSuccess(context);
            }
            else if (error.Contains("62890"))
            {
                await context.PostAsync("To solve this error, please delete the temporary files located in C:\\tempfilelocation");
                context.ConversationData.SetValue("Issue", "Error #62890");
                PromptForSuccess(context);
            }
            else
            {
                context.ConversationData.SetValue("Issue", error);
                PromptForContactMethod(context);
            }
        }

        private void PromptForIssue(IDialogContext context)
        {
            PromptDialog.Text(context, AfterGetIssue, "Please explain in detail the issue you are having.");
        }
        private async Task AfterGetIssue(IDialogContext context, IAwaitable<string> result)
        {
            string issue = await result;
            context.ConversationData.SetValue("Issue", $"{issue}");
            PromptForContactMethod(context);
        }

        private void PromptForContactMethod(IDialogContext context)
        {
            PromptDialog.Choice(context, AfterGetContactMethod, new List<string>() { "Email", "Phone" }, $"How would you prefer for support to contact you?");
        }
        private async Task AfterGetContactMethod(IDialogContext context, IAwaitable<string> result)
        {
            string contactMethod = await result;

            string name;
            string email;
            string phoneNumber;
            string department;
            string issue;
            context.ConversationData.TryGetValue("Name", out name);
            context.ConversationData.TryGetValue("EmailAddress", out email);
            context.ConversationData.TryGetValue("PhoneNumber", out phoneNumber);
            context.ConversationData.TryGetValue("Department", out department);
            context.ConversationData.TryGetValue("Issue", out issue);

            if (contactMethod.ToLower() == "phone")
            {
                //Have support contact {name} from the {department} department at {phoneNumber} regarding {issue} via phone
            }
            else
            {
                //Have support contact {name} from the {department} department at {email} regarding {issue} via email
            }

            await context.PostAsync($"You will be contacted via {contactMethod} within the next hour. Thank you for using the support bot.");

            context.Wait(MessageReceivedAsync);
        }
    }
}
