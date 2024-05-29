﻿using System.Reflection;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;
using MessageSent = ULSS_Helper.Events.MessageSent;

namespace ULSS_Helper;

internal class Program
{
    internal static DiscordClient Client {get; set;}
    internal static Settings Settings;
    internal static Cache Cache = new();
    internal static bool StartupMsg = false;
    
    static async Task Main()
    {
        Timer.StartTimer();

        Settings = new Settings();
        
        var discordConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            Token = Settings.Env.BotToken,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Error
        };
        Client = new DiscordClient(discordConfig);
        
        var sCommands = Client.UseSlashCommands();

        sCommands.RegisterCommands(Assembly.GetExecutingAssembly(), Settings.Env.ServerId);
        sCommands.RegisterCommands<ContextMenu>(Settings.Env.ServerId);

        Client.ModalSubmitted += ModalSubmit.HandleModalSubmit;
        Client.ComponentInteractionCreated += ComponentInteraction.HandleInteraction;
        Client.MessageCreated += MessageSent.MessageSentEvent;
        Client.GuildMemberAdded += JoinLeave.JoinEvent;
        Client.GuildMemberRemoved += JoinLeave.LeaveEvent;
        //Client.VoiceStateUpdated += VoiceChatManager.OnMemberJoinLeaveVC;
        Client.UseInteractivity(new InteractivityConfiguration());

        await Client.ConnectAsync();
        Cache.UpdatePlugins(Database.LoadPlugins());
        Cache.UpdateErrors(Database.LoadErrors());
        Cache.UpdateCases(Database.LoadCases());
        Cache.UpdateUsers(Database.LoadUsers());
        await Task.Run(Startup.StartAutoHelper);
        await StatusMessages.SendStartupMessage();
        await Task.Delay(-1);
    }

    internal static async Task<DiscordMember> GetMember(string uid)
    {
        var serv = await Client.GetGuildAsync(Settings.Env.ServerId);
        return await serv.GetMemberAsync(ulong.Parse(uid));
    }
}
