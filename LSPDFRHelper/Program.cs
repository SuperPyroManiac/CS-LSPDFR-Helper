using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFRHelper.Commands.Case;
using LSPDFRHelper.Commands.ContextMenu;
using LSPDFRHelper.Commands.Error;
using LSPDFRHelper.Commands.Global;
using LSPDFRHelper.Commands.Plugin;
using LSPDFRHelper.Commands.User;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.Functions.AutoHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static LSPDFRHelper.EventManagers.ModalSubmit;
using static LSPDFRHelper.EventManagers.CompInteraction;
using static LSPDFRHelper.EventManagers.MessageSent;
using static LSPDFRHelper.EventManagers.OnJoinLeave;
using CloseCase = LSPDFRHelper.Commands.Case.CloseCase;
using JoinCase = LSPDFRHelper.Commands.Case.JoinCase;
using Timer = LSPDFRHelper.Functions.Timer;

namespace LSPDFRHelper;

public class Program
{
    public static DiscordClient Client { get; set; }
    public static bool IsStarted { get; set; }
    public static Cache Cache = new();
    public static Settings BotSettings = new();
    
    static async Task Main()
    {
        
        var builder = DiscordClientBuilder.CreateDefault(BotSettings.Env.BotToken, DiscordIntents.All);
        builder.SetLogLevel(LogLevel.Error);

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
        commandsExtension.AddCommands(typeof(AddPlugin), 736140566311600138);
        commandsExtension.AddCommands(typeof(EditPlugin), 736140566311600138);
        commandsExtension.AddCommands(typeof(RemovePlugin), 736140566311600138);
        commandsExtension.AddCommands(typeof(FindPlugins), 736140566311600138);
        commandsExtension.AddCommands(typeof(ExportPlugins), 736140566311600138);
        commandsExtension.AddCommands(typeof(AddError), 736140566311600138);
        commandsExtension.AddCommands(typeof(EditError), 736140566311600138);
        commandsExtension.AddCommands(typeof(RemoveError), 736140566311600138);
        commandsExtension.AddCommands(typeof(FindErrors), 736140566311600138);
        commandsExtension.AddCommands(typeof(ExportErrors), 736140566311600138);
        commandsExtension.AddCommands(typeof(EditUser), 736140566311600138);
        commandsExtension.AddCommands(typeof(ForceVerification), 736140566311600138);
        commandsExtension.AddCommands(typeof(EditServer), 736140566311600138);
        
        //Public Commands
        commandsExtension.AddCommands(typeof(Setup));
        commandsExtension.AddCommands(typeof(ToggleAh));
        commandsExtension.AddCommands(typeof(JoinCase));
        commandsExtension.AddCommands(typeof(CloseCase));
        commandsExtension.AddCommands(typeof(FindCases));
        commandsExtension.AddCommands(typeof(CheckPlugin));
        commandsExtension.AddCommands(typeof(ValidateLog));
        commandsExtension.AddCommands(typeof(ValidateXML));

        //WIP Commands
        commandsExtension.AddCommands(typeof(ForwardToAh), 736140566311600138);
        
        
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
    }
}
