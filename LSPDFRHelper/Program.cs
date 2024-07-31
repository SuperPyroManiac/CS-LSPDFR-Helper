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
        commandsExtension.AddCommands(typeof(AddPlugin), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(EditPlugin), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(RemovePlugin), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(FindPlugins), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(ExportPlugins), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(AddError), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(EditError), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(RemoveError), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(FindErrors), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(ExportErrors), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(EditUser), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(ForceVerification), BotSettings.Env.MainServ);
        commandsExtension.AddCommands(typeof(EditServer), BotSettings.Env.MainServ);
        
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
        commandsExtension.AddCommands(typeof(ForwardToAh), BotSettings.Env.MainServ);
        
        
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
