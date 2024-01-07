using System.Reflection;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;

namespace ULSS_Helper;

internal class Program
{
    internal static DiscordClient Client {get; set;}
    internal static Settings Settings;
    internal static Cache Cache = new();
    
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
            MinimumLogLevel = LogLevel.Trace
        };
        Client = new DiscordClient(discordConfig);
        
        var sCommands = Client.UseSlashCommands();

        sCommands.RegisterCommands(Assembly.GetExecutingAssembly(), Settings.Env.ServerId);
        sCommands.RegisterCommands<ContextMenu>(Settings.Env.ServerId);
        sCommands.AutocompleteErrored += Oops();

        Client.ModalSubmitted += ModalSubmit.HandleModalSubmit;
        Client.ComponentInteractionCreated += ComponentInteraction.HandleInteraction;
        Client.MessageCreated += MessageSent.MessageSentEvent;
        Client.GuildMemberAdded += JoinLeave.JoinEvent;
        Client.GuildMemberRemoved += JoinLeave.LeaveEvent;
        //Client.VoiceStateUpdated += VoiceChatManager.OnMemberJoinLeaveVC;

        Client.UseInteractivity(new InteractivityConfiguration());

        await Client.ConnectAsync();
        StatusMessages.SendStartupMessage();
        await Task.Delay(-1);
    }

    private static AsyncEventHandler<SlashCommandsExtension, AutocompleteErrorEventArgs> Oops()
    {
        return (s, e) =>
        {
	        Console.WriteLine(e.Exception);
	        return Task.CompletedTask;
        };
    }
}
