// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxiAIBot.Common;
using TaxiAIBot.Model;
using TaxiAIBot.Service;

namespace TaxiAIBot
{
    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        } 
        public IConfiguration Configuration { get; } 
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<TaxiAIBotBot>(options =>
           {
               var secretKey = Configuration.GetSection("botFileSecret")?.Value;

               // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
               var botConfig = BotConfiguration.Load(@".\TaxiAIBot.bot", secretKey);
               services.AddSingleton(sp => botConfig);

               // Retrieve current endpoint.
               var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
               if (!(service is EndpointService endpointService))
               {
                   throw new InvalidOperationException($"The .bot file does not contain a development endpoint.");
               }

               options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

               // Catches any errors that occur during a conversation turn and logs them.
               options.OnTurnError = async (context, exception) =>
              {
                  await context.SendActivityAsync("Sorry, it looks like something went wrong." + exception.Message);
              };  
           });


            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);   
            services.AddSingleton<TaxiAIBotAccessors>(sp =>
            { 
                var accessors = new TaxiAIBotAccessors(conversationState, userState)
                {
                    DialogStateAccessor = conversationState.CreateProperty<DialogState>(StringHelper.PROP_STATE),
                    UserInfoAccessor = userState.CreateProperty<UserInfo>(StringHelper.PROP_USER_INFO)  ,
                    DidBotWelcomeUserAccessor = userState.CreateProperty<bool>(StringHelper.PROP_BOT_WELCOME_USER) 
                };                 
                return accessors;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
