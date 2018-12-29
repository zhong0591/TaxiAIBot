using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using TaxiAIBot.Common;
using TaxiAIBot.Model;

namespace TaxiAIBot.Dialog
{
    public static class LoginDialog
    {
        public static string temp_username;


        public static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[StringHelper.PROMPT_USERINFO] = new UserInfo();
            return await stepContext.PromptAsync(
               StringHelper.PROMPT_USERNAME,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") },
                cancellationToken);
        }



        public static async Task<DialogTurnResult> PasswordStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var username = (string)stepContext.Result;
            ((UserInfo)stepContext.Values[StringHelper.PROMPT_USERINFO]).Username = username;
            temp_username = username;
            return await stepContext.PromptAsync(
                StringHelper.PROMPT_PASSWORD,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your password."), RetryPrompt = MessageFactory.Text("Username or password is invalid, please try again.") },
                cancellationToken);
        }


        public static Task<bool> PasswordPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var result = promptContext.Recognized.Value;
            //To do 
            if (temp_username == "Seth" && result == "123456")
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public static async Task<DialogTurnResult> SaveDataStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userinfo = (UserInfo)stepContext.Values[StringHelper.PROMPT_USERINFO];
            userinfo.Password = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync($"Welcome {userinfo.Username}");
            return await stepContext.EndDialogAsync(stepContext.Values[StringHelper.PROMPT_USERINFO], default(CancellationToken));  
        }  
    }
}
