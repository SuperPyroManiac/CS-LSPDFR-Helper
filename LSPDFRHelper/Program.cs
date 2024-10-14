using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.Functions.WebAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static LSPDFRHelper.EventManagers.ModalSubmit;
using static LSPDFRHelper.EventManagers.CompInteraction;
using static LSPDFRHelper.EventManagers.MessageSent;
using static LSPDFRHelper.EventManagers.OnJoinLeave;
using Timer = LSPDFRHelper.Functions.Timer;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;

namespace LSPDFRHelper;

public class Program
{
    internal static DiscordClient Client { get; set; }
    internal static bool IsStarted { get; set; }
    internal static Cache Cache = new();
    internal static Settings BotSettings = new();

    private static async Task Main()
    {
        //Startup API Server
        _ = WebApiManager.Run();
        
        //Start Bot
         var builder = DiscordClientBuilder.CreateDefault(BotSettings.Env.BotToken, DiscordIntents.All);
         builder.ConfigureLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Error));
         new ServiceCollection().AddLogging(x => x.AddConsole()).BuildServiceProvider();

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
         
         builder.UseInteractivity(new InteractivityConfiguration());

        builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
        {
            extension.AddCommands(typeof(Program).Assembly);
        }, new CommandsConfiguration()
        {
            UseDefaultCommandErrorHandler = false
        });

         Client = builder.Build();
        
        new ServiceCollection().AddLogging(x => x.AddConsole()).BuildServiceProvider();

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
