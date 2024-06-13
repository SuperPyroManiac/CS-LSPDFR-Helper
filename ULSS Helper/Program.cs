using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;
using MessageSent = ULSS_Helper.Events.MessageSent;

namespace ULSS_Helper;

internal class Program
{
    internal static DiscordClient Client {get; set;}
    internal static Settings Settings = new();
    internal static Cache Cache = new();
    
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
                .HandleModalSubmitted(ModalSubmit.HandleModalSubmit)
                .HandleComponentInteractionCreated(ComponentInteraction.HandleInteraction)
                .HandleMessageCreated(MessageSent.MessageSentEvent)
                .HandleGuildMemberAdded(JoinLeave.JoinEvent)
                .HandleGuildMemberRemoved(JoinLeave.LeaveEvent));
            
        Client = builder.Build();
        
        new ServiceCollection().AddLogging(x => x.AddConsole()).BuildServiceProvider();

        var commandsExtension = Client.UseCommands(new CommandsConfiguration());
        commandsExtension.AddCommands(Assembly.GetExecutingAssembly(), Settings.Env.ServerId);
        TextCommandProcessor textCommandProcessor = new(new()
        { PrefixResolver = new DefaultPrefixResolver(false, ")(").ResolvePrefixAsync});//TODO: Remove this entirely.
        await commandsExtension.AddProcessorsAsync(textCommandProcessor);
        Client.UseInteractivity(new InteractivityConfiguration());

        await Client.ConnectAsync(new DiscordActivity("with fire!", DiscordActivityType.Playing), DiscordUserStatus.DoNotDisturb);
        Timer.StartTimer();
        await Task.Delay(1000);
        await Task.Run(Startup.StartAutoHelper);
        await StatusMessages.SendStartupMessage();
        await Task.Delay(-1);
    }

    internal static DiscordGuild GetGuild()
    {
        return Client.Guilds[Settings.Env.ServerId];
    }

    internal static async Task<DiscordMember> GetMember(string uid)
    {
        var serv = await Client.GetGuildAsync(Settings.Env.ServerId);
        return await serv.GetMemberAsync(ulong.Parse(uid));
    }
}
