using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ULSS_Helper.Messages;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;
using static ULSS_Helper.Events.ComponentInteraction;
using static ULSS_Helper.Events.JoinLeave;
using static ULSS_Helper.Events.MessageSent;
using static ULSS_Helper.Events.ModalSubmit;

namespace ULSS_Helper;

public class Program
{
    public static DiscordClient Client {get; set;}
    public static Settings Settings = new();
    public static Cache Cache = new();
    public static bool isStarted;
    
    static async Task Main()
    {
        Cache.UpdatePlugins(Database.LoadPlugins());
        Cache.UpdateErrors(Database.LoadErrors());
        Cache.UpdateCases(Database.LoadCases());
        Cache.UpdateUsers(Database.LoadUsers());
        
        var builder = DiscordClientBuilder.CreateDefault(Settings.Env.BotToken, DiscordIntents.All);
        builder.SetLogLevel(LogLevel.Error);

        builder.ConfigureEventHandlers(
            e => e
                .HandleGuildDownloadCompleted(WaitForStartup)
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

        while (!isStarted) await Task.Delay(500);
        Timer.StartTimer();
        await Task.Run(Startup.StartAutoHelper);
        await StatusMessages.SendStartupMessage();
        await Task.Delay(-1);
    }

    public static DiscordGuild GetGuild()
    {
        return Client.Guilds[Settings.Env.ServerId];
    }

    public static async Task<DiscordMember> GetMember(string uid)
    {
        var serv = await Client.GetGuildAsync(Settings.Env.ServerId);
        return await serv.GetMemberAsync(ulong.Parse(uid));
    }
    
    private static Task WaitForStartup(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        isStarted = true;
        return Task.CompletedTask;
    }
}
