using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaxiAIBot.Model;

namespace TaxiAIBot.Service
{
    public class TaxiAIBotAccessors
    {
        public TaxiAIBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
      
        }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        public IStatePropertyAccessor<UserInfo> UserInfoAccessor { get; set; } 
        public ConversationState ConversationState { get; }
        public UserState UserState { get; }
        public IStatePropertyAccessor<bool> DidBotWelcomeUserAccessor { get; set; } 
    }
}
