using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TaxiAIBot.Common;
using TaxiAIBot.Model;

namespace TaxiAIBot.Dialog
{
    public static class AccountDialog
    {
        public static async Task<DialogTurnResult> CreateAccountDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[StringHelper.PROMPT_USERINFO] = new UserInfo();
            return await stepContext.BeginDialogAsync(StringHelper.DIA_CREATE_ACCOUNT, null, cancellationToken);
        }


        public static async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // To demonstrate that the slot dialog collected all the properties we will echo them back to the user.
            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {
              
                var firstName = result[StringHelper.PROMPT_FIRSTNAME];
                var lastName = result[StringHelper.PROMPT_LASTNAME];
                var email = result[StringHelper.PROMPT_EMAIL];
                var address = result[StringHelper.PROMPT_ADDRESS];
                var phone = result[StringHelper.PROMPT_PHONE];
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{fullname["first"]} {fullname["last"]}"), cancellationToken);

                //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{result["shoesize"]}"), cancellationToken);

                stepContext.Values[StringHelper.PROMPT_USERINFO] = new UserInfo()
                {
                    FirstName = firstName.ToString(),
                    LastName = lastName.ToString(),
                    Address = address.ToString(),
                    Email = email.ToString(),
                    Phone = phone.ToString(), 
                }; 

                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{firstName}  is created"), cancellationToken);
            }

            // Remember to call EndAsync to indicate to the runtime that this is the end of our waterfall.
            return await stepContext.EndDialogAsync();
        }



        public static Task<bool> EmailPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var email = promptContext.Recognized.Value;
            string strExp = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex r = new Regex(strExp);
            Match m = r.Match(email); 
            return Task.FromResult(m.Success); 
        }
    }
}
