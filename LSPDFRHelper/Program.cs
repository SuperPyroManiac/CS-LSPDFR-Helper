using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFRHelper.Commands;
using LSPDFRHelper.Commands.ContextMenu;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static LSPDFRHelper.EventManagers.ModalSubmit;
using static LSPDFRHelper.EventManagers.CompInteraction;
using static LSPDFRHelper.EventManagers.MessageSent;
using static LSPDFRHelper.EventManagers.OnJoinLeave;
using Timer = LSPDFRHelper.Functions.Timer;

namespace LSPDFRHelper;

public class Program
{
    public static DiscordClient Client { get; set; }
    public static bool IsStarted { get; set; }
    public static Cache Cache = new();
    public static Settings BotSettings = new();

    private static async Task Main()
    {
        //Startup API Server
        string[] prefixes = { "http://localhost:8055/", "http://www.pyrosfun.com:8055/" };
        var apiServ = new RemoteApi(prefixes);
        _ = apiServ.Start();
        
        //Start Bot
         var builder = DiscordClientBuilder.CreateDefault(BotSettings.Env.BotToken, DiscordIntents.All);
         builder.ConfigureLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Error));
        
         builder.ConfigureEventHandlers(
             e => e
                 .HandleGuildDownloadCompleted(Startup)
                 .HandleModalSubmitted(HandleModalSubmit)
                 .HandleComponentInteractionCreated(HandleInteraction)
                 .HandleMessageCreated(MessageSentEvent)
                 .HandleGuildMemberAdded(JoinEvent)
                 .HandleGuildMemberRemoved(LeaveEvent)
                 .HandleGuildCreated(GuildJoinEvent)
                 .HandleGuildDeleted(GuildLeaveEvent));
             
         Client = builder.Build();
        
        new ServiceCollection().AddLogging(x => x.AddConsole()).BuildServiceProvider();

        var cc = new CommandsConfiguration();
        cc.UseDefaultCommandErrorHandler = false;
        var commandsExtension = Client.UseCommands(cc);
        
        //Special Commands
        commandsExtension.AddCommands(typeof(Plugins), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(Errors), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(EditUser), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(ForceVerification), BotSettings.Env.MainServ);
        
        //Public Commands
        commandsExtension.AddCommands(typeof(Setup));
        commandsExtension.AddCommands(typeof(Cases));
        commandsExtension.AddCommands(typeof(ToggleAh));
        commandsExtension.AddCommands(typeof(CheckPlugin));
        commandsExtension.AddCommands(typeof(ValidateFiles));

        //WIP Commands
        //commandsExtension.AddCommands(typeof(EditServer), BotSettings.Env.MainServ);
        //commandsExtension.AddCommands(typeof(ForwardToAh), BotSettings.Env.MainServ);
        
        
        Client.UseInteractivity(new InteractivityConfiguration());
        await Client.ConnectAsync(new DiscordActivity("with fire!", DiscordActivityType.Playing), DiscordUserStatus.DoNotDisturb);

        await Task.Delay(-1);
    }
    
    private static async Task Startup(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        //Startup Tasks
        await Functions.Startup.Init();
        Timer.Start();
        
        //Allow Events
        IsStarted = true;

        Console.WriteLine("Successfully loaded all modules!");
    }
}
