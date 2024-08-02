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
        Console.WriteLine("Registering AddPlugin");
        commandsExtension.AddCommand(typeof(AddPlugin), BotSettings.Env.MainServ);
        Console.WriteLine("Registering EditPlugin");
        commandsExtension.AddCommand(typeof(EditPlugin), BotSettings.Env.MainServ);
        Console.WriteLine("Registering RemovePlugin");
        commandsExtension.AddCommand(typeof(RemovePlugin), BotSettings.Env.MainServ);
        Console.WriteLine("Registering FindPlugins");
        commandsExtension.AddCommand(typeof(FindPlugins), BotSettings.Env.MainServ);
        Console.WriteLine("Registering ExportPlugins");
        commandsExtension.AddCommand(typeof(ExportPlugins), BotSettings.Env.MainServ);
        Console.WriteLine("Registering AddError");
        commandsExtension.AddCommand(typeof(AddError), BotSettings.Env.MainServ);
        Console.WriteLine("Registering EditError");
        commandsExtension.AddCommand(typeof(EditError), BotSettings.Env.MainServ);
        Console.WriteLine("Registering RemoveError");
        commandsExtension.AddCommand(typeof(RemoveError), BotSettings.Env.MainServ);
        Console.WriteLine("Registering FindErrors");
        commandsExtension.AddCommand(typeof(FindErrors), BotSettings.Env.MainServ);
        Console.WriteLine("Registering ExportErrors");
        commandsExtension.AddCommand(typeof(ExportErrors), BotSettings.Env.MainServ);
        Console.WriteLine("Registering EditUser");
        commandsExtension.AddCommand(typeof(EditUser), BotSettings.Env.MainServ);
        Console.WriteLine("Registering ForceVerification");
        commandsExtension.AddCommand(typeof(ForceVerification), BotSettings.Env.MainServ);
        
        //Public Commands
        Console.WriteLine("Registering Setup");
        commandsExtension.AddCommand(typeof(Setup));
        Console.WriteLine("Registering ToggleAh");
        commandsExtension.AddCommand(typeof(ToggleAh));
        Console.WriteLine("Registering JoinCase");
        commandsExtension.AddCommand(typeof(JoinCase));
        Console.WriteLine("Registering CloseCase");
        commandsExtension.AddCommand(typeof(CloseCase));
        Console.WriteLine("Registering FindCases");
        commandsExtension.AddCommand(typeof(FindCases));
        Console.WriteLine("Registering CheckPlugin");
        commandsExtension.AddCommand(typeof(CheckPlugin));
        Console.WriteLine("Registering ValidateLog");
        commandsExtension.AddCommand(typeof(ValidateLog));
        Console.WriteLine("Registering ValidateXML");
        commandsExtension.AddCommand(typeof(ValidateXML));

        //WIP Commands
        //commandsExtension.AddCommand(typeof(EditServer), BotSettings.Env.MainServ);
        //commandsExtension.AddCommand(typeof(ForwardToAh), BotSettings.Env.MainServ);
        
        
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
