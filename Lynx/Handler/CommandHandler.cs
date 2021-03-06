﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lynx.Methods;
using Lynx.Services.Embed;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NSFW;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Lynx.Database;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Lynx.Handler
{
    public class CommandHandler : LynxBase<LynxContext>
    {
        DiscordSocketClient Client;
        IServiceProvider provider;
        CommandService commands;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static LynxConfig LynxConfig = new LynxConfig();
        public CommandHandler(IServiceProvider Prov)
        {
            provider = Prov;
            Client = provider.GetService<DiscordSocketClient>();
            commands = provider.GetService<CommandService>();
            Client.MessageReceived += CurrencyHandler.PotentialCurrency;
            Client.MessageReceived += NSFWService.NSFWImplementation;
            Client.MessageReceived += CustomReactionHandler.CustomReactionService;
            Client.MessageReceived += HandleCommand;
        }
        public async Task ConfigureAsync(IServiceProvider Provider)
        {
            provider = Provider;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            logger.Info("Modules have been initialized.");
        }
                
        public async Task HandleCommand(SocketMessage Message)
        {
            var Config = LynxConfig.LoadConfig;
            Config.MessagesReceived++;
            await LynxConfig.SaveAsync(Config);
            int argPos = 0;
            var Guild = (Message.Channel as SocketTextChannel).Guild;
            if (!(Message as SocketUserMessage).HasStringPrefix(Guild.GetPrefix().ToLowerInvariant(), ref argPos)) return;
            var Context = new LynxContext(Client, Message as SocketUserMessage, provider);
            var Result = await commands.ExecuteAsync(Context, argPos, provider);
            Config.CommandsTriggered++;
            await LynxConfig.SaveAsync(Config);
            if (!Result.IsSuccess)
            {
                if (Config.Debug == true)
                {
                    await Context.Channel.SendMessageAsync(Result.ErrorReason);
                }
                else
                {
                    switch (Result.Error)
                    {
                        case CommandError.UnmetPrecondition:
                            await Message.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithFailedColor().WithDescription("Unknown precondition failed.").Build()); return;
                        case CommandError.Exception:
                            await Guild.GetLogChannel().SendMessageAsync("", embed: new EmbedBuilder().WithFailedColor().WithTitle("Exception occured in #" + (Message.Channel as SocketTextChannel).Name)
                                .WithDescription("**In Channel:** " + (Message.Channel as SocketTextChannel).Mention + $" [{Message.Channel.Id}]\n"
                                + "**Parameters: **" + Message.Content + "\n"
                                + "**Fired by:** " + Message.Author + $"\n**Exception**```{Result.ErrorReason}```").Build());
                            return;
                        case CommandError.BadArgCount:
                            await Guild.GetLogChannel().SendMessageAsync("", embed: new EmbedBuilder().WithFailedColor().WithTitle("Command failed in #" + (Message.Channel as SocketTextChannel).Name)
                            .WithDescription("**In Channel:** " + (Message.Channel as SocketTextChannel).Mention + $" [{Message.Channel.Id}]\n"
                            + "**Parameters: **" + Message.Content + "\n"
                            + "**Fired by:** " + Message.Author).Build());
                            return;
                    }
                }
            }
        }
    }
}
