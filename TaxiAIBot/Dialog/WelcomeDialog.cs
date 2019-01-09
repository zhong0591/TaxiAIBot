using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxiAIBot.Dialog
{
    public static class WelcomeDialog
    {

        private const string WelcomeText = "Welcome to Taxi Bot.  Type anything to get started.";

        public static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = turnContext.Activity.CreateReply();
                    reply.Text = WelcomeText;
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }



        public static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var response = turnContext.Activity.CreateReply();

            SigninCard card = new SigninCard();
            card.Text = "please select a function.";
            card.Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, "Register", text: "Register", displayText: "Register", value: "Register"),
                    new CardAction(ActionTypes.ImBack, "Login", text: "Login", displayText: "Login", value: "Login"),
                    new CardAction(ActionTypes.ImBack, "Booking", text: "Booking", displayText: "Create a booking", value: "Booking"),
                    new CardAction(ActionTypes.ImBack, "Help", text: "Help", displayText: "Help", value: "Help"),
                };
            response.Attachments = new List<Attachment>() { card.ToAttachment() };


            //var suggestedActions = new SuggestedActions();
            //suggestedActions.Actions = new List<CardAction>
            //    {
            //        new CardAction(ActionTypes.ImBack, "Register", text: "Register", displayText: "Register", value: "Register"),
            //        new CardAction(ActionTypes.ImBack, "Login", text: "Login", displayText: "Login", value: "Login"),
            //        new CardAction(ActionTypes.ImBack, "Booking", text: "Booking", displayText: "Create a booking", value: "Booking"),
            //        new CardAction(ActionTypes.ImBack, "Help", text: "Help", displayText: "Help", value: "Help"),
            //    };
            //response.SuggestedActions = suggestedActions;
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

    }
}
