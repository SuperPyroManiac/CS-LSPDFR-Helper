using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFR_Helper.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LSPDFR_Helper;

internal class Program
{
    internal static DiscordClient Client {get; set;}
    internal static bool IsStarted { get; set; }
    internal static Settings Settings = new();
    
    static async Task Main()
    {
        
        var builder = DiscordClientBuilder.CreateDefault(Settings.Env.BotToken, DiscordIntents.All);
        builder.SetLogLevel(LogLevel.Error);

        builder.ConfigureEventHandlers(
            e => e
                .HandleGuildDownloadCompleted(WaitForStartup));
                // .HandleModalSubmitted(HandleModalSubmit)
                // .HandleComponentInteractionCreated(HandleInteraction)
                // .HandleMessageCreated(MessageSentEvent)
                // .HandleGuildMemberAdded(JoinEvent)
                // .HandleGuildMemberRemoved(LeaveEvent));
            
        Client = builder.Build();
        
        new ServiceCollection().AddLogging(x => x.AddConsole()).BuildServiceProvider();

        var commandsExtension = Client.UseCommands(new CommandsConfiguration());
        commandsExtension.AddCommands(Assembly.GetExecutingAssembly(), 1166534357792600155);//TODO: CHANGE TO SETTINGS
        TextCommandProcessor textCommandProcessor = new(new()
        { PrefixResolver = new DefaultPrefixResolver(false, ")(").ResolvePrefixAsync});
        await commandsExtension.AddProcessorsAsync(textCommandProcessor);
        Client.UseInteractivity(new InteractivityConfiguration());
        await Client.ConnectAsync(new DiscordActivity("with fire!", DiscordActivityType.Playing), DiscordUserStatus.DoNotDisturb);

        while (!IsStarted) await Task.Delay(500);
        DbManager.GetGlobalSettings();
        await Task.Delay(-1);
    }
    
    private static Task WaitForStartup(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        IsStarted = true;
        return Task.CompletedTask;
    }
}
