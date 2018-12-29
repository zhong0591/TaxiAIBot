// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxiAIBot.Common;
using TaxiAIBot.Dialog;
using TaxiAIBot.Model;
using TaxiAIBot.Service;


namespace TaxiAIBot
{

    public class TaxiAIBotBot : IBot
    {

        private readonly TaxiAIBotAccessors _accessors;
        private readonly DialogSet _dialogs;

        public TaxiAIBotBot(TaxiAIBotAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _dialogs = new DialogSet(accessors.DialogStateAccessor);
            _dialogs
                .Add(new TextPrompt(StringHelper.PROMPT_USERNAME))
                .Add(new TextPrompt(StringHelper.PROMPT_PASSWORD, LoginDialog.PasswordPromptValidatorAsync));

            WaterfallStep[] steps_login = new WaterfallStep[] {
                LoginDialog.NameStepAsync,
                LoginDialog.PasswordStepAsync,
                LoginDialog.SaveDataStepAsync
            };
            _dialogs.Add(new WaterfallDialog(StringHelper.DIA_LOGIN, steps_login));

            var account_slots = new List<SlotDetails>
            {
                new SlotDetails(StringHelper.PROMPT_FIRSTNAME, "text", "Please enter your first name."),
                new SlotDetails(StringHelper.PROMPT_LASTNAME, "text", "Please enter your last name."),
                new SlotDetails(StringHelper.PROMPT_EMAIL, "email", "Please enter your email.","Email format is incorrect, please try again."),
                new SlotDetails(StringHelper.PROMPT_PHONE, "text", "Please enter your phone number."),
                new SlotDetails(StringHelper.PROMPT_ADDRESS, "text", "Please enter your address."),
            };
            _dialogs.Add(new SlotFillingDialog(StringHelper.DIA_CREATE_ACCOUNT, account_slots));
            _dialogs.Add(new TextPrompt("email", AccountDialog.EmailPromptValidatorAsync));
            _dialogs.Add(new WaterfallDialog(StringHelper.DIA_REGISTER, new WaterfallStep[] { AccountDialog.CreateAccountDialogAsync, AccountDialog.ProcessResultsAsync }));
            _dialogs.Add(new TextPrompt("text"));

            var booking_slots = new List<SlotDetails>
            {
                new SlotDetails(StringHelper.PROMPT_BOOKING_NAME, "booking", "Please enter your name."),
                new SlotDetails(StringHelper.PROMPT_BOOKING_PHONE, "booking", "Please enter your phone."),
                new SlotDetails(StringHelper.PROMPT_BOOKING_ADDRESS, "booking", "Please enter your address."),
                new SlotDetails(StringHelper.PROMPT_BOOKING_DESTINATION, "booking", "Please enter your destination."), 
            };
            _dialogs.Add(new SlotFillingDialog(StringHelper.DIA_CREATE_BOOKING, booking_slots));
            _dialogs.Add(new WaterfallDialog(StringHelper.DIA_BOOKING, new WaterfallStep[] { BookingDialog.CreateBookingDialogAsync, BookingDialog.ProcessResultsAsync }));
            _dialogs.Add(new TextPrompt("booking"));
        }


        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            var didBotWelcomeUser = await _accessors.DidBotWelcomeUserAccessor.GetAsync(turnContext, () => false);
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var text = turnContext.Activity.Text.ToLowerInvariant();
                DialogContext dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                DialogTurnResult results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (didBotWelcomeUser == false)
                {
                    await _accessors.DidBotWelcomeUserAccessor.SetAsync(turnContext, true);
                    await WelcomeDialog.SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
                switch (results.Status)
                {
                    case DialogTurnStatus.Cancelled:
                    case DialogTurnStatus.Empty:
                        await _accessors.UserInfoAccessor.SetAsync(turnContext, new UserInfo(), cancellationToken);
                        if (StringHelper.STR_LOGIN == text)
                        {
                            await dialogContext.BeginDialogAsync(StringHelper.DIA_LOGIN, cancellationToken);
                        }
                        else if (StringHelper.STR_REGISTER == text)
                        {
                            await turnContext.SendActivityAsync("Let's start create a new user.");
                            await dialogContext.BeginDialogAsync(StringHelper.DIA_REGISTER, cancellationToken);
                        }
                        else if (StringHelper.STR_BOOKING == text) {
                            await turnContext.SendActivityAsync("Let's start create a new booking.");
                            await dialogContext.BeginDialogAsync(StringHelper.DIA_BOOKING, cancellationToken);
                        }
                        break;

                    case DialogTurnStatus.Complete: 

                        await WelcomeDialog.SendSuggestedActionsAsync(turnContext, cancellationToken);  
                        break;

                    case DialogTurnStatus.Waiting:
                        // If there is an active dialog, we don't need to do anything here.
                        break;
                }
                await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }

            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    await WelcomeDialog.SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
            }
            await _accessors.UserState.SaveChangesAsync(turnContext);
        }
    }
}
