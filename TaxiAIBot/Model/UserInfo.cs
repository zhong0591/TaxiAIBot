using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxiAIBot.Model
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public bool DidBotWelcomeUser { get; set; } = false;
    }
}
