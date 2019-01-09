// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxiAIBot.Accessors;
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
        private ILoggerFactory _loggerFactory;
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
            var secretKey = Configuration.GetSection("botFilePath")?.Value;
            var botConfig = BotConfiguration.Load(@".\TaxiAIBot.bot", secretKey);

           
            services.AddBot<TaxiAIBotBot>(options =>
           {
                
               services.AddSingleton(sp => botConfig);

               // Retrieve current endpoint.
               var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
               if (!(service is EndpointService endpointService))
               {
                   throw new InvalidOperationException($"The .bot file does not contain a development endpoint.");
               }

               options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

               ILogger logger = _loggerFactory.CreateLogger<TaxiAIBotBot>();

               logger.LogInformation("Bot is starting......");
               options.OnTurnError = async (context, exception) =>
              {
                  logger.LogError($"Exception caught : {exception}");
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
           
            services.AddSingleton(sp => InitBotServices(botConfig));

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }

        private static TaxiBotService InitBotServices(BotConfiguration config)
        {
            var qnaServices = new Dictionary<string, QnAMaker>();

            foreach (var service in config.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.QnA:
                        {
                            // Create a QnA Maker that is initialized and suitable for passing
                            // into the IBot-derived class (QnABot).
                            var qna = (QnAMakerService)service;
                            if (qna == null)
                            {
                                throw new InvalidOperationException("The QnA service is not configured correctly in your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.KbId))
                            {
                                throw new InvalidOperationException("The QnA KnowledgeBaseId ('kbId') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.EndpointKey))
                            {
                                throw new InvalidOperationException("The QnA EndpointKey ('endpointKey') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.Hostname))
                            {
                                throw new InvalidOperationException("The QnA Host ('hostname') is required to run this sample. Please update your '.bot' file.");
                            }

                            var qnaEndpoint = new QnAMakerEndpoint()
                            {
                                KnowledgeBaseId = qna.KbId,
                                EndpointKey = qna.EndpointKey,
                                Host = qna.Hostname,
                            };

                            var qnaMaker = new QnAMaker(qnaEndpoint);
                            qnaServices.Add(qna.Name, qnaMaker);

                            break;
                        }
                }
            }

            var connectedServices = new TaxiBotService(qnaServices);
            return connectedServices;
        }
    }
}
