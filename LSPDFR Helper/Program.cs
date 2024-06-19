using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.CustomTypes.SpecialTypes;
using LSPDFR_Helper.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static LSPDFR_Helper.EventManagers.ModalSubmit;
using static LSPDFR_Helper.EventManagers.CompInteraction;
using static LSPDFR_Helper.EventManagers.MessageSent;
using static LSPDFR_Helper.EventManagers.OnJoinLeave;

namespace LSPDFR_Helper;

public class Program
{
    public static DiscordClient Client { get; set; }
    public static bool IsStarted { get; set; }
    public static Cache Cache = new();
    public static Settings BotSettings = new();
    public static GlobalSettings Settings = DbManager.GetGlobalSettings();
    
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
                    .HandleGuildMemberRemoved(LeaveEvent));
            
        Client = builder.Build();
        
        new ServiceCollection().AddLogging(x => x.AddConsole()).BuildServiceProvider();

        var commandsExtension = Client.UseCommands(new CommandsConfiguration());
        commandsExtension.AddCommands(Assembly.GetExecutingAssembly());
        TextCommandProcessor textCommandProcessor = new(new()
        { PrefixResolver = new DefaultPrefixResolver(false, ")(").ResolvePrefixAsync});
        await commandsExtension.AddProcessorsAsync(textCommandProcessor);
        
        Client.UseInteractivity(new InteractivityConfiguration());
        await Client.ConnectAsync(new DiscordActivity("with fire!", DiscordActivityType.Playing), DiscordUserStatus.DoNotDisturb);

        await Task.Delay(-1);
    }
    
    private static async Task Startup(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        //Startup Tasks
        await Functions.Startup.Init();
        
        //Allow Events
        IsStarted = true;
    }
}
