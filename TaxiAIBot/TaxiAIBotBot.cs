// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using TaxiAIBot.Common;
using TaxiAIBot.Model;
using TaxiAIBot.Service;

namespace TaxiAIBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. Objects that are expensive to construct, or have a lifetime
    /// beyond a single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class TaxiAIBotBot : IBot
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>                        


        // Define constants for the bot...

        // Define properties for the bot's accessors and dialog set.
        private readonly TaxiAIBotAccessors _accessors;
        private readonly DialogSet _dialogs;

        // Initialize the bot and add dialogs and prompts to the dialog set.
        public TaxiAIBotBot(TaxiAIBotAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            // Create a dialog set for the bot. It requires a DialogState accessor, with which
            // to retrieve the dialog state from the turn context.
            _dialogs = new DialogSet(accessors.DialogStateAccessor);


            // Add the prompts we need to the dialog set.
            _dialogs
                .Add(new TextPrompt(StringHelper.USERNAME_PROMPT))
                .Add(new TextPrompt(StringHelper.PASSWORD_PROMPT));
            // .Add(new ChoicePrompt(SelectionPrompt));

            // Add the dialogs we need to the dialog set.
            _dialogs.Add(new WaterfallDialog(StringHelper.TOP_LEVEL_DIALOG)
                .AddStep(NameStepAsync)
                .AddStep(PasswordStepAsync)
                .AddStep(SaveDataStepAsync)
                );

            //.AddStep(AcknowledgementStepAsync));

            //_dialogs.Add(new WaterfallDialog(ReviewSelectionDialog)
            //    .AddStep(SelectionStepAsync)
            //    .AddStep(LoopStepAsync));
        }

        // The first step of the top-level dialog.
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            stepContext.Values[StringHelper.USER_INFO] = new UserInfo();

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(
               StringHelper.USERNAME_PROMPT,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") },
                cancellationToken);
        }

        // The second step of the top-level dialog.
        private async Task<DialogTurnResult> PasswordStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Set the user's name to what they entered in response to the name prompt.
            ((UserInfo)stepContext.Values[StringHelper.USER_INFO]).Username = (string)stepContext.Result;

            // Ask the user to enter their age.
            return await stepContext.PromptAsync(
                StringHelper.PASSWORD_PROMPT,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your password.") },
                cancellationToken);
        }


        private async Task<DialogTurnResult> SaveDataStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            ((UserInfo)stepContext.Values[StringHelper.USER_INFO]).Password = (string)stepContext.Result;
            return await stepContext.EndDialogAsync(stepContext.Values[StringHelper.USER_INFO], default(CancellationToken));
        }


        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {


            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var didBotWelcomeUser = await _accessors.DidBotWelcomeUserAccessor.GetAsync(turnContext, () => false);


            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Run the DialogSet - let the framework identify the current state of the dialog from
                // the dialog stack and figure out what (if any) is the active dialog.
                DialogContext dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                DialogTurnResult results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (didBotWelcomeUser == false)
                {
                    await turnContext.SendActivityAsync($"You are seeing this message because this was your firstmessage ever to this bot.", cancellationToken: cancellationToken);
                    await _accessors.DidBotWelcomeUserAccessor.SetAsync(turnContext, true);
                    await _accessors.UserState.SaveChangesAsync(turnContext);
                }


                switch (results.Status)
                {
                    case DialogTurnStatus.Cancelled:
                    case DialogTurnStatus.Empty:
                        // If there is no active dialog, we should clear the user info and start a new dialog.
                        await _accessors.UserInfoAccessor.SetAsync(turnContext, new UserInfo(), cancellationToken);
                        await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                        await dialogContext.BeginDialogAsync(StringHelper.TOP_LEVEL_DIALOG, null, cancellationToken);
                        break;

                    case DialogTurnStatus.Complete:
                        // If we just finished the dialog, capture and display the results.
                        UserInfo userInfo = results.Result as UserInfo;


                        string status = "Welcome  " + userInfo.Username;
                        await turnContext.SendActivityAsync(status);
                        await _accessors.UserInfoAccessor.SetAsync(turnContext, userInfo, cancellationToken);
                        await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                        break;

                    case DialogTurnStatus.Waiting:
                        // If there is an active dialog, we don't need to do anything here.
                        break;
                }

                await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }

            // Processes ConversationUpdate Activities to welcome the user.
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Welcome new users...


            }
            else
            {
                // Give a default reply for all other activity types...

                // await turnContext.SendActivityAsync($"Welcome!");
            }

            await _accessors.UserState.SaveChangesAsync(turnContext);
        }

    }
}
