using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
using LSPDFRHelper.Commands.ContextMenu;
using LSPDFRHelper.Commands;

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
        builder.UseInteractivity();
        builder.UseCommands((_, extension) =>
        {
            extension.AddCommand(typeof(ValidateFiles));
            extension.AddCommand(typeof(Cases));
            extension.AddCommand(typeof(CheckPlugin));
            extension.AddCommand(typeof(Setup));
            extension.AddCommand(typeof(ToggleAh));

            extension.AddCommand(typeof(EditUser), BotSettings.Env.MainServ);
            extension.AddCommand(typeof(Errors), BotSettings.Env.MainServ);
            extension.AddCommand(typeof(Plugins), BotSettings.Env.MainServ);
            extension.AddCommand(typeof(ForceVerification), BotSettings.Env.MainServ);
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
