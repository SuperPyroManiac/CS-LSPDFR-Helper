using System.Reflection;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Public.AutoHelper;
using MessageSent = ULSS_Helper.Events.MessageSent;

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
            MinimumLogLevel = LogLevel.Warning
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
        await Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));
        await Task.Run(CaseMonitor.UpdateMonitor);
        StatusMessages.SendStartupMessage();
	await Client.GetChannelAsync(600849173322924052).Result.SendMessageAsync("Soon To Be Case Viewer");
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
